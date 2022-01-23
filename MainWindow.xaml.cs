using Microsoft.AspNet.SignalR;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace WpfAndroidInteractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread _thread;
        private HttpServer _httpServer;

        public MainWindow()
        {
            InitializeComponent();
            _thread = new Thread(startListening);
            _thread.Start();
        }

        private void startListening()
        {
            try
            {
                WpfAndroidInteractor.SignalRServer.Start(null);
                ADBInteractor.StartADB();
                var routes = new string[]
                {
                    "index.html",
                    "jquery-1.9.1.min.js",
                    "jquery.signalR-2.2.3.min.js",
                    "json2.js"
                }.Select(r => new Route()
                {
                    Name = "Default",
                    Method = "GET",
                    UrlRegex = r,
                    Callable = (x) =>
                    {
                        return new HttpResponse()
                        {
                            StatusCode = "200",
                            ContentAsUTF8 = File.ReadAllText("Resources/Pages/" + r)
                        };
                    }
                }).ToList();
                _httpServer = new HttpServer(3001, routes);
                _httpServer.Listen();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Process.GetCurrentProcess().Kill();
        }

        private void btnInteract_Click(object sender, RoutedEventArgs e)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<SignalRServer.MessagePropagator>();
            var all = context.Clients.All.broadcast("Hello");
        }
    }
}
