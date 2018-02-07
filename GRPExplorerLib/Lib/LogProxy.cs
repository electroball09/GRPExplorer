using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib
{
    internal class LogProxy
    {
        private string prefix = "";

        public LogProxy(string _prefix)
        {
            prefix = "[" + _prefix + "]";
        }

        public void Debug(string msg)
        {
            GLog.Debug(prefix + " " + msg);
        }

        public void Info(string msg)
        {
            GLog.Info(prefix + " " + msg);
        }

        public void Error(string msg)
        {
            GLog.Error(prefix + " " + msg);
        }
    }
}
