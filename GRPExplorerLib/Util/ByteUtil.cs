using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Util
{
    public static class ByteUtil
    {
        public static byte[] ToByteArray(this int value, byte[] array, int startIndex = 0)
        {
            //halp the endians are coming
            array[startIndex + 0] = (byte)(value >> 0);
            array[startIndex + 1] = (byte)(value >> 8);
            array[startIndex + 2] = (byte)(value >> 16);
            array[startIndex + 3] = (byte)(value >> 24);
            return array;
        }
    }
}
