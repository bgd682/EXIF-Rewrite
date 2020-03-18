using Mehroz;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static EXIF_Rewrite.CSVTags;

namespace EXIF_Rewrite
{
    partial class EXIFReWriter
    {
        struct UpdateMetaPair
        {
            public EXIFTag tag;
            public string value;
        }
        public static bool rewriteTags(string[] images, string outputFolder, List<ColumnData> tags)
        {
            //https://dejanstojanovic.net/aspnet/2014/november/adding-extra-info-to-an-image-file/
            //For each provided file, find matching row in the ColumnData, the read,modify,write
            //Do not update image if dest == source
            var fileNameColumn = tags.Where(c => c.ColumnTag == EXIFTag.FileName).ToArray();
            if (fileNameColumn.Length != 1)
            {
                //TODO Message alert
                return false;
            }
            var tagsToUpdate = tags.Where(c => c.ColumnTag != EXIFTag.Ignored && c.ColumnTag != EXIFTag.FileName).ToArray();
            foreach (var filePath in images)
            {
                var fileName = "";
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                    fileName = fi.Name;
                }
                var itemRow = fileNameColumn[0].cells.FindIndex(fName => fName == fileName);
                if (itemRow == -1)
                {
                    // no matches
                    return false;
                }
                //Update file
                List<UpdateMetaPair> updatedTags = new List<UpdateMetaPair> { };
                foreach (ColumnData c in tagsToUpdate)
                {
                    updatedTags.Add(new UpdateMetaPair
                    {
                        tag = c.ColumnTag,
                        value = c.cells[itemRow]
                    });
                }
                //calculate output filename as rebasing base onto the output folder
                if (!ReTagImage(filePath, System.IO.Path.Combine(outputFolder, fileName), updatedTags))
                {
                    return false;
                }

            }
            return true;
        }

        private static bool ReTagImage(string fileNameIn, string fileNameOut, List<UpdateMetaPair> tags)
        {
            var imageIn = Bitmap.FromFile(fileNameIn);

            foreach (var t in tags)
            {
                AddModifyTag(imageIn, t.tag, t.value);
            }
            //Do not allow overwrite
            if (fileNameOut == fileNameIn)
                return false;
            //Save out the updated image
            imageIn.Save(fileNameOut);
            return true;
        }
        public static bool AddModifyTag(Image img, EXIFTag tag, string value)
        {
            //using the tag type, decode how to parse string -> bytes
            switch (tag)
            {
                case EXIFTag.GPSLatitude:
                case EXIFTag.GPSLongitude:
                    return AddModifyLongLat(img, value, tag);
                case EXIFTag.GPSAltitude:
                    AddModifyRational(img, value, tag);
                    break;
                case EXIFTag.UserComment:
                case EXIFTag.DateTime:
                case EXIFTag.DateTimeOriginal:
                default:
                    return AddModifyTag(img, tag, Encoding.ASCII.GetBytes(value.ToCharArray()), EXIFTypes.ASCII);
            }
            return false; // unhandled type encountered
        }

        private static bool AddModifyRational(Image img, string value, EXIFTag tag)
        {
            value = new string(value.Where(c => char.IsDigit(c) || c == '.').ToArray());

            float sourceVal = float.Parse(value, System.Globalization.NumberStyles.Float);
            var results = ConvertFloatToRational(sourceVal);
            return AddModifyTag(img, tag, results, EXIFTypes.rational);
        }
        private static bool AddModifyLongLat(Image img, string value, EXIFTag tag)
        {
            //parse the provided dd.mmmm or dd.mmm.sss format into a float
            float deg = 0, min = 0, sec = 0;
            if (value.Contains("'"))
            {
                //DMS
                var value2 = value.Replace("'", "").Replace("°", "").Replace("\"", "").Replace("-", "");
                var split = value2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3 || split.Length == 4)
                {
                    //We are good
                    deg = float.Parse(split[0], System.Globalization.NumberStyles.Float);
                    min = float.Parse(split[1], System.Globalization.NumberStyles.Float);
                    sec = float.Parse(split[2], System.Globalization.NumberStyles.Float);

                }
            }
            else
            {
                //Deg decimal
                // TODO NEEDS Testing
                var value2 = value.Replace("'", "").Replace("°", "").Replace("\"", "").Replace("-", "");
                float source = float.Parse(value2);
                deg = (int)(source);
                source -= deg;
                source *= 60;
                min = (int)source;
                source -= min;
                source *= 60;
                sec = source;
            }
            string directionSign = "";
            if (value.Contains("S") || value.Contains("-") || value.Contains("W"))
            {
                if (tag == EXIFTag.GPSLatitude)
                {
                    directionSign = "S";
                }
                else if (tag == EXIFTag.GPSLongitude)
                {
                    directionSign = "W";
                }
            }
            else
            {
                if (tag == EXIFTag.GPSLatitude)
                {
                    directionSign = "N";
                }
                else if (tag == EXIFTag.GPSLongitude)
                {
                    directionSign = "W";
                }
            }
            if (tag == EXIFTag.GPSLatitude)
            {
                //GPSLatitudeRef = 0x001,
                if (AddModifyTag(img, (EXIFTag)0x0001, Encoding.ASCII.GetBytes(directionSign), EXIFTypes.ASCII) == false)
                {
                    return false;
                }
            }
            else if (tag == EXIFTag.GPSLongitude)
            {
                //GPSLongitudeRef = 0x0003,
                if (AddModifyTag(img, (EXIFTag)0x0003, Encoding.ASCII.GetBytes(directionSign), EXIFTypes.ASCII) == false)
                {
                    return false;
                }

            }
            //Convert this float into the byte[] array desired for 3 rationals


            var results = ConvertFloatToRational(deg).Concat(ConvertFloatToRational(min).Concat(ConvertFloatToRational(sec))).ToArray();

            return AddModifyTag(img, tag, results, EXIFTypes.rational);
        }
        static byte[] ConvertFloatToRational(float value)
        {
            //Split this float into a value represented by two uint32_t numbers
            var f = new Fraction(value, UInt32.MaxValue - 1);
            // use bitconverter to get the two LE byte sets
            return BitConverter.GetBytes((UInt32)(f.Numerator)).Concat(BitConverter.GetBytes((UInt32)(f.Denominator))).ToArray();
        }

        enum EXIFTypes
        {
            Unused = 0,
            Byte = 1,
            Undefined = 7,
            ASCII = 2,
            uint16 = 3,
            uint32 = 4,
            rational = 5,//two int32's
            int32 = 9,
            sRational = 10

        }
        private static EXIFTypes TagToType(EXIFTag tag)
        {
            switch (tag)
            {
                case EXIFTag.Ignored: return EXIFTypes.Undefined;
                case EXIFTag.FileName: return EXIFTypes.ASCII;
                case EXIFTag.UserComment: return EXIFTypes.ASCII;
                case EXIFTag.DateTime: return EXIFTypes.ASCII;
                case EXIFTag.DateTimeOriginal: return EXIFTypes.ASCII;
                // case EXIFTag.DateTimeDigitized: return "Date & Time Digitized";
                case EXIFTag.GPSLatitude: return EXIFTypes.rational;
                case EXIFTag.GPSLongitude: return EXIFTypes.rational;
                case EXIFTag.GPSAltitude: return EXIFTypes.rational;
            }
            return EXIFTypes.Undefined; // fallthrough
        }
        private static bool AddModifyTag(Image img, EXIFTag tag, byte[] value, EXIFTypes type)
        {
            // Read - modify - write
            if ((int)tag > 0)
            {
                var existingTag = img.GetPropertyItem((int)tag);
                existingTag.Id = (int)tag;
                existingTag.Type = (short)type;
                existingTag.Value = value;
                existingTag.Len = value.Length;
                img.SetPropertyItem(existingTag);
            }

            return true;
        }

    }


}
