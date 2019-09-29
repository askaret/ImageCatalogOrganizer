using System;

namespace Utils
{
    public class NefFile : FileWrapper
    {
        public NefFile(string filePath) : base(filePath, FileTypeCode.NEF)
        {
        }

    }
}
