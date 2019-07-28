using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class RawFile : FileWrapper
    {
        public RawFile(string filePath) : base(filePath, FileTypeCode.RAW)
        {
        }

        public override DateTime GetCreationTime()
        {
            return DateTime.MinValue;
        }
    }
}
