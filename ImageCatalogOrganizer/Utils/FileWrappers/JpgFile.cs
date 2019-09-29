using System;

namespace Utils
{
    public class JpgFile : FileWrapper
    {
        public JpgFile(string filePath) : base(filePath, FileTypeCode.JPG)
        {
        }

    }
}
