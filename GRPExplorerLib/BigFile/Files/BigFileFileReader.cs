using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;
using Ionic.Zlib;

namespace GRPExplorerLib.BigFile.Files
{
    public abstract class BigFileFileReader
    {
        protected BigFile bigFile;
        public BigFile BigFile { get { return bigFile; } }

        public abstract BigFileFlags DefaultFlags { get; }

        internal BigFileFileReader(BigFile _bigFile)
        {
            bigFile = _bigFile;
        }
        
        public abstract int ReadFileRaw(BigFileFile file, IOBuffers buffers, BigFileFlags flags);
        public abstract int[] ReadFileHeader(BigFileFile file, IOBuffers buffers, BigFileFlags flags);
        public abstract int ReadFileData(BigFileFile file, IOBuffers buffers, BigFileFlags flags);

        public abstract IEnumerable<int> ReadAllRaw(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags);
        public abstract IEnumerable<int[]> ReadAllHeaders(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags);
        public abstract IEnumerable<int> ReadAllData(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags);
    }
}
