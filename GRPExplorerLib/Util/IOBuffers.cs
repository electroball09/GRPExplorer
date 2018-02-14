using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Util
{
    public struct FileBuffer
    {
        public byte[] bytes;
        public int size;

        public FileBuffer(byte[] _bytes, int _size)
        {
            bytes = _bytes;
            size = _size;
        }
    }

    public class IOBuffers
    {
        public const int KB = 1024;
        public const int MB = 1024 * KB;

        private byte[][] buffers = new byte[][]
        {
            new byte[4],
            new byte[64],
            new byte[512],
            new byte[KB * 4],
            new byte[KB * 512],
            new byte[MB * 4],
            new byte[MB * 36],
        };

        public byte[] this[int size]
        {
            get
            {
                return GetBuffer(size);
            }
        }

        public byte[] this[uint size]
        {
            get
            {
                return GetBuffer(size);
            }
        }

        public byte[] this[long size]
        {
            get
            {
                return GetBuffer(size);
            }
        }

        public byte[] GetBuffer(int size)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                if (size <= buffers[i].Length)
                    return buffers[i];
            }

            throw new Exception("No buffer found for size: " + size);
        }

        public byte[] GetBuffer(uint size)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                if (size <= buffers[i].Length)
                    return buffers[i];
            }

            throw new Exception("No buffer found for size: " + size);
        }

        public byte[] GetBuffer(long size)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                if (size <= buffers[i].Length)
                    return buffers[i];
            }

            throw new Exception("No buffer found for size: " + size);
        }
    }
}
