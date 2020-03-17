
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;

namespace EXIF_Rewrite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private CSVTags cSVTags = new CSVTags();
        private List<string> filesToBeTagged = new List<string> { };
        private string outputFolderPath = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load in any predefinied options
            string[] customOperations = { "Not Yet Implemented" };

            foreach (var x in customOperations)
            {
                cbCustomOperations.Items.Add(x);
            }
            if (customOperations.Length > 0)
            {
                cbCustomOperations.SelectedIndex = 0;
            }
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            EXIFReWriter.rewriteTags(filesToBeTagged.ToArray(), outputFolderPath, cSVTags.parsedColumns);


        }

        private void btnLoadImages_Click(object sender, RoutedEventArgs e)
        {
            //Need to load in a collection of images
            using (var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                EnsurePathExists = true,
                Title = "Select images to modify",
                EnsureFileExists = true,
                Multiselect = true
            })
            {
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    var files = dialog.FileNames.Select(p => p).ToArray();

                    handleProcessList(files);
                }
            }
        }
        private void handleProcessList(string[] incoming)
        {
            foreach (var path in incoming)
            {
                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(path);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {//Directory, get files
                    var dirInfo = new System.IO.DirectoryInfo(path);
                    foreach (var f in dirInfo.GetFiles())
                    {
                        var ext = f.Extension.ToLower();
                        if (ext == ".jpg" || ext == ".jpeg")
                        {
                            filesToBeTagged.Add(f.FullName);
                        }
                    }
                }
                else
                {
                    //File -> add it
                    filesToBeTagged.Add(path);
                }
            }
            updateFileViewList();
        }
        private void btnLoadImages_Drop(object sender, DragEventArgs e)
        {
            filesToBeTagged.Clear();
            var incoming = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            //If these are directories, enumerate all contained images
            //Otherise if its files, add thes directly
            handleProcessList(incoming);
        }
        /// <summary>
        /// Updates the list of files to be processed
        /// </summary>
        private void updateFileViewList()
        {
            listImages.Items.Clear();
            foreach (var s in filesToBeTagged)
            {
                listImages.Items.Add(s);
            }
            EnableStartCheck();
        }
        private void btnSelectCustomOp_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnLoadCSV_Click(object sender, RoutedEventArgs e)
        {
            //Prompt user for the CSV source file


            OpenFileDialog openFileDialog = new OpenFileDialog();

            //                openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            var result = openFileDialog.ShowDialog();
            if (result == null)
            {
                return;
            }
            if ((bool)result == false)
            {
                return;
            }
            //Get the path of specified file
            var filePath = openFileDialog.FileName;
            loadCSV(filePath);

        }
        private void loadCSV(string filePath)
        {
            cSVTags.Parse(filePath);
            //Have now parsed the CSV file
            renderCSVInfo();
        }
        /// <summary>
        /// Renders out the current CSVTags into the UI
        /// </summary>
        private void renderCSVInfo()
        {
            gridSettings.Children.Clear();
            gridSettings.HorizontalAlignment = HorizontalAlignment.Stretch;
            gridSettings.VerticalAlignment = VerticalAlignment.Top;
            gridSettings.ShowGridLines = true;
            gridSettings.ColumnDefinitions.Clear();
            gridSettings.RowDefinitions.Clear();
            {
                for (int i = 0; i < 2; i++)
                {
                    var row = new RowDefinition
                    {
                        Height = GridLength.Auto
                    };
                    gridSettings.RowDefinitions.Add(row);
                }
            }
            {

                for (int i = 0; i < cSVTags.parsedColumns.Count; i++)
                {
                    var col = new ColumnDefinition
                    {
                        Width = GridLength.Auto
                    };
                    gridSettings.ColumnDefinitions.Add(col);
                }
            }
            {
                for (int i = 0; i < cSVTags.parsedColumns.Count; i++)
                {
                    var col = cSVTags.parsedColumns[i];
                    {

                        // For each column, add in a label (column) and dropdown (type)
                        Label label = new Label();
                        label.Content = col.ColumnName;
                        Grid.SetRow(label, 0);
                        Grid.SetColumn(label, i);
                        gridSettings.Children.Add(label);
                    }
                    //Add dropdown for setting the type
                    {
                        ComboBox cb = new ComboBox();
                        foreach (var e in Enum.GetValues(typeof(EXIFReWriter.EXIFTag)))
                        {
                            cb.Items.Add(EXIFReWriter.EXIFTagToString((EXIFReWriter.EXIFTag)e));
                        }
                        try
                        {
                            cb.SelectedIndex = (int)col.ColumnTag;
                        }
                        catch { }
                        Grid.SetRow(cb, 1);
                        Grid.SetColumn(cb, i);
                        cb.Tag = i;
                        cb.SelectionChanged += CsvTagColumn_SelectionChanged;
                        gridSettings.Children.Add(cb);
                    }
                }
            }
            EnableStartCheck();
        }

        private void CsvTagColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = (ComboBox)sender;
            //Derive the column for this one and update the appropriate tag
            try
            {
                int column = (int)c.Tag;
                if (column < cSVTags.parsedColumns.Count)
                {
                    var columnData = cSVTags.parsedColumns[column];
                    var possibleOptions = (EXIFReWriter.EXIFTag[])Enum.GetValues(typeof(EXIFReWriter.EXIFTag));
                    if (c.SelectedIndex < possibleOptions.Length)
                    {
                        columnData.ColumnTag = possibleOptions[c.SelectedIndex];
                    }else
                    {
                        if (c.SelectedIndex > 0)
                        {
                            c.SelectedIndex = 0;//reset if save failed
                        }
                    }
                    cSVTags.parsedColumns[column] = columnData;
                }
                else
                {
                    if (c.SelectedIndex > 0)
                    {
                        c.SelectedIndex = 0;//reset if save failed
                    }
                }
            }
            catch
            {
                if (c.SelectedIndex > 0)
                {
                    c.SelectedIndex = 0;//reset if save failed
                }
            }
        }

        private void btnLoadCSV_Drop(object sender, DragEventArgs e)
        {
            var path = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (path.Length > 0)
            {
                loadCSV(path[0]);
            }
            EnableStartCheck();
        }

        private void btnSelectOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsurePathExists = true,
                Title = "Select output folder"
            })
            {
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    outputFolderPath = dialog.FileName;

                }
            }
            EnableStartCheck();
        }

        private void EnableStartCheck()
        {
            btnStart.IsEnabled = false;

            if (cSVTags.parsedColumns != null && cSVTags.parsedColumns.Count > 1)
            {
                if (filesToBeTagged.Count > 0)
                {
                    if (outputFolderPath.Length > 3)
                    {
                        btnStart.IsEnabled = true;
                    }
                }
            }
        }
    }
}
