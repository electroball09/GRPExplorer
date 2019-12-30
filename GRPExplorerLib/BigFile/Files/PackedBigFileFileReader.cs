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
        private FileStream _fs;
        private FileStream FileStream
        {
            get
            {
                if (_fs == null)
                {
                    _fs = File.OpenRead(packedBigFile.MetadataFileInfo.FullName);
                }

                return _fs;
            }
        }

        public PackedBigFileFileReader(PackedBigFile _bigFile) : base(_bigFile)
        {
            packedBigFile = _bigFile;
        }

        ~PackedBigFileFileReader()
        {
            if (_fs != null)
                _fs.Close();

            log.Info("Closing file!");
        }

        private BigFileFileRead internal_ReadFile(Stream stream, YetiObject file, IOBuffers buffers, BigFileFlags flags)
        {
            log.Debug("Reading file {0}", file.NameWithExtension);

            if (file.FileInfo.Offset < 0)
            {
                if (file.FileInfo.FileType == YetiObjects.YetiObjectType.vxt) //all files of this type have offset of -1
                    return BigFileFileRead.Error;

                log.Error(string.Format("Can't seek to file: {0} (key:{1:X8}) because offset is {2}!", file.Name, file.FileInfo.Key, file.FileInfo.Offset));
                return BigFileFileRead.Error;
            }

            int dataOffset = packedBigFile.FileUtil.CalculateDataOffset(ref packedBigFile.SegmentHeader, ref packedBigFile.FileHeader);

            stream.Seek(dataOffset + (file.FileInfo.Offset * 8L), SeekOrigin.Begin); //make sure it converts to LONG to avoid overflow

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

                if (dataSize > 0)
                    Array.Copy(buffers[origDataSize], referenceNum * 4 + 4, buffers[dataSize], 0, dataSize);
            }

            return new BigFileFileRead()
            {
                file = file,
                header = header,
                buffer = buffers[dataSize],
                dataSize = dataSize
            };
        }

        public override BigFileFileRead ReadFile(YetiObject file, IOBuffers buffers, BigFileFlags flags)
        {
            BigFileFileRead fileRead;
            fileRead = internal_ReadFile(FileStream, file, buffers, flags);
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

            foreach (YetiObject file in files)
            {
                yield return internal_ReadFile(FileStream, file, buffers, flags);
            }
        }
    }
}
