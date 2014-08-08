//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    /// <summary>
    /// This layer displays any xaml element with mercator coordinates
    /// </summary>
    public class GeoXamlLayer : Canvas, IDisposable
    {
        private Map map;

        public GeoXamlLayer(Map map, string xamlUrl)
        {
            this.map = map;

            this.RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercator);

            map.GeoCanvas.Children.Add(this);
            Canvas.SetZIndex(this, 40);

            WebRequest request = WebRequest.Create(xamlUrl);
            request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        private void ResponseCallback(IAsyncResult asyncResult)
        {
            var request = (HttpWebRequest)asyncResult.AsyncState;
            var response = (HttpWebResponse)request.EndGetResponse(asyncResult);

            using(var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                string xamlString = reader.ReadToEnd();

                Dispatcher.BeginInvoke(new Action<string>(SetXaml), xamlString);
            }
        }

        public void SetXaml(string xamlString)
        {
            var uiElement = System.Windows.Markup.XamlReader.Load(xamlString) as UIElement;            

            this.Children.Add(uiElement);
        }
         
        #region IDisposable Members

        public void Dispose()
        {
            this.Children.Clear();

            // ToDO: is there any way to propely dispose Wpf-elements?
            // animated elements still use 100% processor time after removing.
            // Calling GC.Collect() is not a proper solution            
            GC.Collect();
        }

        #endregion
     }
}
