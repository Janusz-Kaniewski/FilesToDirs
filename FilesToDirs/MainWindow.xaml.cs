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
        public static System.Windows.Controls.Label StatusLabel = new System.Windows.Controls.Label { Content = "Ready" };
        public static System.Windows.Controls.TextBox LogText = new System.Windows.Controls.TextBox { Height = 200, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 10, 0, 0)};
        public static System.Windows.Controls.ProgressBar Progress = new System.Windows.Controls.ProgressBar { Height = 10, Margin = new Thickness(0, 10, 0, 0), Minimum = 0, Maximum = 100 };
        public static System.Windows.Controls.Button SelectSourceButton = new System.Windows.Controls.Button { Content = "Select source", Height = 20, Width = 150 };
        public static System.Windows.Controls.Button SelectDestinationButton = new System.Windows.Controls.Button { Content = "Select destination", Height = 20, Width = 150 };
        public static System.Windows.Controls.Button OrganizeButton = new System.Windows.Controls.Button { Content = "Organize", Height = 20, Width = 150, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Margin = new Thickness(0, 10, 0, 0) };
        private System.Windows.Controls.Label SourcePath = new System.Windows.Controls.Label { Content = "", Margin = new Thickness(10, 0, 0, 0) };
        private System.Windows.Controls.Label DestinationPath = new System.Windows.Controls.Label { Content = "", Margin = new Thickness(10, 0, 0, 0) };

        string sourcePath;
        string destinationPath;

        public MainWindow()
        {
            InitializeComponent();

            StackPanel stack = new StackPanel {Margin = new Thickness(10, 10, 10, 10) };
            StackPanel sourceStack = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            StackPanel destinationStack = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };

            SelectSourceButton.Click += SelectSourceButton_Click;
            SelectDestinationButton.Click += SelectDestinationButton_Click;
            OrganizeButton.Click += OrganizeButton_Click;

            stack.Children.Add(new System.Windows.Controls.Label { Content = "Source" });

            sourceStack.Children.Add(SelectSourceButton);
            sourceStack.Children.Add(SourcePath);
            stack.Children.Add(sourceStack);

            stack.Children.Add(new System.Windows.Controls.Label { Content = "Destination" });

            destinationStack.Children.Add(SelectDestinationButton);
            destinationStack.Children.Add(DestinationPath);
            stack.Children.Add(destinationStack);

            stack.Children.Add(Progress);
            stack.Children.Add(LogText);
            stack.Children.Add(OrganizeButton);
            stack.Children.Add(StatusLabel);

            Content = stack;            
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
            await Organizer.CopyFilesByExtensionAsync(sourcePath, destinationPath);
        }

        public async Task CopyFilesAsync()
        {
            List<FileModel> files = new List<FileModel>();
            List<string> paths = new List<string>();
            List<string> extensions = new List<string>();

            paths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
            int total = paths.Count;
            int count = 0;

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

                LogText.AppendText("Copying file " + f.Path + "...\n");
                await source.CopyToAsync(destination);
                LogText.AppendText("Copied file " + f.Path + " to " + copyToPath + "\n");
                count++;
                StatusLabel.Content = "Copied " + count + " of " + total + " files";
                LogText.ScrollToEnd();
            }

            LogText.AppendText("---------------------------\n");
            LogText.AppendText("All files copied successfully\n");
        }
    }
}
