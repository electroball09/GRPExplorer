using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using GRPExplorerLib;
using GRPExplorerLib.Logging;

namespace GRPExplorerGUI.Model
{
    public class LogModel
    {
        private GRPExplorerGUILogProxy logProxy = new GRPExplorerGUILogProxy();
        public GRPExplorerGUILogProxy LogProxy
        {
            get { return logProxy; }
        }

        public LogModel()
        {

        }
    }

    public class LogMessage
    {
        string msg;
        public string Message { get { return msg; } }
        LogType logType;
        public LogType LogType { get { return logType; } }

        public LogMessage(LogType _logType, string _msg)
        {
            msg = _msg;
            logType = _logType;
        }

        public override string ToString()
        {
            return msg;
        }
    }

    public enum LogType
    {
        Debug = 0,
        Info = 1,
        Error = 2
    }

    public class GRPExplorerGUILogProxy : IGRPExplorerLibLogInterface
    {
        ObservableCollection<LogMessage> messages = new ObservableCollection<LogMessage>();
        public ObservableCollection<LogMessage> Messages
        {
            get { return messages; }
        }

        public GRPExplorerGUILogProxy()
        {

        }

        private void AddMsg(LogMessage msg)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke((Action<LogMessage>)messages.Add, msg);
            }
            catch { } //sometimes there's an exception when closing the program so i'm cancelling it
        }

        public void Debug(string msg)
        {
            AddMsg(new LogMessage(LogType.Debug, msg));
        }

        public void Error(string msg)
        {
            AddMsg(new LogMessage(LogType.Error, msg));
        }

        public void Info(string msg)
        {
            AddMsg(new LogMessage(LogType.Info, msg));
        }
    }
}
