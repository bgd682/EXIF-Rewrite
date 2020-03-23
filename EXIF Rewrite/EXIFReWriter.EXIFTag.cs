namespace EXIFRewrite
{
    partial class EXIFReWriter
    {
        /// <summary>
        /// Exif tags that support being re-written
        /// </summary>
        public enum EXIFTag
        {
            Ignored = 0,// Default state
            FileName=-1,
            UserComment = 0x9286,
            DateTime=0x0132,
            DateTimeOriginal=0x9003,
            //DateTimeDigitized=0x9004,
            GPSLatitude = 0x002,
            GPSLongitude = 0x0004,
            GPSAltitude = 0x0006,
            GPSAltitudeReference = 0x0005,
        }
        public static string EXIFTagToString(EXIFTag tag)
        {
            switch (tag)
            {
                case EXIFTag.Ignored: return "Ignored";
                case EXIFTag.FileName: return "File Name";
                case EXIFTag.UserComment: return "User Comment";
                case EXIFTag.DateTime: return "Date & Time";
                case EXIFTag.DateTimeOriginal: return "Date & Time Original";
               // case EXIFTag.DateTimeDigitized: return "Date & Time Digitized";
                case EXIFTag.GPSLatitude: return "GPS Latitude";
                case EXIFTag.GPSLongitude: return "GPS Longitude";
                case EXIFTag.GPSAltitude: return "GPS Altitude (m)";
                case EXIFTag.GPSAltitudeReference: return "GPS Altitude Ref";
            }
            return ""; // fallthrough
        }
    }
}
