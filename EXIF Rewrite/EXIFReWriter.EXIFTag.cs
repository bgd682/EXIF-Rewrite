namespace EXIF_Rewrite
{
    partial class EXIFReWriter
    {
        /// <summary>
        /// Exif tags that support being re-written
        /// </summary>
        public enum EXIFTag
        {
            Ignored,// Default state
            FileName,
            UserComment,
            DateTime,
            DateTimeOriginal,
            DateTimeDigitized,
            GPSLatitude,
            GPSLongitude,
            GPSAltitude,
            GPSTimeStamp,

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
                case EXIFTag.DateTimeDigitized: return "Date & Time Digitized";
                case EXIFTag.GPSLatitude: return "GPS Latitude";
                case EXIFTag.GPSLongitude: return "GPS Longitude";
                case EXIFTag.GPSAltitude: return "GPS Altitude";
                case EXIFTag.GPSTimeStamp: return "GPS Timestamp";
            }
            return "Ignored"; // fallthrough
        }
    }
}
