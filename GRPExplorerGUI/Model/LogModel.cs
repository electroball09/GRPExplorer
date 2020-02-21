using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using GRPExplorerLib;
using GRPExplorerLib.Logging;
using System.ComponentModel;

namespace GRPExplorerGUI.Model
{
    public class LogModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        static Action<PropertyChangedEventHandler, LogModel, string> propChangedAction =
            (ev, vm, prop) =>
            {
                ev?.Invoke(vm, new PropertyChangedEventArgs(prop));
            };
        protected void NotifyPropertyChanged([CallerMemberName] string prop = "")
        {
            Application.Current.Dispatcher.Invoke(propChangedAction, PropertyChanged, this, prop);
        }

        public GRPExplorerGUILogProxy LogProxy { get; } = new GRPExplorerGUILogProxy();

        private LogFlags flags = LogFlags.Error | LogFlags.Info;
        public LogFlags LogFlags
        {
            get { return flags; }
            set { flags = value; LogProxy.SetFlags(value); NotifyPropertyChanged(); }
        }

        public LogModel()
        {
            LogProxy.SetFlags(flags);
        }
    }

    public class LogMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        static Action<PropertyChangedEventHandler, LogMessage, string> propChangedAction =
            (ev, vm, prop) =>
            {
                ev?.Invoke(vm, new PropertyChangedEventArgs(prop));
            };
        protected void NotifyPropertyChanged([CallerMemberName] string prop = "")
        {
            Application.Current?.Dispatcher.Invoke(propChangedAction, PropertyChanged, this, prop);
        }

        public string Message { get; }
        public LogType LogType { get; }

        bool visible = true;
        public bool IsVisible { get { return visible; } private set { visible = value; NotifyPropertyChanged(); } }

        public LogMessage(LogType _logType, string _msg)
        {
            Message = _msg;
            LogType = _logType;
        }

        public void SetVisibility(LogFlags flags)
        {
            switch (LogType)
            {
                case LogType.Debug:
                    IsVisible = (flags & LogFlags.Debug) != 0;
                    break;
                case LogType.Info:
                    IsVisible = (flags & LogFlags.Info) != 0;
                    break;
                case LogType.Error:
                    IsVisible = (flags & LogFlags.Error) != 0;
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return Message;
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
        public ObservableCollection<LogMessage> Messages { get; } = new ObservableCollection<LogMessage>();

        private LogFlags flags = LogFlags.All;

        public GRPExplorerGUILogProxy()
        {

        }

        private void AddMsg(LogMessage msg)
        {
            try
            {
                lock (Messages)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action<LogMessage>)Messages.Add, msg);
                }
            }
            catch { } //sometimes there's an exception when closing the program so i'm cancelling it
        }

        public void SetFlags(LogFlags flags)
        {
            this.flags = flags;

            lock(Messages)
            {
                try
                {
                    foreach (LogMessage msg in Messages)
                        msg.SetVisibility(flags);
                }
                catch (Exception ex) { /* lol who cares */ }
            }
        }

        public LogFlags CombineFlags(LogFlags original)
        {
            return LogFlags.All;
        }

        public void Debug(string msg)
        {
            LogMessage message = new LogMessage(LogType.Debug, msg);
            message.SetVisibility(flags);
            AddMsg(message);
        }

        public void Info(string msg)
        {
            LogMessage message = new LogMessage(LogType.Info, msg);
            message.SetVisibility(flags);
            AddMsg(message);
        }

        public void Error(string msg)
        {
            LogMessage message = new LogMessage(LogType.Error, msg);
            message.SetVisibility(flags);
            AddMsg(message);
        }
    }
}
