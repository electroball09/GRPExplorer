using Microsoft.Extensions.DependencyInjection;
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
using MudBlazor;
using MudBlazor.Services;
using System.Net.Http;
using GRiPE.Code.Renderer;
using GRiPE.Code.Util;

namespace GRiPE.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddMudServices();
            serviceCollection.AddScoped<WebGLShaderCache>();
            serviceCollection.AddScoped(sp => new HttpClient() );
            serviceCollection.AddScoped<SessionId>();

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                //WindowState = WindowState.Maximized;
                Left = 0;
                Top = 0;
                Width = SystemParameters.WorkArea.Width;
                Height = SystemParameters.WorkArea.Height;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
