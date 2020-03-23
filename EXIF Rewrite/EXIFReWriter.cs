using Mehroz;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using static EXIFRewrite.CSVTags;

namespace EXIFRewrite
{
    partial class EXIFReWriter
    {
       
        public struct ReTagError
        {
            public string errorMessage;
            public string failingFile;
        }
        public delegate void StatusUpdateHandler(object sender, float percentDone);
        public event StatusUpdateHandler OnUpdateStatus;


        public delegate void FinishHandler(object sender, bool completedWithoutErrors, ReTagError failingMessage);
        public event FinishHandler OnFinish;

        struct UpdateMetaPair
        {
            public EXIFTag tag;
            public string value;
        }
        public void rewriteTags(string[] images, string outputFolder, List<ColumnData> tags)
        {
            ReTagError err;
            //https://dejanstojanovic.net/aspnet/2014/november/adding-extra-info-to-an-image-file/
            //For each provided file, find matching row in the ColumnData, the read,modify,write
            //Do not update image if dest == source
            var fileNameColumn = tags.Where(c => c.ColumnTag == EXIFTag.FileName).ToArray();
            if (fileNameColumn.Length != 1)
            {

                err.errorMessage = "No file name mapping column selected";
                err.failingFile = "";
                OnFinish?.Invoke(this, false, err);
                return;
            }
            var tagsToUpdate = tags.Where(c => c.ColumnTag != EXIFTag.Ignored && c.ColumnTag != EXIFTag.FileName).ToArray();
            for (int index = 0; index < images.Length; index++)
            {

                var fileName = "";
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(images[index]);
                    fileName = fi.Name;
                }
                var itemRow = fileNameColumn[0].cells.FindIndex(fName => fName == fileName);
                if (itemRow == -1)
                {
                    // no matches

                    err.errorMessage = "Provided Image could not be matched";
                    err.failingFile = fileName;
                    OnFinish?.Invoke(this, false, err);
                    return;
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

                err = FileRetag.ReTagImage(images[index], System.IO.Path.Combine(outputFolder, fileName), updatedTags);
                if (err.errorMessage.Length > 0)
                {

                    OnFinish?.Invoke(this, false, err);
                    return;
                }

                OnUpdateStatus?.Invoke(this, ((float)index / (float)images.Length) * 100);
                //Every so often, run a GC pass over the data
                if (index % 3 == 0)
                {
                    GC.Collect();
                }
            }
            err.errorMessage = "";
            err.failingFile = "";
            OnFinish?.Invoke(this, true, err);
        }

    }


}
