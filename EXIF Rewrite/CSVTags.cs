using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EXIF_Rewrite
{
    /// <summary>
    /// Represets the contents of a CSV file into EXIF Tags
    /// </summary>
    class CSVTags
    {
        public struct ColumnData
        {
            public string ColumnName;
            public EXIFReWriter.EXIFTag ColumnTag;
            public List<string> cells;
        }
        public void Reset()
        {

        }
        public List<ColumnData> parsedColumns;
        public bool Parse(string FileName)
        {
            // Load the provided file into a line buffer
            try
            {
                using (StreamReader sr = new StreamReader(FileName))
                {
                    //csv's are often tiny so just read it all into ram in one blob operation
                    string file = sr.ReadToEnd();
                    if (file.Length < 20)
                    {
                        Console.WriteLine("csv too short");
                        return false;
                    }
                    file = file.Replace("\r", "\n");

                    string[] lines = file.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length < 2)
                    {
                        Console.WriteLine("Not enough rows in csv");
                        return false;
                    }
                    Console.WriteLine("CSV Header Row ~> %s", lines[0]);
                    string[] headings = lines[0].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    parsedColumns = new List<ColumnData> { };
                    foreach (string header in headings)
                    {
                        ColumnData c = new ColumnData
                        {
                            ColumnName = header,
                            cells = new List<string> { }
                        };
                        parsedColumns.Add(c);
                    }

                    Console.WriteLine("Parsing %d Lines", lines.Length - 1);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        //Split line into columns, and assign all of these out
                        var lineCols = lines[i].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        int column = 0;
                        for (; column < lineCols.Length && column < parsedColumns.Count; column++)
                        {
                            parsedColumns[column].cells.Add(lineCols[column].Trim());
                        }
                        //Add any blank filling
                        for (; column < parsedColumns.Count; column++)
                        {
                            Console.WriteLine("Passing Column %d on line %d", column, i);
                            parsedColumns[column].cells.Add("");
                        }

                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The File could not be read:");
                Console.WriteLine(e.Message);
                return false;
            }
            // Read first line as headings
            // Read all subsequent lines as data for said headings

        }
    }
}
