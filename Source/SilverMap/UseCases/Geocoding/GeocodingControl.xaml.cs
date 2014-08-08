//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using Ptvag.Dawn.Controls.SilverMap.Core;
using SilverMap.XlocateService;

namespace SilverMap.UseCases.Geocoding
{
    public partial class GeocodingControl : UserControl
    {
        StackPanel workspace;
        Map map;
        GeoCodeResultsLayer resultsLayer;

        public GeocodingControl(StackPanel workspace, Map map)
        {
            InitializeComponent();

            this.workspace = workspace;
            this.map = map;

            workspace.Children.Add(this);
            this.Focus();
        }

        public void Remove()
        {
            workspace.Children.Remove(this);

            if (resultsLayer != null)
                resultsLayer.Remove();
        }

        SilverMap.XlocateService.Address address;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // use Xlocate address for data binding
            // note: this is not a best practice for a real-world application
            address = new XlocateService.Address
            {
                country = "D",
                postCode = "76131",
                city = "Karlsruhe",
                street = "Stumpfstr.",
                houseNumber = "1", 
                city2 = "",
                state = ""
            };

            // Do not load your data at design time.
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                //Load your data here and assign the result to the CollectionViewSource.
                System.Windows.Data.CollectionViewSource addressViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["addressViewSource"];
                addressViewSource.Source = new SilverMap.XlocateService.Address[] { address };
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Locate();
        }

        public void Locate()
        {
            if (resultsLayer != null)
                resultsLayer.Remove();

            // to call an xServer directly from SL, you have to put a cross-domain policy-file on your xServer machine
            // see http://msdn.microsoft.com/en-us/library/cc197955(VS.95).aspx 
//            XLocateWS xLocate = new XLocateWSClient();
            XLocateWS xLocate = new XLocateWSClient(new BasicHttpBinding { MaxReceivedMessageSize = 2147483647 },
                new EndpointAddress(App.BaseUrl + "/XServerProxy.ashx?type=xlocate"));
            xLocate.BeginfindAddress(new findAddressRequest
            {
                Address_1 = address
            }, new AsyncCallback(Invoke), xLocate);
        }

        public void Invoke(IAsyncResult result)
        {
            try
            {
                // not the UI thread!
                findAddressResponse response = (result.AsyncState as XLocateWS).EndfindAddress(result);

                if(response.result.errorCode < 0)
                    Dispatcher.BeginInvoke(new Action<string>(DisplayError), response.result.errorDescription);
                else
                    Dispatcher.BeginInvoke(new Action<AddressResponse>(InitializeUI), response.result);
            }
            catch (Exception ex)
            {
                // Note: you must register the Server for ClientHttp-Protokoll to get the real exception, see
                // http://blogs.microsoft.co.il/blogs/idof/archive/2009/12/08/handling-soap-faults-in-silverlight.aspx
                Dispatcher.BeginInvoke(new Action<string>(DisplayError), ex.Message);
            }
        }

        public void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage);
        }

        private void InitializeUI(AddressResponse response)
        {
            // back in the UI thread
            if (resultsLayer != null)
                resultsLayer.Remove();

            resultsLayer = new GeoCodeResultsLayer(this.map, response);
        }
    }
}
