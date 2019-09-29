using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.HashFunction.xxHash;
using System.Data.HashFunction;
using ImgCli;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using MetadataExtractor;

namespace Utils
{
    public enum FileTypeCode
    {
        RW2,
        RAW,
        JPG,
        PNG,
        Video,
        Unsupported,
        NEF
    }

    public abstract class FileWrapper
    {
        public FileTypeCode FileType { get; private set; }
        public System.Data.HashFunction.IHashValue Hash { get; private set; }
        public string FilePath { get; private set; }
        public FileWrapper(string filePath, FileTypeCode fileType)
        {
            FileType = fileType;
            FilePath = filePath;

            try
            {
                // No real hashing of videos
                Hash = FileType == FileTypeCode.Video
                    ? xxHashFactory.Instance.Create(new HashConfig()).ComputeHash(System.IO.Path.GetFileName(FilePath) + System.IO.File.GetCreationTime(FilePath).ToString())
                    : xxHashFactory.Instance.Create(new HashConfig()).ComputeHash(System.IO.File.ReadAllBytes(FilePath));
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Error processing {filePath}: {e}");
            }

        }
        private static Regex r = new Regex(":");
        private DateTime createdDate = DateTime.MinValue;
        public DateTime GetCreationTime()
        {
            if (createdDate != DateTime.MinValue)
                return createdDate;

            try
            {
                var metadata = ImageMetadataReader.ReadMetadata(FilePath);

                var date = metadata.First(x => x.Name == "Exif IFD0").Tags.First(y => y.Name == "Date/Time").Description;
                var dateTakenStr = r.Replace(date, "-", 2);
                createdDate = DateTime.Parse(dateTakenStr);
            }
            catch (Exception)
            {
                Logger.LogError($"Unable to find date taken, using file created date which is wrong for {FilePath}");
                createdDate = File.GetCreationTime(FilePath);
            }

             return createdDate;
        }
    }
}
