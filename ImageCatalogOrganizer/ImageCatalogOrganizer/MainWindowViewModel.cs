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

namespace ImageCatalogOrganizer
{
    public class MainWindowViewModel : ViewModelBase
    {
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
        private string _selectedPath;
        private string _logText;
        private int _concurrencyLevel = 1;
        public int FilesFound { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                _selectedPath = value;
                OnPropertyChanged("SelectedPath");
                GoAction.RaiseCanExecuteChanged();
                Properties.Settings.Default["SelectedPath"] = _selectedPath;
                Properties.Settings.Default.Save();
            }
        }
        public DelegateCommand FileBrowseAction { get; }
        public DelegateCommand GoAction { get; }

        public MainWindowViewModel()
        {
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            FileBrowseAction = new DelegateCommand(ExecuteButtonFileBrowseAction, canExecuteFileBrowseAction);
            GoAction = new DelegateCommand(ExecuteGoAction, CanExecuteGoAction);
            SelectedPath = Properties.Settings.Default["SelectedPath"]?.ToString();
            int numProcs = Environment.ProcessorCount;
            _concurrencyLevel = numProcs * 2;
            addLogEntry($"{numProcs} processors, using {_concurrencyLevel} threads");
            FilesFound = 0;
        }

        private void ExecuteGoAction(object obj)
        {
            backgroundWorker.RunWorkerAsync();
            FileBrowseAction.RaiseCanExecuteChanged();
            GoAction.RaiseCanExecuteChanged();
        }

        private bool CanExecuteGoAction(object obj)
        {
            return string.IsNullOrEmpty(SelectedPath) == false && backgroundWorker.IsBusy == false;
        }

        private bool canExecuteFileBrowseAction(object obj)
        {
            return backgroundWorker.IsBusy == false;
        }

        // actions
        private void ExecuteButtonFileBrowseAction(object obj)
        {
            if (backgroundWorker.IsBusy)
                return;

            var fbd = new FolderBrowserDialog();
            fbd.SelectedPath = SelectedPath;
            if (fbd.ShowDialog() != DialogResult.OK)
                return;

            SelectedPath = fbd.SelectedPath;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FileBrowseAction.RaiseCanExecuteChanged();
            GoAction.RaiseCanExecuteChanged();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            clearLog();
            
            processFiles(SelectedPath);
            HashToFile.Clear();
            FilesFound = 0;
            disp.BeginInvoke((Action)(() =>
            {
                OnPropertyChanged("FilesFound");
            }));

            //var files = getFiles("laz");
            //addLogEntry($"Found {files.Count()} LasZip-compressed files");

            //foreach (var file in files)
            //{
            //    processLazFile(file);
            //}

            addLogEntry($"Found {HashToFile.Count} unique files");
        }

        ConcurrentDictionary<string, string> HashToFile = new ConcurrentDictionary<string, string>();
        Dispatcher disp = System.Windows.Application.Current.Dispatcher;
        private void processFiles(string rootPath)
        {            
            var files = Directory.GetFiles(rootPath);
            addLogEntry($"Processing {files.Length:N0} files in {rootPath}");

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = _concurrencyLevel }, (file) =>
            {
                var hash = getFileHash(file);
                var added = HashToFile.TryAdd(hash, file);
                if (!added)
                {
                    addLogEntry($"Ignoring duplicate file {file}");
                }
                else
                {
                    FilesFound += 1;
                    disp.BeginInvoke((Action)(() =>
                    {
                        OnPropertyChanged("FilesFound");
                    }));
                }
            });

            var directories = Directory.GetDirectories(rootPath);
            foreach (var directory in directories)
            {
                processFiles(directory);
            }
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
                    LogText = $"\t\t{log}{Environment.NewLine}{LogText}";
                else
                    LogText = $"[{DateTime.Now:HH:mm:ss}]\t{log}{Environment.NewLine}{LogText}";
            }));
        }
    }
}
