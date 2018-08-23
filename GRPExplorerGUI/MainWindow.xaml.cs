using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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
using GRPExplorerGUI.Model;
using GRPExplorerGUI.ViewModel;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Extra;

namespace GRPExplorerGUI
{
    public partial class MainWindow : Window
    {
        private BigFile bigFile;
        private string lastText = "";

        IGRPExplorerLibLogInterface logInterface;

        public MainWindow()
        {
            InitializeComponent();

            btnOpenBigFile.Click += BtnOpenBigFile_Click;
            btnFindKey.Click += BtnFindKey_Click;
            
            LogModel logModel = new LogModel();
            logInterface = logModel.LogProxy;
            logView.Log = logModel;
            LogManager.LogInterface = logInterface;
            LogManager.GlobalLogFlags = LogFlags.Info | LogFlags.Error;
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
            BigFileViewModel vm = new BigFileViewModel();
            bigFileview.BigFileViewModel = vm;
            vm.BigFile = bigFile;
        }

        private void btnOpenBigFile_Copy_Click(object sender, RoutedEventArgs e)
        {
            bigFile = new PackedBigFile(new System.IO.FileInfo(txtFileInput.Text));
            BigFileViewModel vm = new BigFileViewModel();
            bigFileview.BigFileViewModel = vm;
            vm.BigFile = bigFile;
        }

        private void SetFile(int key)
        {
            if (bigFile == null)
            {
                Log("no.");
                return;
            }

            BigFileFile file = bigFile.FileMap[key];
            if (file != null)
            {
                groupFile.Header = file.Name;
                lblKey.Content = string.Format("{0:X8}", key);
                lblPath.Content = file.FullFolderPath;
                if (bigFile is UnpackedBigFile)
                {
                    lblRenamed.Content = (bigFile as UnpackedBigFile).RenamedMapping[key].FileName;
                }
            }
            else
            {
                Log("oh now ya done it");
            }
        }

        private void Log(string msg)
        {
            logInterface.Info("[GUI] " + msg);
        }
        
        BigFileUnpacker unpacker;
        private void btnUnpackBigfile_Click(object sender, RoutedEventArgs e)
        {
            BigFileUnpackOptions options = new BigFileUnpackOptions
            {
                Directory = new System.IO.DirectoryInfo(txtUnpackDir.Text),
                Flags = BigFileFlags.Decompress | BigFileFlags.UseThreading,
                Threads = 4,
                LoadExtensionsFile = "FileExtensionsList.gex",
            };

            unpacker = new BigFileUnpacker(bigFile);

            BigFileUnpackOperationStatus status = unpacker.UnpackBigfile(options);
        }

        BigFilePacker packer;
        private void btnPackBigFile_Click(object sender, RoutedEventArgs e)
        {
            BigFilePackOptions options = new BigFilePackOptions
            {
                Directory = new System.IO.DirectoryInfo(txtUnpackDir.Text),
                Flags = (bool)chkCompress.IsChecked ? BigFileFlags.UseThreading | BigFileFlags.Compress : BigFileFlags.UseThreading,
                Threads = (int)sldThreads.Value,
                BigFileName = "Yeti",
                DeleteChunks = true
            };

            logInterface.Info("[GUI] Threads: " + options.Threads);

            packer = new BigFilePacker(bigFile);

            BigFilePackOperationStatus status = packer.PackBigFile(options);
        }

        BackgroundWorker bgworker;
        BigFileFileExtensionsList gen;
        private void btnGenFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            gen = new BigFileFileExtensionsList(bigFile);
            
            if (bgworker == null)
            {
                bgworker = new BackgroundWorker();
                bgworker.DoWork += Bgworker_DoWork;
            }

            bgworker.RunWorkerAsync();
        }

        private void Bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dictionary<short, string> extensionsList = gen.LoadFileExtensionsListFromLoadReport(new System.IO.FileInfo("LoadReport.txt"));
            //Dictionary<short, string> extensionsList = gen.LoadFileExtensionsList(new System.IO.FileInfo("FileExtensionsList.gex"));
            gen.WriteFileExtensionsListToFile(extensionsList);
            gen.VerifyFileTypes(extensionsList);
        }
    }
}
