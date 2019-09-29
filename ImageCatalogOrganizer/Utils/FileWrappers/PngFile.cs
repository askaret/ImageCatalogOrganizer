using System;

namespace Utils
{
    public class PngFile : FileWrapper
    {
        public PngFile(string filePath) : base(filePath, FileTypeCode.PNG)
        {
        }

    }
}
