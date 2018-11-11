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
using Microsoft.Win32;
using GRPExplorerLib.BigFile.Extra;
using System.IO;

namespace GRPExplorerGUI.Extra
{
    /// <summary>
    /// Interaction logic for FEUtoSWFWindow.xaml
    /// </summary>
    public partial class FEUtoSWFWindow : Window
    {
        public FEUtoSWFWindow()
        {
            InitializeComponent();
        }

        private void txtFileName_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                txtFileName.Text = files[0];
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            FEUtoSWF.Convert(new FileInfo(txtFileName.Text));
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".feu";
            dialog.Multiselect = false;
            bool? res = dialog.ShowDialog(this);
            if (res == true)
                txtFileName.Text = dialog.FileName;
        }
    }
}
