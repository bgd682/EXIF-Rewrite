
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load in any predefinied options
            string[] customOperations = { "Adjust Date/Time" };

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

        }

        private void btnLoadImages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLoadImages_Drop(object sender, DragEventArgs e)
        {

        }

        private void btnSelectCustomOp_Click(object sender, RoutedEventArgs e)
        {

        }
        private CSVTags cSVTags = new CSVTags();
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
        }

        private void CsvTagColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = (ComboBox)sender;
            //Derive the column for this one and update the appropriate tag
            try
            {
                int column = (int)c.Tag;
                if (column < cSVTags.parsedColumns.Count)
                { var columnData = cSVTags.parsedColumns[column];
                    columnData.ColumnTag = (EXIFReWriter.EXIFTag)c.SelectedIndex;
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
    }
}
