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
            Application.Current.Dispatcher.Invoke
                (() =>
                {
                    if (Log.LogProxy.Messages.Count == lastCount)
                        return;

                    lastCount = Log.LogProxy.Messages.Count;

                    listBox.SelectedIndex = listBox.Items.Count - 1;
                    listBox.ScrollIntoView(listBox.SelectedItem);
                    listBox.SelectedIndex = -1;
                });
        }
    }
}
