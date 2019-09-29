using photo.exif;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Utils;

namespace ImageCatalogOrganizer
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _outputPath;
        private string _rootPath;
        private string _logText;
        private int _concurrencyLevel = 1;

        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged("LogText");
            }
        }

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        
        public int FilesFound { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public string OutputPath
        {
            get => _outputPath;
            set
            {
                _outputPath = value;
                OnPropertyChanged("OutputPath");
                ProcessImages.RaiseCanExecuteChanged();
                Properties.Settings.Default["OutputPath"] = _outputPath;
                Properties.Settings.Default.Save();
            }
        }
        public string RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = value;
                OnPropertyChanged("RootPath");
                ProcessImages.RaiseCanExecuteChanged();
                Properties.Settings.Default["RootPath"] = _rootPath;
                Properties.Settings.Default.Save();
            }
        }
        public DelegateCommand BrowseOutputFolder { get; }
        public DelegateCommand BrowseRootFolder { get; }
        public DelegateCommand ProcessImages { get; }

        public MainWindowViewModel()
        {
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            BrowseOutputFolder = new DelegateCommand(ExecuteBrowseOutputFolder);
            BrowseRootFolder = new DelegateCommand(ExecuteBrowseRootFolder);
            ProcessImages = new DelegateCommand(ExecuteProcessImages, CanProcessImages);
            RootPath = Properties.Settings.Default["RootPath"]?.ToString();
            OutputPath = Properties.Settings.Default["OutputPath"]?.ToString();
            int numProcs = Environment.ProcessorCount;

            _concurrencyLevel = numProcs * 4;
            addLogEntry($"{numProcs} processors, using {_concurrencyLevel} threads");
            FilesFound = 0;
        }

        private void ExecuteProcessImages(object obj)
        {
            backgroundWorker.RunWorkerAsync();
            ProcessImages.RaiseCanExecuteChanged();
        }

        private bool CanProcessImages(object obj)
        {
            return string.IsNullOrEmpty(OutputPath) == false && string.IsNullOrEmpty(RootPath) == false && backgroundWorker.IsBusy == false;
        }

        private void ExecuteBrowseRootFolder(object obj)
        {
            if (backgroundWorker.IsBusy)
                return;

            var fbd = new FolderBrowserDialog();
            fbd.SelectedPath = RootPath;
            if (fbd.ShowDialog() != DialogResult.OK)
                return;
            
            RootPath = fbd.SelectedPath;
        }

        private void ExecuteBrowseOutputFolder(object obj)
        {
            if (backgroundWorker.IsBusy)
                return;

            var fbd = new FolderBrowserDialog();
            fbd.SelectedPath = OutputPath;
            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            OutputPath = fbd.SelectedPath;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProcessImages.RaiseCanExecuteChanged();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            clearLog();
            HashToFile.Clear();
            FilesFound = 0;

            disp.BeginInvoke((Action)(() =>
            {
                OnPropertyChanged("FilesFound");
            }));

            processFiles();
        }

        ConcurrentDictionary<string, string> HashToFile = new ConcurrentDictionary<string, string>();
        Dispatcher disp = System.Windows.Application.Current.Dispatcher;

        private void organizeFiles(Dictionary<FileTypeCode, List<FileWrapper>> files)
        {
            var outputRoot = Path.Combine(RootPath, $"{DateTime.Now.Date:yyyy-MM-dd} output");
            addLogEntry($"Processing unique files to {outputRoot}");

            if (Directory.Exists(outputRoot))
            {
                if (MessageBox.Show($"Folder {outputRoot} already exists, delete it before proceeding?", "Delete existing output folder?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    addLogEntry($"Deleting {outputRoot}");
                    Directory.Delete(outputRoot, true);
                }
            }

            foreach (var filetype in files.Keys)
            {
                addLogEntry($"Processing {files[filetype].Count()} file(s) of type {filetype}");
                var filesToProcess = files[filetype];

                Parallel.ForEach(filesToProcess, new ParallelOptions { MaxDegreeOfParallelism = 16 }, (fileToProcess) =>
                {
                    var creationTime = File.GetCreationTime(fileToProcess.FilePath);
                    var imageOutputPath = filetype == FileTypeCode.Video ? Path.Combine(outputRoot, $"videos/{creationTime.Year}") : Path.Combine(outputRoot, creationTime.Year.ToString());
                    var extension = Path.GetExtension(fileToProcess.FilePath);

                    if (!Directory.Exists(imageOutputPath))
                        Directory.CreateDirectory(imageOutputPath);

                    var newFileName = Path.Combine(imageOutputPath, $"{creationTime:YYYY-mm-DD HH:MM:ss} ({Path.GetFileNameWithoutExtension(fileToProcess.FilePath)}).{extension}");
                    if (File.Exists(newFileName))
                    {
                        addLogEntry($"{newFileName} already exists, skipping");
                    }

                    File.Copy(fileToProcess.FilePath, newFileName);
                });
            }
        }
        private void processFiles()
        {
            addLogEntry($"Getting files from {RootPath}, please wait...");
            Dictionary<FileTypeCode, List<FileWrapper>> files;
            try
            {
                files = FileFinder.GetAllFiles(RootPath);                
            }
            catch (Exception e)
            {
                addLogEntry($"Error: {e.Message}");
                return;
            }

            addLogEntry($"Unique files found");
            var sumAllFiles = 0;
            foreach (var key in files.Keys)
            {
                var count = files[key].Count;
                addLogEntry($"\t{key}\t{count}");
                sumAllFiles += count;
            }
            addLogEntry($"{sumAllFiles} file(s) in total");

            organizeFiles(files);
        }

        private static string getFileHash(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        private void clearLog()
        {
            var disp = System.Windows.Application.Current.Dispatcher;
            disp.BeginInvoke((Action)(() => { LogText = string.Empty; }));
        }

        private void addLogEntry(string log, bool excludeTimestamp = false)
        {
            if (string.IsNullOrWhiteSpace(log))
                return;
                        
            disp.BeginInvoke((Action)(() =>
            {
                if (excludeTimestamp)
                    LogText += $"\t\t{log}{Environment.NewLine}";
                else
                    LogText += $"[{DateTime.Now:HH:mm:ss}]\t{log}{Environment.NewLine}";
            }));
        }
    }
}
