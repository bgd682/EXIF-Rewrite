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
    }
}
