using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using GRPExplorerGUI.ViewModel;
using Microsoft.Win32;

namespace GRPExplorerGUI.View
{
    /// <summary>
    /// Interaction logic for BigFileView.xaml
    /// </summary>
    public partial class BigFileView : UserControl
    {
        public static DependencyProperty BigFileViewModelProperty =
            DependencyProperty.Register("BigFileViewModel", typeof(BigFileViewModel), typeof(BigFileView));
        public static DependencyProperty BigFileProperty =
            DependencyProperty.Register("BigFile", typeof(BigFile), typeof(BigFileView));

        public BigFileViewModel BigFileViewModel
        {
            get { return (BigFileViewModel)GetValue(BigFileViewModelProperty); }
            set { SetValue(BigFileViewModelProperty, value); }
        }

        public BigFileView()
        {
            InitializeComponent();
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.DefaultExt = ".big";
            dialog.Filter = "BIG files (*.big)|*.big";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                BigFile bf = new PackedBigFile(new System.IO.FileInfo(dialog.FileName));
                BigFileViewModel = new BigFileViewModel();
                BigFileViewModel.BigFile = bf;

                BigFileViewModel.LoadFromDisk
                    (() =>
                    {
                        bf.FileUtil.SortFolderTree(bf.RootFolder);
                        BigFileFolderTreeComponent.RootFolder = BigFileViewModel.BigFile.RootFolder;
                    });
            }
        }
    }
}
