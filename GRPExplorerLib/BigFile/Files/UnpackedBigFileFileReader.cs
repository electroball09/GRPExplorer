using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;
using Ionic.Zlib;

namespace GRPExplorerLib.BigFile.Files
{
    public sealed class UnpackedBigFileFileReader : BigFileFileReader
    {
        private UnpackedBigFile unpackedBigFile;
        private ILogProxy log = LogManager.GetLogProxy("UnpackedBigFileFileReader");

        public override BigFileFlags DefaultFlags
        {
            get
            {
                return BigFileFlags.None;
            }
        }

        public UnpackedBigFileFileReader(UnpackedBigFile _bigFile) : base(_bigFile)
        {
            unpackedBigFile = _bigFile;
        }

        public override int ReadFileRaw(BigFileFile file, IOBuffers buffers, BigFileFlags flags)
        {
            int[] header = ReadFileHeader(file, buffers, flags);
            int dataSize = ReadFileData(file, buffers, flags);
            int headerSize = header.Length * 4 + 4;

            byte[] dataBuffer = buffers[dataSize];
            byte[] buffer = buffers[dataSize + headerSize];
            for (int i = 0; i < dataSize; i++)
            {
                buffer[i + headerSize] = dataBuffer[i];
            }


            header.Length.ToByteArray(buffer, 0);
            for (int i = 0; i < header.Length; i++)
            {
                header[i].ToByteArray(buffer, i * 4 + 4);
            }

            return dataSize + headerSize;
        }

        public override int[] ReadFileHeader(BigFileFile file, IOBuffers buffers, BigFileFlags flags)
        {
            string fileName = unpackedBigFile.Directory.FullName + "\\"
                                + BigFileConst.UNPACK_DIR
                                + unpackedBigFile.RenamedMapping[file.FileInfo.Key].FileName
                                + BigFileConst.UNPACKED_HEADER_FILE_EXTENSION;

            int fileSize = -1;
            byte[] buffer;
            using (FileStream fs = File.OpenRead(fileName))
            {
                fileSize = (int)fs.Length;
                buffer = buffers[fileSize];
                fs.Read(buffer, 0, fileSize);
            }

            if (fileSize == 0)
                return new int[0];

            int referenceNum = BitConverter.ToInt32(buffer, 0);
            if (referenceNum * 4 + 4 > fileSize)
                throw new Exception("referenceNum * 4 + 4 > fileSize");

            if (referenceNum <= 0)
                return new int[0];

            int[] header = new int[referenceNum];
            for (int i = 1; i < referenceNum; i++)
            {
                header[i - 1] = BitConverter.ToInt32(buffer, i * 4);
            }
            return header;
        }

        public override int ReadFileData(BigFileFile file, IOBuffers buffers, BigFileFlags flags)
        {
            string fullFileName = unpackedBigFile.Directory.FullName
                                    + "\\" + BigFileConst.UNPACK_DIR
                                    + unpackedBigFile.RenamedMapping[file.FileInfo.Key].FileName;

            int size = -1;
            byte[] buffer = buffers[4];
            using (FileStream fs = File.OpenRead(fullFileName))
            {
                size = (int)fs.Length;
                buffer = buffers[size];
                fs.Read(buffer, 0, size);
            }

            if (size == 0)
                size = -1;

            return size;
        }

        public override IEnumerable<int> ReadAllRaw(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags = BigFileFlags.None)
        {
            for (int i = 0; i < filesToRead.Length; i++)
            {
                yield return ReadFileRaw(filesToRead[i], buffers, flags);
            }
        }

        public override IEnumerable<int> ReadAllData(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags)
        {
            for (int i = 0; i < filesToRead.Length; i++)
            {
                yield return ReadFileData(filesToRead[i], buffers, flags);
            }
        }

        public override IEnumerable<int[]> ReadAllHeaders(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags)
        {
            for (int i = 0; i < filesToRead.Length; i++)
            {
                yield return ReadFileHeader(filesToRead[i], buffers, flags);
            }
        }
    }
}
