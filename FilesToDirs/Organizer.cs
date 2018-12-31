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
        public static async Task CopyFilesByExtensionAsync(string sourcePath, string destinationPath, bool includeSubdirectories)
        {
            var progress = new Progress<double>(value => MainWindow.Progress.Value = value);

            List<FileModel> files = new List<FileModel>();
            List<string> paths = new List<string>();
            List<string> extensions = new List<string>();

            MainWindow.SelectSourceButton.IsEnabled = false;
            MainWindow.SelectDestinationButton.IsEnabled = false;
            MainWindow.PercentLabel.Visibility = System.Windows.Visibility.Visible;
            MainWindow.Progress.Visibility = System.Windows.Visibility.Visible;

            if(includeSubdirectories)
            {
                paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
            }
            else
            {
                paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.TopDirectoryOnly).ToList();
            }

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
                if (!MainWindow.IsCancelled)
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
                    source.Close();
                    destination.Close();
                }
            }

            MainWindow.LogText.AppendText("---------------------------\n");

            if(MainWindow.IsCancelled)
            {
                MainWindow.LogText.AppendText("Process aborted by user.\nCopied " + count + " of " + total + " files\n");
                MainWindow.Progress.Visibility = System.Windows.Visibility.Hidden;
                MainWindow.PercentLabel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                MainWindow.LogText.AppendText("All files copied successfully\n");
            }

            MainWindow.SelectSourceButton.IsEnabled = true;
            MainWindow.SelectDestinationButton.IsEnabled = true;

            MainWindow.OrganizeButton.Content = "Organize";
            MainWindow.OrganizeButton.Click += MainWindow.OrganizeButton_Click;
            MainWindow.OrganizeButton.Click -= MainWindow.Cancel_Click;
            MainWindow.OrganizeButton.IsEnabled = true;
            MainWindow.IsCancelled = false;
        }

        public static async Task CopyFilesByTypeAsync(string sourcePath, string destinationPath, Dictionary<string, string> extToType, bool includeSubdirectories)
        {
            var progress = new Progress<double>(value => MainWindow.Progress.Value = value);

            List<FileModel> files = new List<FileModel>();
            List<string> paths = new List<string>();
            List<string> types = new List<string>();

            MainWindow.SelectSourceButton.IsEnabled = false;
            MainWindow.SelectDestinationButton.IsEnabled = false;
            MainWindow.PercentLabel.Visibility = System.Windows.Visibility.Visible;
            MainWindow.Progress.Visibility = System.Windows.Visibility.Visible;

            if (includeSubdirectories)
            {
                paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
            }
            else
            {
                paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.TopDirectoryOnly).ToList();
            }

            int total = paths.Count;
            int count = 0;
            bool other = false;

            foreach (var p in paths)
            {
                FileInfo file = new FileInfo(p);
                string type;
                string extension = file.Extension.Substring(1, file.Extension.Length - 1).ToUpper();

                if(extToType.TryGetValue(extension, out type))
                {
                    files.Add(new FileModel { Name = file.Name, Path = p, Extension = extension, Type = type });
                }
                else
                {
                    files.Add(new FileModel { Name = file.Name, Path = p, Extension = extension, Type = "Other" });
                    other = true;
                }   
            }

            types = files.GroupBy(a => a.Type).Select(x => x.Key).ToList();
            if (other) types.Add("Other");

            MainWindow.LogText.AppendText("Types found:\n");

            foreach (var t in types)
            {
                MainWindow.LogText.AppendText(t + "\n");
            }

            MainWindow.LogText.AppendText("---------------------------\n");

            foreach (var t in types)
            {
                string dirPath = destinationPath + @"\" + t;
                Directory.CreateDirectory(dirPath);
                MainWindow.LogText.AppendText("Directory created in " + dirPath + "\n");
            }

            MainWindow.LogText.AppendText("---------------------------\n");

            foreach (var f in files)
            {
                if (!MainWindow.IsCancelled)
                {
                    string copyToPath = destinationPath + @"\" + f.Type + @"\" + f.Name;

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
                    source.Close();
                    destination.Close();
                }
            }

            MainWindow.LogText.AppendText("---------------------------\n");

            if (MainWindow.IsCancelled)
            {
                MainWindow.LogText.AppendText("Process aborted by user.\nCopied " + count + " of " + total + " files\n");
                MainWindow.Progress.Visibility = System.Windows.Visibility.Hidden;
                MainWindow.PercentLabel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                MainWindow.LogText.AppendText("All files copied successfully\n");
            }

            MainWindow.SelectSourceButton.IsEnabled = true;
            MainWindow.SelectDestinationButton.IsEnabled = true;

            MainWindow.OrganizeButton.Content = "Organize";
            MainWindow.OrganizeButton.Click += MainWindow.OrganizeButton_Click;
            MainWindow.OrganizeButton.Click -= MainWindow.Cancel_Click;
            MainWindow.OrganizeButton.IsEnabled = true;
            MainWindow.IsCancelled = false;
        }
    }
}
