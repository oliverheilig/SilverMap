//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Net;
using System.Net.Browser;
using System.Windows;

namespace SilverMap
{
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        /// <summary>
        /// Returns base url dynamically from host
        /// </summary>
        public static string BaseUrl
        {
            get
            {
                // get base url dynamically from host
                string baseUrl;
                if (Application.Current.Host.Source != null)
                {
                    baseUrl = Application.Current.Host.Source.AbsoluteUri;
                    baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/'));
                    baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/'));
                }
                else
                    baseUrl = "http://localhost/SilverMap.Web";

                return baseUrl;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // http://blogs.microsoft.co.il/blogs/idof/archive/2009/12/08/handling-soap-faults-in-silverlight.aspx
            HttpWebRequest.RegisterPrefix("http://80.146.239.180", WebRequestCreator.ClientHttp);

            if (Current.IsRunningOutOfBrowser)
            {
                // the event handler always returns true for UpdateAvailable on our IIS, so the restart-box alays appers
                // IIS should return a 304 for the xap file. Don't know how to fix this, so just ignore the box
#if !DeployOnDemoServer
                Current.CheckAndDownloadUpdateCompleted += (senderv, ev) =>
                {
                    if (ev.Error == null && ev.UpdateAvailable)
                        MessageBox.Show("New version! Please restart!");
                };
#endif
                Current.CheckAndDownloadUpdateAsync();

           } 
            
            this.RootVisual = new MainPage();
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
