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
using GRPExplorerGUI.ViewModel;
using Microsoft.Win32;

namespace GRPExplorerGUI.View
{
    /// <summary>
    /// Interaction logic for BigFileFileView.xaml
    /// </summary>
    public partial class BigFileFileView : UserControl
    {
        public static readonly DependencyProperty vmProperty =
            DependencyProperty.Register("ViewModel", typeof(BigFileViewModel), typeof(BigFileFileView), new FrameworkPropertyMetadata());

        public BigFileViewModel ViewModel
        {
            get { return (BigFileViewModel)GetValue(vmProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(vmProperty, value);
                }
            }
        }

        public BigFileFileView()
        {
            InitializeComponent();
        }

        private void BtnExtractFile_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ExtractSelectedFile();

            //void log(string msg) => GRPExplorerLib.Logging.LogManager.Info(msg);
        }
    }
}
