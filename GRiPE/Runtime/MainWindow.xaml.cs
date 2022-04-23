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
using GRiPE.Code.Config;

namespace GRiPE.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowState = GRiPEConfig.Config.Maximized ? WindowState.Maximized : WindowState.Normal;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            Left = GRiPEConfig.Config.WindowLeft;
            Top = GRiPEConfig.Config.WindowTop;

            //blazor initialization
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddMudServices();
            serviceCollection.AddScoped<WebGLShaderCache>();
            serviceCollection.AddScoped(sp => new HttpClient());
            serviceCollection.AddScoped<SessionId>();
            serviceCollection.AddScoped((_) => GRiPEConfig.Config);

            //setting parameters on App.razor
            Dictionary<string, object> parameters = new();
            parameters.Add("QuitFunc", () => Environment.Exit(0));
            parameters.Add("MaximizeFunc", () =>
            {
                bool shouldBeMaximized = WindowState != WindowState.Maximized;
                WindowState = shouldBeMaximized ? WindowState.Maximized : WindowState.Normal;
                GRiPEConfig.Config.Maximized = shouldBeMaximized;
                UpdateWebviewMargin();
                return shouldBeMaximized;
            });
            parameters.Add("MinimizeFunc", () => { WindowState = WindowState.Minimized; });

            //add resources, these won't show up in the xaml but they work
            Resources.Add("parameters", parameters);
            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }

        private void UpdateWebviewMargin()
        {
            blazorThing.Margin = new Thickness(0, WindowState == WindowState.Maximized ? 0 : 25, 0, 0);
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void maximize_Click(object sender, RoutedEventArgs e)
        {
        }

        private void minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var mousePos = PointToScreen(Mouse.GetPosition(this));
            //WindowState = WindowState.Normal;
            //Left = mousePos.X;
            //Top = mousePos.Y;
            DragMove();
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GRiPEConfig.Config.WindowLeft = Left;
            GRiPEConfig.Config.WindowTop = Top;
        }

        bool changedFromLoad = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWebviewMargin();
            changedFromLoad = true;
            Width = GRiPEConfig.Config.WindowWidth;
            Height = GRiPEConfig.Config.WindowHeight;
            changedFromLoad = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this gets called from framework as window loads as well as Window_Loaded above setting the size
            //so make sure we're not saving the default values of the window
            if (!changedFromLoad && IsLoaded)
            {
                //MessageBox.Show(changedFromLoad.ToString());
                GRiPEConfig.Config.WindowWidth = e.NewSize.Width;
                GRiPEConfig.Config.WindowHeight = e.NewSize.Height;
            }
        }
    }
}
