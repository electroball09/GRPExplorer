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
        static Action<PropertyChangedEventHandler, BigFileViewModel, string> propChangedAction = 
            (ev, vm, prop) => 
            {
                //MessageBox.Show("wat lol");
                ev?.Invoke(vm, new PropertyChangedEventArgs(prop));
            };
        protected void NotifyPropertyChanged([CallerMemberName] string prop = "")
        {
            Application.Current.Dispatcher.Invoke(propChangedAction, PropertyChanged, this, prop);
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

        private BigFileOperationStatus currentOperation;
        public BigFileOperationStatus CurrentOperationStatus
        {
            get { return currentOperation; }
            private set { currentOperation = value;  NotifyPropertyChanged(); }
        }
        
        public float LoadProgress
        {
            get
            {
                if (bigFile == null || currentOperation == null)
                    return 0f;
                return currentOperation.Progress;
            }
        }

        public bool IsOperationHappening
        {
            get
            {
                return currentOperation != null;
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
        private BackgroundWorker extraDataWorker = new BackgroundWorker();
        private ILogProxy log;
        
        public BigFileViewModel()
        {
            bgworker.DoWork += Bgworker_DoWork;
            extraDataWorker.DoWork += ExtraDataWorker_DoWork;
            log = LogManager.GetLogProxy("BigFileViewModel");
        }

        private void ExtraDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bigFile?.LoadExtraData(e.Argument as BigFileOperationStatus);
        }

        private void Bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            log.Info("BGWorker loading bigfile");
            bigFile.LoadStatus.OnProgressUpdated +=
                (BigFileOperationStatus status) =>
                    {
                        NotifyPropertyChanged("LoadProgress");
                    };
            CurrentOperationStatus = bigFile.LoadStatus;
            bigFile.LoadFromDisk();
            BigFile = bigFile;
        }

        public void LoadFromDisk(Action onFinished = null)
        {
            if (bgworker.IsBusy || BigFileLoaded == true)
                return;

            bgworker.RunWorkerCompleted += 
                (sender, e) =>
                {
                    Application.Current.Dispatcher.Invoke(onFinished);
                };

            bgworker.RunWorkerAsync();
        }

        public void LoadExtraData(Action onFinished = null)
        {
            if (extraDataWorker.IsBusy)
                return;

            BigFileExtraDataLoadOperationStatus status = new BigFileExtraDataLoadOperationStatus();
            CurrentOperationStatus = status;

            extraDataWorker.RunWorkerAsync(status);

            extraDataWorker.RunWorkerCompleted +=
                (sender, e) =>
                {
                    Application.Current.Dispatcher.Invoke(onFinished);
                };
        }
    }
}
