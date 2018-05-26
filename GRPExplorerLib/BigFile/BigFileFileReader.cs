using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;
using Ionic.Zlib;

namespace GRPExplorerLib.BigFile
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

        //public abstract int GetBytesSizeAndSeekToStartOfFileData(Stream stream, BigFileFile file, BigFileFlags flags = BigFileFlags.None);
        public abstract int ReadFile(BigFileFile file, IOBuffers buffers, BigFileFlags flags = BigFileFlags.None);
        public abstract int ReadFile(Stream stream, BigFileFile file, IOBuffers buffers, BigFileFlags flags = BigFileFlags.None);
        public abstract IEnumerable<int> ReadAll(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags = BigFileFlags.None);
        public abstract Stream OpenStream(BigFileFile file, BigFileFlags flags = BigFileFlags.None);
        public abstract Stream OpenStream(Stream stream, BigFileFile file, BigFileFlags flags = BigFileFlags.None);
    }

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

            int dataOffset = packedBigFile.YetiHeaderFile.CalculateDataOffset(ref packedBigFile.FileHeader, ref packedBigFile.CountInfo);

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
                    size = BitConverter.ToInt32(sizeBuffer, 0);
            }

            if (size == 0)
                throw new Exception("wtf");

            return size;
        }

        public override int ReadFile(Stream stream, BigFileFile file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
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

        public override int ReadFile(BigFileFile file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            int size = -1;
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                size = ReadFile(fs, file, buffers, flags);
            }
            return size;
        }

        public override IEnumerable<int> ReadAll(BigFileFile[] filesToRead, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
        {
            int size = -1;
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                for (int i = 0; i < filesToRead.Length; i++)
                {
                    size = ReadFile(fs, filesToRead[i], buffers, flags);
                    yield return size;
                }
            }
        }

        public override Stream OpenStream(BigFileFile file, BigFileFlags flags = DEFAULT_FLAGS)
        {
            throw new NotImplementedException();
        }

        public override Stream OpenStream(Stream stream, BigFileFile file, BigFileFlags flags = DEFAULT_FLAGS)
        {
            throw new NotImplementedException();
        }
    }
}
