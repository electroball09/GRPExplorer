using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using GRPExplorerGUI.Model;
using System.Globalization;
using GRPExplorerLib.Logging;

namespace GRPExplorerGUI.View
{
    public partial class LogView : UserControl
    {
        public static DependencyProperty LogListProperty =
            DependencyProperty.Register("LogList", typeof(LogModel), typeof(LogView));

        public LogModel Log
        {
            get { return (LogModel)GetValue(LogListProperty); }
            set { SetValue(LogListProperty, value); }
        }

        int lastCount = 0;
        Timer logUpdateTimer = new Timer(100);
        
        public LogView()
        {
            InitializeComponent();

            logUpdateTimer.Elapsed += LogUpdateTimer_Elapsed;
            logUpdateTimer.Start();
        }

        private void LogUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke
                    (() =>
                    {
                        if (Log.LogProxy.Messages.Count == lastCount)
                            return;

                        lastCount = Log.LogProxy.Messages.Count;

                        listBox.SelectedIndex = listBox.Items.Count - 1;
                        listBox.ScrollIntoView(listBox.SelectedItem);
                        listBox.SelectedIndex = -1;

                        chkERROR.IsChecked = (Log.LogFlags & LogFlags.Error) != 0;
                        chkINFO.IsChecked  = (Log.LogFlags & LogFlags.Info) != 0;
                        chkDEBUG.IsChecked = (Log.LogFlags & LogFlags.Debug) != 0;
                    });
            }
            catch { } //sometimes it crashes on exit ¯\_(ツ)_/¯
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (Log == null)
                return;

            if (sender == chkERROR)
                Log.LogFlags = Log.LogFlags | LogFlags.Error;
            if (sender == chkINFO)            
                Log.LogFlags = Log.LogFlags | LogFlags.Info;
            if (sender == chkDEBUG)           
                Log.LogFlags = Log.LogFlags | LogFlags.Debug;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Log == null)
                return;

            if (sender == chkERROR)
                Log.LogFlags = Log.LogFlags ^ LogFlags.Error;
            if (sender == chkINFO)
                Log.LogFlags = Log.LogFlags ^ LogFlags.Info;
            if (sender == chkDEBUG)
                Log.LogFlags = Log.LogFlags ^ LogFlags.Debug;
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            Log.LogProxy.Messages.Clear();
        }
    }
}
