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

namespace GRPExplorerGUI.View
{
    /// <summary>
    /// Interaction logic for BigFileView.xaml
    /// </summary>
    public partial class BigFileView : UserControl
    {
        public static DependencyProperty BigFileViewModelProperty =
            DependencyProperty.Register("BigFileViewModel", typeof(BigFileViewModel), typeof(BigFileView));

        public BigFileViewModel BigFileViewModel
        {
            get { return (BigFileViewModel)GetValue(BigFileViewModelProperty); }
            set { SetValue(BigFileViewModelProperty, value); }
        }

        public BigFileView()
        {
            InitializeComponent();
        }

        private void LoadFromDiskBtnClick(object sender, RoutedEventArgs e)
        {
            BigFileViewModel.LoadFromDisk();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            BigFileFolderTreeComponent.RootFolder = BigFileViewModel.BigFile.RootFolder;
        }

        private void btnLoadXtra_Click(object sender, RoutedEventArgs e)
        {
            BigFileViewModel.LoadExtraData();
        }
    }
}
