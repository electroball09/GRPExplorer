using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GRPExplorerLib.BigFile;

namespace GRPExplorerGUI.View
{
    public partial class BigFileFolderTree : UserControl
    {
        static DependencyProperty RootFolderProperty =
            DependencyProperty.Register("RootFolder", typeof(BigFileFolder), typeof(BigFileFolderTree));
        static DependencyProperty SelectedFolderProperty =
            DependencyProperty.Register("SelectedFolder", typeof(BigFileFolder), typeof(BigFileFolderTree));
        static DependencyProperty SelectedFileProperty =
            DependencyProperty.Register("SelectedFile_", typeof(BigFileFile), typeof(BigFileFolderTree));

        public BigFileFolder RootFolder
        {
            get { return (BigFileFolder)GetValue(RootFolderProperty); }
            set { SetValue(RootFolderProperty, value); }
        }

        public BigFileFolder SelectedFolder
        {
            get { return (BigFileFolder)GetValue(SelectedFolderProperty); }
            set { SetValue(SelectedFolderProperty, value); }
        }

        public BigFileFile SelectedFile
        {
            get { return (BigFileFile)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); bigFileView.File = value; }
        }

        public BigFileFolderTree()
        {
            InitializeComponent();
        }

        private void listFiles_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as ListBox).SelectedIndex = -1;
        }
    }
}
