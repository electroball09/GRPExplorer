using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using System.Windows;

namespace GRPExplorerGUI.ViewModel
{
    public class BigFileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private BigFile bigFile;
        public BigFile BigFile
        {
            get { return bigFile; }
            set
            {
                bigFile = value;
                NotifyPropertyChanged();
                BigFileLoaded = false;
                BigFileType = "lol";
            }
        }
        
        public float LoadProgress
        {
            get
            {
                if (bigFile == null)
                    return 0f;
                return bigFile.LoadStatus.Progress * 100;
            }
        }

        public bool? BigFileLoaded
        {
            get { return bigFile?.IsLoaded; }
            private set { NotifyPropertyChanged(); }
        }

        public string BigFileType
        {
            get { return (bigFile == null) ? "N/A" : (bigFile is UnpackedBigFile) ? "Unpacked" : "Packed"; }
            private set { NotifyPropertyChanged(); }
        }

        private BackgroundWorker bgworker = new BackgroundWorker();
        private ILogProxy log;
        
        public BigFileViewModel()
        {
            bgworker.DoWork += Bgworker_DoWork;
            log = LogManager.GetLogProxy("BigFileViewModel");
        }

        private void Bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            log.Info("BGWorker loading bigfile");
            try
            {
                bigFile.LoadStatus.OnProgressUpdated +=
                    (BigFileOperationStatus status) =>
                        {
                            Action action = () =>
                            {
                                NotifyPropertyChanged("LoadProgress");
                            };
                            Application.Current.Dispatcher.BeginInvoke(action);
                        };
                bigFile.LoadFromDisk();
                BigFile = bigFile;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        public void LoadFromDisk()
        {
            if (bgworker.IsBusy)
                return;

            bgworker.RunWorkerAsync();
        }
    }
}
