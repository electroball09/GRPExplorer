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
    /// <summary>
    /// Interaction logic for BigFileFileView.xaml
    /// </summary>
    public partial class BigFileFileView : UserControl
    {
        static DependencyProperty fileProperty =
            DependencyProperty.Register("BigFileFile", typeof(BigFileFile), typeof(BigFileFileView));

        public BigFileFile File
        {
            get { return (BigFileFile)GetValue(fileProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(fileProperty, value);
                }
            }
        }

        public BigFileFileView()
        {
            InitializeComponent();
        }
    }
}
