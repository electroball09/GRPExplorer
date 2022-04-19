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
    public struct BigFileFileRead
    {
        public static BigFileFileRead MakeError(YetiObject obj)
        {
            return new BigFileFileRead()
            {
                file = obj,
                header = null,
                buffer = null,
                dataSize = -1
            };
        }

        public bool IsError()
        {
            return 
                header == null &&
                buffer == null &&
                dataSize == -1;
        }

        //public static bool operator ==(BigFileFileRead readA, BigFileFileRead readB)
        //{
        //    return
        //        readA.file == readB.file &&
        //        readA.header == readB.header &&
        //        readA.buffer == readB.buffer &&
        //        readA.dataSize == readB.dataSize;
        //}

        //public static bool operator !=(BigFileFileRead readA, BigFileFileRead readB)
        //{
        //    return
        //        readA.file != readB.file ||
        //        readA.header != readB.header ||
        //        readA.buffer != readB.buffer ||
        //        readA.dataSize != readB.dataSize;
        //}

        public YetiObject file;
        public int[] header;
        public byte[] buffer;
        public int dataSize;
    }

    public abstract class BigFileFileReader
    {
        protected BigFile bigFile;
        public BigFile BigFile { get { return bigFile; } }

        public abstract BigFileFlags DefaultFlags { get; }

        internal BigFileFileReader(BigFile _bigFile)
        {
            bigFile = _bigFile;
        }

        public abstract BigFileFileRead ReadFile(YetiObject file, IOBuffers buffers, BigFileFlags flags);
        public abstract IEnumerable<BigFileFileRead> ReadAllFiles(List<YetiObject> files, IOBuffers buffers, BigFileFlags flags);

        //    public abstract int ReadFileRaw(YetiObject file, IOBuffers buffers, BigFileFlags flags);
        //    public abstract int[] ReadFileHeader(YetiObject file, IOBuffers buffers, BigFileFlags flags);
        //    public abstract int ReadFileData(YetiObject file, IOBuffers buffers, BigFileFlags flags);

        //    public abstract IEnumerable<int> ReadAllRaw(YetiObject[] filesToRead, IOBuffers buffers, BigFileFlags flags);
        //    public abstract IEnumerable<int[]> ReadAllHeaders(YetiObject[] filesToRead, IOBuffers buffers, BigFileFlags flags);
        //    public abstract IEnumerable<int> ReadAllData(YetiObject[] filesToRead, IOBuffers buffers, BigFileFlags flags);
    }
}
