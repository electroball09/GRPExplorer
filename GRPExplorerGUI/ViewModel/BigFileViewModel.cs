using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib;
using GRPExplorerLib.BigFile;

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
        
        public BigFileViewModel()
        {
            bgworker.DoWork += Bgworker_DoWork;
        }

        private void Bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            bigFile?.LoadFromDisk();
            BigFile = bigFile;
        }

        public void LoadFromDisk()
        {
            if (bgworker.IsBusy)
                return;

            bgworker.RunWorkerAsync();
        }
    }
}
