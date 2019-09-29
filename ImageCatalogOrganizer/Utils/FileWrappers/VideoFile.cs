using System;

namespace Utils
{
    public class VideoFile : FileWrapper
    {
        public VideoFile(string filePath) : base(filePath, FileTypeCode.Video)
        {
        }

    }
}
