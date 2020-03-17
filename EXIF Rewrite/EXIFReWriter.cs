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
                if (!ReTagImage(filePath, filePath, updatedTags))
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
            if (fileNameOut == fileNameIn)
                return false;
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
                    break;
                case EXIFTag.GPSTimeStamp:
                    break;
                case EXIFTag.UserComment:
                case EXIFTag.DateTime:
                case EXIFTag.DateTimeOriginal:
                default:
                    return AddModifyTag(img, tag, Encoding.ASCII.GetBytes(value.ToCharArray()), EXIFTypes.ASCII);
            }
            return false; // unhandled type encountered
        }

        private static bool AddModifyLongLat(Image img, string value, EXIFTag tag)
        {
            //parse the provided dd.mmmm or dd.mmm.sss format into a float
            float deg = 0, min = 0, sec = 0;
            if (value.Contains("'"))
            {
                //DMS
                value = value.Replace("'", "").Replace("°", "").Replace("\"", "").Replace("-", "");
                var split = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

            }
            if (value.Contains("S") || value.Contains("-") || value.Contains("W"))
            {
                deg = (0 - deg);
            }
            //Convert this float into the byte[] array desired for 3 rationals
            // https://www.codeproject.com/Articles/9078/Fraction-class-in-C
            //Using Fraction class to split the 3 floats into parts
            Fraction degF = new Fraction(deg);
            Fraction minF = new Fraction(min);
            Fraction secF = new Fraction(sec);
            //These are stored as a series of three pairs of uint32's
            UInt32[] payloadUInts = new UInt32[] { (UInt32)degF.Numerator, (UInt32)degF.Denominator, (UInt32)minF.Numerator, (UInt32)minF.Denominator, (UInt32)secF.Numerator, (UInt32)secF.Denominator };
            //Covert this to a LE byte[]
            //BitConverter.GetBytes()
            var results = FlatternDoubleByteArray(payloadUInts.Select(var => BitConverter.GetBytes(var)).ToArray());

            //var existingTag = img.GetPropertyItem((int)EXIFTag.GPSLatitude);
            return AddModifyTag(img, tag, results, EXIFTypes.rational);
        }
        static byte[] FlatternDoubleByteArray(byte[][] array)
        {
            byte[] tmp = new byte[array.GetLength(0) * array.GetLength(1)];
            Buffer.BlockCopy(array, 0, tmp, 0, tmp.Length * sizeof(byte));
            return (tmp);
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
                case EXIFTag.GPSTimeStamp: return EXIFTypes.rational;
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
