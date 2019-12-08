using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Logging
{
    internal class ConsoleLogInterface : IGRPExplorerLibLogInterface
    {
        public LogFlags CombineFlags(LogFlags original)
        {
            return original;
        }

        public void Debug(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Error(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Info(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
