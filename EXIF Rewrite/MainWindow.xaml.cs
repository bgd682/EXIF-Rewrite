
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


        }
        /// <summary>
        /// Renders out the current CSVTags into the UI
        /// </summary>
        private void renderCSVInfo()
        {
            gridSettings.Children.Clear();
            gridSettings.HorizontalAlignment = HorizontalAlignment.Center;
            gridSettings.VerticalAlignment = VerticalAlignment.Center;
            gridSettings.ShowGridLines = true;
            gridSettings.ColumnDefinitions.Clear();
            gridSettings.RowDefinitions.Clear();
            {
                var row = new RowDefinition
                {
                    Height = GridLength.Auto
                };
                gridSettings.RowDefinitions.Add(row);
                gridSettings.RowDefinitions.Add(row);
            }
            {
                var col = new ColumnDefinition
                {
                    Width = GridLength.Auto
                };
                for (int i = 0; i < cSVTags.parsedColumns.Count; i++)
                {
                    gridSettings.ColumnDefinitions.Add(col);
                }
            }

        }
    }
}
