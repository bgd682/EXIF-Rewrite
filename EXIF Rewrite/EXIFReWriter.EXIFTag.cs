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
            MakerNote,
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
                case EXIFTag.MakerNote: return "MakerNote";
                case EXIFTag.UserComment: return "UserComment";
                case EXIFTag.DateTime: return "DateTime";
                case EXIFTag.DateTimeOriginal: return "DateTimeOriginal";
                case EXIFTag.DateTimeDigitized: return "DateTimeDigitized";
                case EXIFTag.GPSLatitude: return "GPSLatitude";
                case EXIFTag.GPSLongitude: return "GPSLongitude";
                case EXIFTag.GPSAltitude: return "GPSAltitude";
                case EXIFTag.GPSTimeStamp: return "GPSTimeStamp";
            }
            return "Ignored"; // fallthrough
        }
    }
}
