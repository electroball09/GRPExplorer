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
using System.Windows.Shapes;

namespace GRPExplorerGUI.Extra
{
    public enum clicked
    {
        none,
        unpack,
        pack
    }

    /// <summary>
    /// Interaction logic for BigfilePackingWindow.xaml
    /// </summary>
    public partial class BigfilePackingWindow : Window
    {
        public BigfilePackingWindow()
        {
            InitializeComponent();
        }

        public clicked Clicked = clicked.none;

        private void btnUnpackBigfile_Click(object sender, RoutedEventArgs e)
        {
            Clicked = clicked.unpack;
        }
        private void btnPackBigFile_Click(object sender, RoutedEventArgs e)
        {
            Clicked = clicked.pack;
        }
    }
}
