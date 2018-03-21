using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using GRPExplorerLib;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Util;
using System.ComponentModel;

namespace GRPExplorerGUI
{
    public partial class MainWindow : Window
    {
        private class LogProxy : IGRPExplorerLibLogInterface
        {
            private ObservableCollection<string> _logList;

            public LogProxy(ObservableCollection<string> logList)
            {
                _logList = logList;
            }

            public void Debug(string msg)
            {
                lol(msg);
            }

            public void Error(string msg)
            {
                lol(msg);
            }

            public void Info(string msg)
            {
                lol(msg);
            }

            private void lol(string msg)
            {
                Action<string> method = _logList.Add;
                Application.Current.Dispatcher.BeginInvoke(method, msg);
            }
        }

        private UnpackedBigFile bigFile;
        private string lastText = "";
        ObservableCollection<string> logList = new ObservableCollection<string>();
        private LogProxy log;
        BackgroundWorker readWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            btnOpenBigFile.Click += BtnOpenBigFile_Click;
            btnFindKey.Click += BtnFindKey_Click;
            listLog.ItemsSource = logList;
            log = new LogProxy(logList);
            GRPExplorerLibManager.SetLogInterface(log);
            GRPExplorerLibManager.SetLogFlags(LogFlags.Info | LogFlags.Error);
            readWorker.DoWork += ReadWorker_DoWork;
        }

        private void ReadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bigFile.LoadFromDisk();
            bigFile.FileUtil.DebugLogRootFolderTree(bigFile.RootFolder);
        }

        private void BtnFindKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int val = Convert.ToInt32(txtFindKey.Text, 16);
                lastText = txtFindKey.Text;

                SetFile(val);
            }
            catch (Exception ex)
            {
                txtFindKey.Text = lastText;
                Log(ex.Message);
            }
        }

        private void BtnOpenBigFile_Click(object sender, RoutedEventArgs e)
        {
            bigFile = new UnpackedBigFile(new System.IO.DirectoryInfo(txtFileInput.Text));
            readWorker.RunWorkerAsync();
        }

        private void SetFile(int key)
        {
            if (bigFile == null)
            {
                Log("no.");
                return;
            }

            BigFileFile file = bigFile.MappingData[key];
            if (file != null)
            {
                groupFile.Header = file.Name;
                lblKey.Content = string.Format("{0:X8}", key);
                lblPath.Content = file.FullFolderPath;
                lblRenamed.Content = bigFile.RenamedMapping[key].FileName;
            }
            else
            {
                Log("oh now ya done it");
            }
        }

        private void Log(string msg)
        {
            log.Info("[GUI] " + msg);
        }
    }
}
