using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.HashFunction.xxHash;
using System.Data.HashFunction;

namespace Utils
{
    public enum FileTypeCode
    {
        RW2,
        RAW,
        JPG,
        Video,
        Unsupported
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
            Hash = xxHashFactory.Instance.Create(new HashConfig()).ComputeHash(System.IO.File.ReadAllBytes(FilePath));                        
        }
        public abstract DateTime GetCreationTime();
        
    }
}
