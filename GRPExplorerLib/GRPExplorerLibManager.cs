using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib
{
    public class GRPExplorerLibManager
    {
        public static void SetLogInterface(IGRPExplorerLibLogInterface logInterface)
        {
            GLog.logInterface = logInterface;
        }

        public static void SetLogFlags(LogFlags flags)
        {
            GLog.logFlags = flags;
        }
    }

    internal static class GLog
    {
        internal static IGRPExplorerLibLogInterface logInterface = new DefaultLogInterface();
        internal static LogFlags logFlags = LogFlags.Debug | LogFlags.Info | LogFlags.Error;

        public static void Debug(string msg)
        {
            if ((logFlags & LogFlags.Debug) != 0)
                logInterface.Debug(msg);
        }

        public static void Info(string msg)
        {
            if ((logFlags & LogFlags.Info) != 0)
                logInterface.Info(msg);
        }

        public static void Error(string msg)
        {
            if ((logFlags & LogFlags.Error) != 0)
                logInterface.Error(msg);
        }
    }

    internal class DefaultLogInterface : IGRPExplorerLibLogInterface
    {
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

    [Flags]
    public enum LogFlags : uint
    {
        None = 0x0,
        All = 0xffffffff,
        Debug = 0x1,
        Info = 0x2,
        Error = 0x4
    }
}
