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

        private BigFileFileRead internal_ReadFile(Stream stream, YetiObject file, IOBuffers buffers, BigFileFlags flags)
        {
            log.Debug("Reading file {0}", file.NameWithExtension);

            if (file.FileInfo.Offset == -1)
            {
                log.Error(string.Format("Can't seek to file: {0} (key:{1:X8}) because offset is -1!", file.Name, file.FileInfo.Key));
                return BigFileFileRead.Error;
            }

            int dataOffset = packedBigFile.FileUtil.CalculateDataOffset(ref packedBigFile.SegmentHeader, ref packedBigFile.FileHeader);

            stream.Seek(dataOffset + (file.FileInfo.Offset * 8), SeekOrigin.Begin);

            int dataSize = -1;
            int[] header;

            //read the file
            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                if (file.FileInfo.ZIP == 0)
                {
                    dataSize = br.ReadInt32();

                    stream.Read(buffers[dataSize], 0, dataSize);
                }
                else
                {
                    int compressedSize = br.ReadInt32();
                    dataSize = br.ReadInt32();

                    using (ZlibStream zs = new ZlibStream(stream, CompressionMode.Decompress, true))
                    {
                        zs.Read(buffers[dataSize], 0, dataSize);
                    }
                }
            }

            log.Debug(string.Format("    > [ZIP: {0}] [flags: {1}] [size: {2}]", file.FileInfo.ZIP, flags, dataSize));

            //read the object references
            using (MemoryStream ms = new MemoryStream(buffers[dataSize]))
            using (BinaryReader brms = new BinaryReader(ms))
            {
                int referenceNum = brms.ReadInt32();

                if (referenceNum * 4 >= dataSize)
                {
                    log.Error("{2} {3} referenceNum * 4 > fileSize [ {0} * 4 > {1} ]", referenceNum, dataSize, file.FullFolderPath, file.NameWithExtension);
                    return BigFileFileRead.Error;
                }

                header = new int[referenceNum];

                for (int i = 0; i < referenceNum; i++)
                    header[i] = brms.ReadInt32();

                int origDataSize = dataSize;
                dataSize -= referenceNum * 4 + 4;

                Array.Copy(buffers[origDataSize], referenceNum * 4 + 4, buffers[dataSize], 0, dataSize);
            }

            return new BigFileFileRead()
            {
                header = header,
                buffer = buffers[dataSize],
                dataSize = dataSize
            };
        }

        public override BigFileFileRead ReadFile(YetiObject file, IOBuffers buffers, BigFileFlags flags)
        {
            BigFileFileRead fileRead;
            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                fileRead = internal_ReadFile(fs, file, buffers, flags);
            }
            return fileRead;
        }

        public override IEnumerable<BigFileFileRead> ReadAllFiles(List<YetiObject> files, IOBuffers buffers, BigFileFlags flags)
        {
            //sort the files by location in the bigfile to avoid unnecessary seeks
            files.Sort(
                (a, b) =>
                {
                    if (a.FileInfo.Offset == -1)
                        return 1;
                    if (b.FileInfo.Offset == -1)
                        return -1;

                    return a.FileInfo.Offset - b.FileInfo.Offset;
                });

            using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
            {
                foreach (YetiObject file in files)
                {
                    yield return internal_ReadFile(fs, file, buffers, flags);
                }
            }
        }

    //    public int GetBytesSizeAndSeekToStartOfFileData(Stream stream, YetiObject file, BigFileFlags flags = DEFAULT_FLAGS)
    //    {
    //        if (file.FileInfo.Offset == -1)
    //        {
    //            log.Error(string.Format("Can't seek to file: {0} (key:{1:X8}) because offset is -1!", file.Name, file.FileInfo.Key));
    //            return -1;
    //        }

    //        int dataOffset = packedBigFile.FileUtil.CalculateDataOffset(ref packedBigFile.SegmentHeader, ref packedBigFile.FileHeader);

    //        stream.Seek((uint)dataOffset + (uint)(file.FileInfo.Offset * 8), SeekOrigin.Begin);

    //        int size;

    //        if (file.FileInfo.ZIP == 0)
    //        {
    //            stream.Read(sizeBuffer, 0, 4);
    //            size = BitConverter.ToInt32(sizeBuffer, 0);
    //        }
    //        else
    //        {
    //            stream.Read(sizeBuffer, 0, 8); //4 bytes for on disk size, 4 bytes for decompressed size

    //            if ((flags & BigFileFlags.Decompress) != 0)
    //                size = BitConverter.ToInt32(sizeBuffer, 4);
    //            else
    //                size = BitConverter.ToInt32(sizeBuffer, 0) - 4; //subtract 4 because yeti engine takes the decompressed size with the actual file data
    //        }

    //        if (size == 0)
    //            throw new Exception("wtf");

    //        return size;
    //    }

    //    private int internal_ReadFileRaw(Stream stream, YetiObject file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
    //    {
    //        log.Debug("Reading file: " + file.Name);

    //        int bytesSize = GetBytesSizeAndSeekToStartOfFileData(stream, file, flags);

    //        if (bytesSize == -1)
    //        {
    //            return -1;
    //        }

    //        byte[] buffer = buffers[bytesSize];

    //        log.Debug(string.Format("    > [ZIP: {0}] [flags: {1}] [size: {2}]", file.FileInfo.ZIP, flags, bytesSize));

    //        if (file.FileInfo.ZIP == 0)
    //        {
    //            stream.Read(buffer, 0, bytesSize);
    //        }
    //        else
    //        {
    //            if ((flags & BigFileFlags.Decompress) != 0)
    //            {
    //                using (ZlibStream zs = new ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress, true))
    //                {
    //                    zs.Read(buffer, 0, bytesSize);
    //                }
    //            }
    //            else
    //            {
    //                stream.Read(buffer, 0, bytesSize);
    //            }
    //        }

    //        return bytesSize;
    //    }

    //    private int[] internal_ReadFileHeader(Stream stream, YetiObject file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
    //    {
    //        int fileSize = internal_ReadFileRaw(stream, file, buffers, flags);
    //        if (fileSize == -1)
    //            return new int[0];

    //        int referenceNum = BitConverter.ToInt32(buffers[fileSize], 0);
    //        if (referenceNum * 4 >= fileSize)
    //        {
    //            log.Error("{2} {3} referenceNum * 4 > fileSize\n{0}>{1}", referenceNum, fileSize, file.FullFolderPath, file.NameWithExtension);
    //            return new int[0];
    //        }

    //        int[] header = new int[referenceNum];

    //        for (int i = 0; i < referenceNum; i++)
    //        {
    //            header[i] = BitConverter.ToInt32(buffers[fileSize], (i * 4) + 4);
    //        }

    //        return header;
    //    }

    //    private int internal_ReadFileData(Stream stream, YetiObject file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
    //    {
    //        int fileSize = internal_ReadFileRaw(stream, file, buffers, flags);
    //        if (fileSize == -1)
    //            return -1;

    //        int referenceNum = BitConverter.ToInt32(buffers[fileSize], 0);
    //        if (referenceNum * 4 >= fileSize)
    //        {
    //            log.Error("referenceNum * 4 > fileSize\n{0}>{1}", referenceNum, fileSize);
    //            return -1;
    //        }

    //        int dataSize = fileSize - (referenceNum + 1) * 4;
    //        byte[] dataBuffer = buffers[dataSize];
    //        byte[] fileBuffer = buffers[fileSize];

    //        for (int i = 0; i < dataSize; i++)
    //        {
    //            dataBuffer[i] = fileBuffer[i + (referenceNum + 1) * 4];
    //        }

    //        return dataSize;
    //    }

    //    public override int ReadFileRaw(YetiObject file, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
    //    {
    //        int size = -1;
    //        using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
    //        {
    //            size = internal_ReadFileRaw(fs, file, buffers, flags);
    //        }
    //        return size;
    //    }

    //    public override int[] ReadFileHeader(YetiObject file, IOBuffers buffers, BigFileFlags flags)
    //    {
    //        int[] header;
    //        using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
    //        {
    //            header = internal_ReadFileHeader(fs, file, buffers, flags);
    //        }
    //        return header;
    //    }

    //    public override int ReadFileData(YetiObject file, IOBuffers buffers, BigFileFlags flags)
    //    {
    //        int size = -1;
    //        using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
    //        {
    //            size = internal_ReadFileData(fs, file, buffers, flags);
    //        }
    //        return size;
    //    }

    //    public override IEnumerable<int> ReadAllRaw(YetiObject[] filesToRead, IOBuffers buffers, BigFileFlags flags = DEFAULT_FLAGS)
    //    {
    //        using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
    //        {
    //            for (int i = 0; i < filesToRead.Length; i++)
    //            {
    //                yield return internal_ReadFileRaw(fs, filesToRead[i], buffers, flags);
    //            }
    //        }
    //    }

    //    public override IEnumerable<int[]> ReadAllHeaders(YetiObject[] filesToRead, IOBuffers buffers, BigFileFlags flags)
    //    {
    //        using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
    //        {
    //            for (int i = 0; i < filesToRead.Length; i++)
    //            {
    //                yield return internal_ReadFileHeader(fs, filesToRead[i], buffers, flags);
    //            }
    //        }
    //    }

    //    public override IEnumerable<int> ReadAllData(YetiObject[] filesToRead, IOBuffers buffers, BigFileFlags flags)
    //    {
    //        using (FileStream fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName))
    //        {
    //            for (int i = 0; i < filesToRead.Length; i++)
    //            {
    //                yield return internal_ReadFileData(fs, filesToRead[i], buffers, flags);
    //            }
    //        }
    //    }
    }
}
