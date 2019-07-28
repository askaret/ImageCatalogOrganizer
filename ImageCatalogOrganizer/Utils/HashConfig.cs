using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.HashFunction.xxHash;

namespace Utils
{
    public class HashConfig : IxxHashConfig
    {
        private ulong _seed = 0;
        public HashConfig(ulong seed = 0)
        {
            var r = new Random();
            _seed = seed != 0 ? seed : (ulong) r.Next();
        }
        public int HashSizeInBits => 64;

        public ulong Seed => _seed; 

        public IxxHashConfig Clone()
        {
            return new HashConfig(_seed);
        }
    }
}
