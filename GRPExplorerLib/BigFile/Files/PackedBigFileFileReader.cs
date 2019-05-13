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
    public sealed class PackedBigFileFileReader : BigFileFileReader
    {
        public const BigFileFlags DEFAULT_FLAGS = BigFileFlags.Decompress;

        public override BigFileFlags DefaultFlags
        {
            get
            {
                return DEFAULT_FLAGS;
            }
        }

        private PackedBigFile packedBigFile;
        private ILogProxy log = LogManager.GetLogProxy("PackedBigFileFileReader");
        private byte[] sizeBuffer = new byte[10];

        public PackedBigFileFileReader(PackedBigFile _bigFile) : base(_bigFile)
        {
            packedBigFile = _bigFile;
        }

        public int GetBytesSizeAndSeekToStartOfFileData(Stream stream, BigFileFile file, BigFileFlags flags = DEFAULT_FLAGS)
        {
            if (file.FileInfo.Offset == -1)
            {
                log.Error(string.Format("Can't seek to file: {0} (key:{1:X8}) because offset is -1!", file.Name, file.FileInfo.Key));
                return -1;
            }

            int dataOffset = packedBigFile.FileUtil.CalculateDataOffset(ref packedBigFile.SegmentHeader, ref packedBigFile.FileHeader);

            stream.Seek((uint)dataOffset + (uint)(file.FileInfo.Offset * 8), SeekOrigin.Begin);

            int size = -1;

            if (file.FileInfo.ZIP == 0)
            {
                stream.Read(sizeBuffer, 0, 4);
                size = BitConverter.ToInt32(sizeBuffer, 0);
            }
            else
            {
                stream.Read(sizeBuffer, 0, 8); //4 bytes for on disk size, 4 bytes for decompressed size

                if ((flags & BigFileFlags.Decompress) != 0)
                    size = BitConverter.ToInt32(sizeBuffer, 4);
                else
                    size = BitConverter.ToInt32(sizeBuffer, 0) - 4; //subtract 4 because yeti engine takes the decompressed size with the actual file data
            }

            if (size == 0)
                throw new Exception("wtf");

            return size;
        }

        private int internal_ReadFileRaw(Stream stream, BigFileFile file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            log.Debug("Reading file: " + file.Name);

            int bytesSize = GetBytesSizeAndSeekToStartOfFileData(stream, file, flags);

            if (bytesSize == -1)
            {
                log.Error("There was an error reading the file!");
                return -1;
            }

            byte[] buffer = buffers[bytesSize];

            log.Debug(string.Format("    > [ZIP: {0}] [flags: {1}] [size: {2}]", file.FileInfo.ZIP, flags, bytesSize));

            if (file.FileInfo.ZIP == 0)
            {
                stream.Read(buffer, 0, bytesSize);
            }
            else
            {
                if ((flags & BigFileFlags.Decompress) != 0)
                {
                    using (ZlibStream zs = new ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress, true))
                    {
                        zs.Read(buffer, 0, bytesSize);
                    }
                }
                else
                {
                    stream.Read(buffer, 0, bytesSize);
                }
            }

            return bytesSize;
        }

        private int[] internal_ReadFileHeader(Stream stream, BigFileFile file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            int fileSize = internal_ReadFileRaw(stream, file, buffers, flags);
            if (fileSize == -1)
                return new int[0];

            int referenceNum = BitConverter.ToInt32(buffers[fileSize], 0);
            if (referenceNum * 4 >= fileSize)
            {
                log.Error("referenceNum * 4 > fileSize\n{0}>{1}", referenceNum, fileSize);
                return new int[0];
            }

            int[] header = new int[referenceNum];

            for (int i = 0; i < referenceNum; i++)
            {
                header[i] = BitConverter.ToInt32(buffers[fileSize], (i * 4) + 4);
            }

            return header;
        }

        private int internal_ReadFileData(Stream stream, BigFileFile file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            int fileSize = internal_ReadFileRaw(stream, file, buffers, flags);
            if (fileSize == -1)
                return -1;

            int referenceNum = BitConverter.ToInt32(buffers[fileSize], 0);
            if (referenceNum * 4 >= fileSize)
            {
                log.Error("referenceNum * 4 > fileSize\n{0}>{1}", referenceNum, fileSize);
                return -1;
            }

            int dataSize = fileSize - (referenceNum + 1) * 4;
            byte[] dataBuffer = buffers[dataSize];
            byte[] fileBuffer = buffers[fileSize];

            for (int i = 0; i < dataSize; i++)
            {
                dataBuffer[i] = fileBuffer[i + (referenceNum + 1) * 4];
            }

            return dataSize;
        }

        public override int ReadFileRaw(BigFileFile file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            int size = -1;
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                size = internal_ReadFileRaw(fs, file, buffers, flags);
            }
            return size;
        }

        public override int[] ReadFileHeader(BigFileFile file, IOBuffers buffers, BigFileFlags flags)
        {
            int[] header;
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                header = internal_ReadFileHeader(fs, file, buffers, flags);
            }
            return header;
        }

        public override int ReadFileData(BigFileFile file, IOBuffers buffers, BigFileFlags flags)
        {
            int size = -1;
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                size = internal_ReadFileData(fs, file, buffers, flags);
            }
            return size;
        }

        public override IEnumerable<int> ReadAllRaw(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                for (int i = 0; i < filesToRead.Length; i++)
                {
                    yield return internal_ReadFileRaw(fs, filesToRead[i], buffers, flags);
                }
            }
        }

        public override IEnumerable<int[]> ReadAllHeaders(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags)
        {
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                for (int i = 0; i < filesToRead.Length; i++)
                {
                    yield return internal_ReadFileHeader(fs, filesToRead[i], buffers, flags);
                }
            }
        }

        public override IEnumerable<int> ReadAllData(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags)
        {
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                for (int i = 0; i < filesToRead.Length; i++)
                {
                    yield return internal_ReadFileData(fs, filesToRead[i], buffers, flags);
                }
            }
        }
    }
}
