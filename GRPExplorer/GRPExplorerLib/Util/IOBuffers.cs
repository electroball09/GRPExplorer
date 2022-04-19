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

        static readonly int[] sizes = new int[]
        {
                   4, // 1/2 byte
                  64, // 8 byte
                 512, // 1/2 kilobyte
            KB *   4, // 4 kilobyte
            KB * 512, // 1/2 megabyte
            MB *   4, // 4 megabyte
            MB *  36  // 36 megabyte (who needs RAM anyways)
        };

        private byte[][] buffers;

        public IOBuffers()
        {
            buffers = new byte[sizes.Length][];
        }

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
            return GetBuffer((long)size);
        }

        public byte[] GetBuffer(uint size)
        {
            return GetBuffer((long)size);
        }

        public byte[] GetBuffer(long size)
        {
            for (int i = 0; i < sizes.Length; i++)
            {
                if (size <= sizes[i])
                {
                    if (buffers[i] == null)
                    {
                        buffers[i] = new byte[sizes[i]];
                    }

                    return buffers[i];
                }
            }

            throw new Exception("No buffer found for size: " + size);
        }
    }
}
