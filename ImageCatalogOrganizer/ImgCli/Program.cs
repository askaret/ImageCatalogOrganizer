using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace ImgCli
{
    class Program
    {
        private static string RootFolders = @"F:\Backup processing;H:\Picture vault;H:\Picture export;H:\OneDrive\Pictures\Camera Roll";
        private static string OutputFolder = @"H:\temp output";
        private static int Concurrency = 16;
        static void Main(string[] args)
        {
            Logger.Log($"Getting files from {RootFolders}, please wait...");
            Dictionary<FileTypeCode, List<FileWrapper>> files;
            try
            {
                files = FileFinder.GetAllFiles(RootFolders, Concurrency);
            }
            catch (Exception e)
            {
                Logger.LogError($"{e.Message}");
                return;
            }

            Logger.Log($"Unique files found");
            var sumAllFiles = 0;
            foreach (var key in files.Keys)
            {
                var count = files[key].Count;
                Logger.Log($"\t{key}\t{count}");
                sumAllFiles += count;
            }
            Logger.Log($"{sumAllFiles} file(s) in total");

            organizeFiles(files);

            finalDuplicatePrune(OutputFolder);

            Logger.Log("Done! Press any key to close...");
            Console.ReadKey(true);
        }

        private static void finalDuplicatePrune(string outputFolder)
        {
            var files = Directory.GetFiles(outputFolder, "*.*", SearchOption.AllDirectories);
            var fileDict = new Dictionary<long, string>();
            
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                if (fileDict.ContainsKey(fi.Length))
                {
                    Logger.LogWarning($"Deleting {file}, duplicate of {fileDict[fi.Length]}");
                    fi.Delete();
                }
                else
                    fileDict.Add(fi.Length, file);
            }
        }

        private static void organizeFiles(Dictionary<FileTypeCode, List<FileWrapper>> files)
        {
            var outputRoot = Path.Combine(OutputFolder, $"{DateTime.Now.Date:yyyy-MM-dd} output");
            Logger.Log($"Processing unique files to {outputRoot}");

            if (Directory.Exists(outputRoot))
            {
                Logger.LogWarning($"Deleting {outputRoot}");
                Directory.Delete(outputRoot, true);
            }

            foreach (var filetype in files.Keys)
            {
                Logger.Log($"Processing {files[filetype].Count()} file(s) of type {filetype}");
                var filesToProcess = files[filetype];

                Parallel.ForEach(filesToProcess, new ParallelOptions { MaxDegreeOfParallelism = Concurrency }, (fileToProcess) =>
                {
                    var creationTime = fileToProcess.GetCreationTime();
                    var imageOutputPath = filetype == FileTypeCode.Video ? Path.Combine(outputRoot, $"videos/{creationTime.Year}") : Path.Combine(outputRoot, creationTime.Year.ToString());
                    var extension = Path.GetExtension(fileToProcess.FilePath);

                    if (!Directory.Exists(imageOutputPath))
                        Directory.CreateDirectory(imageOutputPath);

                    var newFileName = Path.Combine(imageOutputPath, $"{creationTime:yyyy-mm-dd HH.MM.ss} ({Path.GetFileNameWithoutExtension(fileToProcess.FilePath)}){extension}");
                    if (File.Exists(newFileName))
                    {
                        Logger.LogWarning($"{newFileName} already exists, renaming");
                        newFileName = Path.Combine(imageOutputPath, $"{creationTime:yyyy-mm-dd HH.MM.ss} ({Path.GetFileNameWithoutExtension(fileToProcess.FilePath)})({DateTime.Now.Ticks}){extension}");
                    }

                    File.Copy(fileToProcess.FilePath, newFileName);
                });
            }
        }
    }
}
