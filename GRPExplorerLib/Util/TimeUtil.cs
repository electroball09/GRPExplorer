using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Util
{
    public static class TimeUtil
    {
        public static DateTime UnixTime(this int seconds)
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            return epoch.AddSeconds(seconds);
        }
    }
}
