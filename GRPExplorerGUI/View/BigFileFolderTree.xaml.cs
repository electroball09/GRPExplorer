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
        public static DependencyProperty CurrentFolderProperty =
            DependencyProperty.Register("CurrentFolder", typeof(BigFileFolder), typeof(BigFileFolderTree));

        public BigFileFolder CurrentFolder
        {
            get { return (BigFileFolder)GetValue(CurrentFolderProperty); }
            set { SetValue(CurrentFolderProperty, value); }
        }

        public BigFileFolderTree()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentFolder = CurrentFolder[(e.OriginalSource as Button).Content.ToString()];
        }

        private void btnUpFolder_Click(object sender, RoutedEventArgs e)
        {
            BigFileFolder parent = CurrentFolder?.ParentFolder;
            if (parent != null)
                CurrentFolder = parent;
        }
    }
}
