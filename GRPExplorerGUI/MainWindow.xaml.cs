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
using GRPExplorerGUI.Extra;
using Microsoft.Win32;

namespace GRPExplorerGUI
{
    public partial class MainWindow : Window
    {
        private string lastText = "";

        private FEUtoSWFWindow FEUtoSWFWindow;

        IGRPExplorerLibLogInterface logInterface;

        public MainWindow()
        {
            InitializeComponent();

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

        private void SetFile(int key)
        {
            if (bigFileview.BigFileViewModel == null)
            {
                Log("no.");
                return;
            }

            BigFileFile file = bigFileview.BigFileViewModel.BigFile.FileMap[key];
            if (file != null)
            {
                //groupFile.Header = file.Name;
                //lblKey.Content = string.Format("{0:X8}", key);
                //lblPath.Content = file.FullFolderPath;
                //if (bigFileview.BigFileViewModel.BigFile is UnpackedBigFile)
                //{
                //    lblRenamed.Content = (bigFileview.BigFileViewModel.BigFile as UnpackedBigFile).RenamedMapping[key].FileName;
                //}
                bigFileview.FolderTree.SelectedFile = file;
            }
            else
            {
                Log("File key not found!");
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
            };

            unpacker = new BigFileUnpacker(bigFileview.BigFileViewModel.BigFile);

            BigFileUnpackOperationStatus status = unpacker.UnpackBigfile(options);
        }

        BigFilePacker packer;
        private void btnPackBigFile_Click(object sender, RoutedEventArgs e)
        {
            BigFilePackOptions options = new BigFilePackOptions
            {
                Directory = new System.IO.DirectoryInfo(txtUnpackDir.Text),
                Flags = (bool)chkCompress.IsChecked ? BigFileFlags.UseThreading | BigFileFlags.Compress : BigFileFlags.UseThreading,
                Threads = 4,
                BigFileName = "Yeti",
                DeleteChunks = true
            };

            logInterface.Info("[GUI] Threads: " + options.Threads);

            packer = new BigFilePacker(bigFileview.BigFileViewModel.BigFile);

            BigFilePackOperationStatus status = packer.PackBigFile(options);
        }

        private void btnFEUtoSWFShow_Click(object sender, RoutedEventArgs e)
        {
            FEUtoSWFWindow = new FEUtoSWFWindow();
            FEUtoSWFWindow.ShowDialog();
        }

        private void MenuOpenBigfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.DefaultExt = ".big";
            dialog.Filter = "BIG files (*.big)|*.big";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                BigFile bf = new PackedBigFile(new System.IO.FileInfo(dialog.FileName));
                bigFileview.BigFileViewModel = new BigFileViewModel();
                bigFileview.BigFileViewModel.BigFile = bf;

                bigFileview.BigFileViewModel.LoadFromDisk
                    (() =>
                    {
                        bf.FileUtil.SortFolderTree(bf.RootFolder);
                        bigFileview.FolderTree.RootFolder = bigFileview.BigFileViewModel.BigFile.RootFolder;

                        bigFileview.BigFileViewModel.LoadExtraData
                            (() =>
                            {
                                GRPExplorerLib.Logging.LogManager.Info("REFERENCES LOADED");
                            });
                    });
            }
        }
    }
}
