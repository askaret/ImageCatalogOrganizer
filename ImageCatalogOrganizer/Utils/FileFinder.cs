using ImgCli;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class FileFinder
    {
        private static List<string> SupportedExtensions = new List<string> { ".jpg", ".raw", ".avi", ".wmv", ".rw2", ".png", ".jpeg", ".nef", ".mpg", ".mpeg", ".mp4", ".mov"  };
        public static Dictionary<FileTypeCode, List<FileWrapper>> GetAllFiles(string path, int maxThreads = 16, bool onlyUniqueFiles = true, bool recursive = true, int minimumFileSizeKB = 40)
        {   
            var folders = path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                       
            List<string> files = new List<string>();

            try
            {
                foreach (var folder in folders)
                {
                    var candidateFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(x => FileFinder.SupportedExtensions.Contains(Path.GetExtension(x).ToLower()));
                    
                    files.AddRange(candidateFiles.OrderBy(x => x));
                }                
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                throw e;
            }

            var dict = new ConcurrentDictionary<FileTypeCode, ConcurrentBag<FileWrapper>>();
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, (file) =>            
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (fi.Length < minimumFileSizeKB * 1024)
                    {                        
                        return;
                    }
                }
                catch (Exception)
                {
                    Logger.LogWarning($"Skipping {file}, unable to read");
                    return;
                }

                var ext = Path.GetExtension(file).ToLower();

                FileWrapper processedFile;
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                        processedFile = new JpgFile(file); break;
                    case ".png":
                        processedFile = new PngFile(file); break;
                    case ".nef":
                        processedFile = new NefFile(file); break;
                    case ".raw":
                    case ".rw2":
                        processedFile = new RawFile(file); break;
                    case ".avi":
                    case ".wmv":
                    case ".mpg":
                    case ".mpeg":
                    case ".mp4":
                    case ".mov":
                        processedFile = new VideoFile(file); break;
                    default:
                        Logger.LogWarning($"Skipping {file}");
                        return;
                }

                if (!dict.ContainsKey(processedFile.FileType))
                {
                    dict.TryAdd(processedFile.FileType, new ConcurrentBag<FileWrapper>());
                }

                if (processedFile.Hash == null)
                {
                    Logger.LogWarning($"Unable to hash {file}");
                    return;
                }
                if (dict[processedFile.FileType].Any(x => x.Hash.AsHexString() == processedFile.Hash.AsHexString()))
                {
                    Logger.LogWarning($"Duplicate {file}");
                    return;
                }

                dict[processedFile.FileType].Add(processedFile);

            });

            return dict.ToDictionary(x => x.Key, x => x.Value.ToList());
        }
    }
}
