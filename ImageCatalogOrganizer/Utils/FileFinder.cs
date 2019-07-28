using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class FileFinder
    {
        private static List<string> SupportedExtensions = new List<string> { ".jpg", ".raw", ".avi", ".wmv", ".rw2", ".png", ".jpeg", ".nef" };
        public static Dictionary<FileTypeCode, FileWrapper> GetAllFiles(string path, int maxThreads = 8, bool onlyUniqueFiles = true, bool recursive = true, int minimumFileSizeKB = 400)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            } 
            
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, (file) =>
            {
                var ext = Path.GetExtension(file).ToLower();
                if (SupportedExtensions.Any(x => x == ext) == false)
                {
                    Console.WriteLine($"Skipping {file}");
                    return;
                }

                var raw = new RawFile(file);

                //var hash = getFileHash(file);
                //var added = HashToFile.TryAdd(hash, file);
                //if (!added)
                //{
                //    addLogEntry($"Ignoring duplicate file {file}");
                //}
                //else
                //{
                //    FilesFound += 1;
                //    disp.BeginInvoke((Action)(() =>
                //    {
                //        OnPropertyChanged("FilesFound");
                //    }));
                //}
            });
            return new Dictionary<FileTypeCode, FileWrapper>();
        }
    }
}
