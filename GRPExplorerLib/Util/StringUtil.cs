using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Util
{
    public static class StringUtil
    {
        public static string EncodeToGoodString(this byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x00)
                {
                    return builder.ToString();
                }

                builder.Append(Convert.ToChar(bytes[i]));
            }
            return builder.ToString();
        }
    }
}
