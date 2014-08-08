//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;

#if !PHONE7
using System.Windows.Browser;
#endif

namespace Ptvag.Dawn.Controls.SilverMap.Core.Overlays
{
    public class PtvAjaxLabelOverlay : IOverlayProvider
    {
        public PtvAjaxLabelOverlay(string baseUrl, string token)
        {
            this.BaseUrl = baseUrl;
            this.Token = token;
        }

        public string BaseUrl { get; set; }

        public string Token { get; set; }

        public Uri GetUri(double minX, double maxX, double minY, double maxY, double width, double height)
        {
#if !PHONE7
            string host = Application.Current.Host.Source.Host;
            string referrer;
            if (Application.Current.IsRunningOutOfBrowser)
                referrer = host;
            else
            {
                referrer = HtmlPage.Document.GetProperty("referrer") as string;
                if (string.IsNullOrEmpty(referrer))
                    referrer = host;
            }
            string token = this.Token + "$" + referrer + "$" + host;
#else
            string token = "t$o$k";
#endif

            string str = string.Format("{0}/MapServlet?left={1}&top={2}&right={3}&bottom={4}&width={5}&height={6}&tok={7}&visibleLayers=Town&transparent=true",
                BaseUrl,
                (int)Math.Round(minX), (int)Math.Round(minY), (int)Math.Round(maxX), (int)Math.Round(maxY), (int)Math.Round(width), (int)Math.Round(height),
                token);

            return new Uri(str);
        }
    }
}
