using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GRPExplorerLib.Util
{
    public static class StringUtil
    {
        public static string EncodeToGoodString(this byte[] bytes)
        {
            if (bytes == null)
                return "";

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x00)
                {
                    break;
                }

                if (bytes[i] == 0x3A)
                    bytes[i] = 0x20;

                builder.Append(Convert.ToChar(bytes[i]));
            }
            return builder.ToString().Trim();
        }

        public static byte[] EncodeToBadString(this string str, byte[] buffer = null, int length = -1)
        {
            if (length == -1)
                length = str.Length;

            if (buffer == null)
                buffer = new byte[length];

            if (str.Length > length)
                throw new Exception("String is longer than calculated length! (" + length + ")");

            for (int i = 0; i < str.Length; i++)
            {
                buffer[i] = Convert.ToByte(str[i]);
            }

            for (int i = str.Length; i < buffer.Length; i++)
            {
                buffer[i] = 0x00;
            }

            return buffer;
        }

        public static string ReadNullTerminatedString(Stream stream)
        {
            StringBuilder builder = new StringBuilder();
            byte b;
            while ((b = (byte)stream.ReadByte()) != 0x00)
                builder.Append(Convert.ToChar(b));
            return builder.ToString();
        }
    }
}
