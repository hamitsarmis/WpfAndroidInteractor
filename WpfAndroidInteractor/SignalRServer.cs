using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Windows;

[assembly: OwinStartup(typeof(WpfAndroidInteractor.SignalRServer.Startup))]
namespace WpfAndroidInteractor
{

    /// <summary>
    /// SignalR interaction logic
    /// </summary>
    public class SignalRServer
    {

        #region Private Members

        private static IDisposable _signalRServer;

        #endregion Private Members

        #region Public Methods

        /// <summary>
        /// Starts the SignalRServer
        /// </summary>
        /// <param name="args">Parameters</param>
        public static void Start(string[] args)
        {
            if (_signalRServer == null)
            {
                string url = "http://localhost:3002";
                StartOptions options = new StartOptions();
                options.Urls.Add(url);
                _signalRServer = WebApp.Start(options, builder =>
                {
                    builder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
                    builder.MapSignalR("/signalr", new HubConfiguration
                    {
                        // You can enable JSONP by uncommenting line below.
                        // JSONP requests are insecure but some older browsers (and some
                        // versions of IE) require JSONP to work cross domain
                        EnableJSONP = false,

                        // Turns on sending detailed information to clients when errors occur, disable for production
                        EnableDetailedErrors = true,
                        EnableJavaScriptProxies = true
                    });
                    builder.RunSignalR();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Stop()
        {
        }

        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
                app.MapSignalR();
                GlobalHost.DependencyResolver.Register(typeof(MessagePropagator), () => new MessagePropagator());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class MessagePropagator : Hub
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            public void Send(string message)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => 
                    MessageBox.Show($"Customer says {message}")));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="message"></param>
            public void SendBack(string message)
            {
            }

        }

        #endregion Public Properties

    }
}
