using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace ReOrganizeMusic
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var directory = SourcePathTextBox.Text;
            if (!System.IO.Directory.Exists(directory))
                return;

            using (var logfile = new StreamWriter("log.txt", false))
            {
                var subDirectories = Directory.GetDirectories(directory);
                foreach (var subDirectory in subDirectories)
                {
                    var relativeSubDirectory = System.IO.Path.GetFileName(subDirectory);

                    var separatorPos = relativeSubDirectory.IndexOf(" - ");
                    if (separatorPos < 0)
                    {
                        logfile.WriteLine("SKIP {0}", relativeSubDirectory);
                    }

                    var interpret = relativeSubDirectory.Substring(0, separatorPos);
                    var album = relativeSubDirectory.Substring(separatorPos + 3);

                    logfile.WriteLine("{0} => {1}\\{2}", relativeSubDirectory, interpret, album);

                    var targetDir = System.IO.Path.Combine(@"G:\mp3\a neu", interpret, album);
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    _CopyRecursive(subDirectory, targetDir, true);
                    _Verify(logfile, subDirectory, targetDir);

                }
            }
        }

        private void _Verify(StreamWriter logfile, string sourceDirectory, string targetDirectory)
        {
            var sourceFiles = Directory.GetFiles(sourceDirectory);
            
            foreach (var sourceFile in sourceFiles)
	        {
                var filename = System.IO.Path.GetFileName(sourceFile);
                var targetPath = System.IO.Path.Combine(targetDirectory, filename);

                if (!File.Exists(targetPath))
                {
                    logfile.WriteLine("ERROR - NOT EXISTING - {0}", targetPath);
                    continue;
                }

                var areFilesEqual = CompareFileBytes(sourceFile, targetPath);
                if (!areFilesEqual)
                {
                    logfile.WriteLine("ERROR - NOT EQUAL - {0}", targetPath);
                }
                else
                {
                    logfile.WriteLine("OK - {0}", targetPath);
                }
	        }

            var subDirectories = Directory.GetDirectories(sourceDirectory);
            foreach (var subDirectory in subDirectories)
            {
                var relativeDir = System.IO.Path.GetFileName(subDirectory);
                _Verify(logfile, subDirectory, System.IO.Path.Combine(targetDirectory, relativeDir));
            }
        }

        private static bool CompareFileBytes(string fileName1, string fileName2)
        {
            // Compare file sizes before continuing. 
            // If sizes are equal then compare bytes.
            if (CompareFileSizes(fileName1, fileName2))
            {
                int file1byte = 0;
                int file2byte = 0;

                // Open a System.IO.FileStream for each file.
                // Note: With the 'using' keyword the streams 
                // are closed automatically.
                using (FileStream fileStream1 = new FileStream(fileName1, FileMode.Open),
                                  fileStream2 = new FileStream(fileName2, FileMode.Open))
                {
                    // Read and compare a byte from each file until a
                    // non-matching set of bytes is found or the end of
                    // file is reached.
                    do
                    {
                        file1byte = fileStream1.ReadByte();
                        file2byte = fileStream2.ReadByte();
                    }
                    while ((file1byte == file2byte) && (file1byte != -1));
                }

                return ((file1byte - file2byte) == 0);
            }
            else
            {
                return false;
            }
        }

        private static bool CompareFileSizes(string fileName1, string fileName2)
        {
            bool fileSizeEqual = true;

            // Create System.IO.FileInfo objects for both files
            FileInfo fileInfo1 = new FileInfo(fileName1);
            FileInfo fileInfo2 = new FileInfo(fileName2);

            // Compare file sizes
            if (fileInfo1.Length != fileInfo2.Length)
            {
                // File sizes are not equal therefore files are not identical
                fileSizeEqual = false;
            }

            return fileSizeEqual;
        }

        private void _CopyRecursive(string sourceDirectory, string targetDirectory, bool copySubDirectories)
        {
            var destDirName = targetDirectory;
            var dir = new DirectoryInfo(sourceDirectory);
            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirectories)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    _CopyRecursive(subdir.FullName, temppath, copySubDirectories);
                }
            }
        }
    }
}
