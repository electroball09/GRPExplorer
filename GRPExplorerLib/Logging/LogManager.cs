using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.Logging
{
    public static class LogManager
    {
        private class LogProxy : ILogProxy
        {
            private string prefix = "";
            private IGRPExplorerLibLogInterface logInterface;
            private LogFlags logFlags = LogFlags.All;
            public LogFlags LogFlags { get { return logFlags; } set { logFlags = value; } }

            internal LogProxy(string _prefix, IGRPExplorerLibLogInterface _logInterface)
            {
                prefix = "[" + _prefix + "]";
                logInterface = _logInterface;
            }

            public void Debug(string msg)
            {
                if ((logFlags & LogFlags.Debug) != 0 && (LogManager.GlobalLogFlags & LogFlags.Debug) != 0)
                {
                    if (logInterface == null)
                        LogManager.Debug(prefix + " " + msg);
                    else
                        logInterface.Debug(prefix + " " + msg);
                }
            }

            public void Debug(string formattedMsg, params object[] args)
            {
                Debug(string.Format(formattedMsg, args));
            }

            public void Info(string msg)
            {
                if ((logFlags & LogFlags.Info) != 0 && (LogManager.GlobalLogFlags & LogFlags.Info) != 0)
                {
                    if (logInterface == null)
                        LogManager.Info(prefix + " " + msg);
                    else
                        logInterface.Info(prefix + " " + msg);
                }
            }

            public void Info(string formattedMsg, params object[] args)
            {
                Info(string.Format(formattedMsg, args));
            }

            public void Error(string msg)
            {
                if ((logFlags & LogFlags.Error) != 0 && (LogManager.GlobalLogFlags & LogFlags.Error) != 0)
                {
                    if (logInterface == null)
                        LogManager.Error(prefix + " " + msg);
                    else
                        logInterface.Error(prefix + " " + msg);
                }
            }

            public void Error(string formattedMsg, params object[] args)
            {
                Error(string.Format(formattedMsg, args));
            }
        }
        static Dictionary<string, LogProxy> registeredProxies = new Dictionary<string, LogProxy>();

        public static ILogProxy GetLogProxy(string prefix, IGRPExplorerLibLogInterface alternateInterface = null)
        {
            if (registeredProxies.ContainsKey(prefix))
                return registeredProxies[prefix];

            LogProxy newLogProxy = new LogProxy(prefix, alternateInterface);
            registeredProxies.Add(prefix, newLogProxy);
            return newLogProxy;
        }

        public static IGRPExplorerLibLogInterface LogInterface { get; set; } = new ConsoleLogInterface();
        public static LogFlags GlobalLogFlags { get; set; } = LogFlags.Debug | LogFlags.Info | LogFlags.Error;

        public static void Debug(string msg)
        {
            if ((LogInterface.CombineFlags(GlobalLogFlags) & LogFlags.Debug) != 0)
                LogInterface.Debug(msg);
        }

        public static void Info(string msg)
        {
            if ((LogInterface.CombineFlags(GlobalLogFlags) & LogFlags.Info) != 0)
                LogInterface.Info(msg);
        }

        public static void Error(string msg)
        {
            if ((LogInterface.CombineFlags(GlobalLogFlags) & LogFlags.Error) != 0)
                LogInterface.Error(msg);
        }
    }

    public interface ILogProxy
    {
        LogFlags LogFlags { get; set; }
        void Debug(string msg);
        void Debug(string formattedMsg, params object[] args);
        void Info(string msg);
        void Info(string formattedMsg, params object[] args);
        void Error(string msg);
        void Error(string formattedMsg, params object[] args);
    }
}
