using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static EXIF_Rewrite.CSVTags;

public static class Extensions
{
    public enum MetaProperty
    {
        Title = 40091,
        Comment = 40092,
        Author = 40093,
        Keywords = 40094,
        Subject = 40095,
        Copyright = 33432,
        Software = 11,
        DateTime = 36867
    }
    public static Bitmap SetMetaValue(this Bitmap sourceBitmap, MetaProperty property, string value)
    {
        if (sourceBitmap == null)
        {
            throw new ArgumentException("Argument cannot be null", nameof(sourceBitmap));
        }
        PropertyItem prop = sourceBitmap.PropertyItems[0];
        int iLen = value.Length + 1;
        byte[] bTxt = new Byte[iLen];
        for (int i = 0; i < iLen - 1; i++)
            bTxt[i] = (byte)value[i];
        bTxt[iLen - 1] = 0x00;
        prop.Id = (int)property;
        prop.Type = 2;
        prop.Value = bTxt;
        prop.Len = iLen;
        sourceBitmap.SetPropertyItem(prop);
        return sourceBitmap;
    }

    public static string GetMetaValue(this Bitmap sourceBitmap, MetaProperty property)
    {
        if (sourceBitmap == null)
        {
            throw new ArgumentException("Argument cannot be null", nameof(sourceBitmap));
        }
        PropertyItem[] propItems = sourceBitmap.PropertyItems;
        var prop = propItems.FirstOrDefault(p => p.Id == (int)property);
        if (prop != null)
        {
            return Encoding.UTF8.GetString(prop.Value);
        }
        else
        {
            return null;
        }
    }

}
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
            var imageIn = (Bitmap)Bitmap.FromFile(fileNameIn);
            imageIn.SetMetaValue(Extensions.MetaProperty.Comment, "Test");
            

            return true;
        }

    }

    
}
