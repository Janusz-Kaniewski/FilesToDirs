using FilesToDirs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesToDirs
{
    public static class Organizer
    {
        public static async Task CopyFilesByExtensionAsync(string sourcePath, string destinationPath)
        {
            var progress = new Progress<double>(value => MainWindow.Progress.Value = value);

            List<FileModel> files = new List<FileModel>();
            List<string> paths = new List<string>();
            List<string> extensions = new List<string>();

            MainWindow.SelectSourceButton.IsEnabled = false;
            MainWindow.SelectDestinationButton.IsEnabled = false;
            MainWindow.OrganizeButton.IsEnabled = false;
            MainWindow.Progress.Visibility = System.Windows.Visibility.Visible;

            paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
            int total = paths.Count;
            int count = 0;

            foreach (var p in paths)
            {
                FileInfo file = new FileInfo(p);
                files.Add(new FileModel { Name = file.Name, Path = p, Extension = file.Extension.Substring(1, file.Extension.Length - 1).ToUpper() });
            }

            extensions = files.GroupBy(a => a.Extension).Select(x => x.Key).ToList();

            MainWindow.LogText.AppendText("Extensions found:\n");

            foreach (var ext in extensions)
            {
                MainWindow.LogText.AppendText(ext + "\n");
            }

            MainWindow.LogText.AppendText("---------------------------\n");

            foreach (var ext in extensions)
            {
                string dirPath = destinationPath + @"\" + ext;
                Directory.CreateDirectory(dirPath);
                MainWindow.LogText.AppendText("Directory created in " + dirPath + "\n");
            }

            MainWindow.LogText.AppendText("---------------------------\n");

            foreach (var f in files)
            {
                string copyToPath = destinationPath + @"\" + f.Extension + @"\" + f.Name;

                FileStream source = File.Open(f.Path, FileMode.Open);
                FileStream destination = File.Create(copyToPath);

                MainWindow.LogText.AppendText("Copying file " + f.Path + "...\n");
                await source.CopyToAsync(destination);
                MainWindow.LogText.AppendText("Copied file " + f.Path + " to " + copyToPath + "\n");
                count++;
                MainWindow.StatusLabel.Content = "Copied " + count + " of " + total + " files";
                var percent = Convert.ToDouble(count) / Convert.ToDouble(total) * 100;
                MainWindow.LogText.ScrollToEnd();
                ((IProgress<double>)progress).Report(percent);
                MainWindow.PercentLabel.Content = Convert.ToInt32(percent) + "%";
            }

            MainWindow.LogText.AppendText("---------------------------\n");
            MainWindow.LogText.AppendText("All files copied successfully\n");

            MainWindow.SelectSourceButton.IsEnabled = true;
            MainWindow.SelectDestinationButton.IsEnabled = true;
            MainWindow.OrganizeButton.IsEnabled = true;
        }
    }
}
