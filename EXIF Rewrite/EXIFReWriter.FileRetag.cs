using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace EXIFRewrite
{
    partial class EXIFReWriter
    {
        class FileRetag
        {

            public static ReTagError ReTagImage(string fileNameIn, string fileNameOut, List<UpdateMetaPair> tags)
            {
                ReTagError err;
                Image imageIn;
                try
                {
                    imageIn = Image.FromFile(fileNameIn, true);
                }
                catch (Exception e)
                {
                    err.errorMessage = e.Message;
                    err.failingFile = fileNameIn;
                    return err;
                }
                foreach (var t in tags)
                {
                    if (!AddModifyTag(imageIn, t.tag, t.value))
                    {
                        err.errorMessage = "Failed Parsing tag " + EXIFTagToString(t.tag);
                        err.failingFile = fileNameIn;
                        return err;
                    }
                }
                //Do not allow overwrite
                if (fileNameOut == fileNameIn)
                {
                    err.errorMessage = "Output cannot be same as input file";
                    err.failingFile = fileNameIn;
                    return err;
                }
                //Save out the updated image
                imageIn.Save(fileNameOut);
                err.errorMessage = "";
                err.failingFile = "";
                return err;
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
                        return AddModifyRational(img, value, tag);
                    case EXIFTag.GPSAltitudeReference:
                        return AddModifyAltitudeRef(img, value, tag);
                    case EXIFTag.UserComment:
                    case EXIFTag.DateTime:
                    case EXIFTag.DateTimeOriginal:
                        return AddModifyTag(img, tag, Encoding.ASCII.GetBytes(value.ToCharArray()), EXIFTypes.ASCII);
                    default:
                        return false;
                }
                return false; // unhandled type encountered
            }
            private static bool AddModifyAltitudeRef(Image img, string value, EXIFTag tag)
            {
                value = value.ToLower();
                byte parsedValue = 0;
                if (value.Contains("above") || value.Contains("0"))
                {

                    parsedValue = 0;
                }
                else if (value.Contains("below") || value.Contains("1"))
                {
                    parsedValue = 1;
                }
                else
                {
                    return false;
                }
                return AddModifyTag(img, tag, new byte[] { parsedValue }, EXIFTypes.Byte);
            }
            private static bool AddModifyRational(Image img, string value, EXIFTag tag)
            {
                //strip useless chars
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
            private static byte[] ConvertFloatToRational(float value)
            {
                //Split this float into a value represented by two uint32_t numbers
                var f = new Fraction(value, UInt32.MaxValue - 1);
                // use bitconverter to get the two LE byte sets
                return BitConverter.GetBytes((UInt32)(f.Numerator)).Concat(BitConverter.GetBytes((UInt32)(f.Denominator))).ToArray();
            }

            private enum EXIFTypes
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


}
