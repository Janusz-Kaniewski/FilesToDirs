using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FilesToDirs.Models;
using System.IO;

namespace FilesToDirs
{
    public partial class MainWindow : Window
    {
        string sourcePath;
        string destinationPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectSourceButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder where are files you want to organize.";
            DialogResult result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                SourcePath.Content = sourcePath = dialog.SelectedPath;
            }
        }

        private void SelectDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select destination folder where organized files will be stored.";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DestinationPath.Content = destinationPath = dialog.SelectedPath;
            }
        }

        private async void OrganizeButton_Click(object sender, RoutedEventArgs e)
        {
            await CopyFilesAsync();
        }

        public async Task CopyFilesAsync()
        {
            List<FileModel> files = new List<FileModel>();
            List<string> paths = new List<string>();
            List<string> extensions = new List<string>();

            paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();

            foreach (var p in paths)
            {
                FileInfo file = new FileInfo(p);
                files.Add(new FileModel { Name = file.Name, Path = p, Extension = file.Extension.Substring(1, file.Extension.Length - 1).ToUpper() });
            }

            extensions = files.GroupBy(a => a.Extension).Select(x => x.Key).ToList();

            LogText.AppendText("Extensions found:\n");

            foreach (var ext in extensions)
            {
                LogText.AppendText(ext + "\n");
            }

            LogText.AppendText("---------------------------\n");

            foreach (var ext in extensions)
            {
                string dirPath = destinationPath + @"\" + ext;
                Directory.CreateDirectory(dirPath);
                LogText.AppendText("Directory created in " + dirPath + "\n");
            }

            LogText.AppendText("---------------------------\n");

            foreach (var f in files)
            {
                string copyToPath = destinationPath + @"\" + f.Extension + @"\" + f.Name;

                FileStream source = File.Open(f.Path, FileMode.Open);
                FileStream destination = File.Create(copyToPath);

                await source.CopyToAsync(destination);

                LogText.AppendText("Copied file " + f.Path + " to " + copyToPath + "\n");
                LogText.ScrollToEnd();
            }

            LogText.AppendText("---------------------------\n");
            LogText.AppendText("All files copied successfully\n");
        }
    }
}
