//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Overlays
{
    public class PtvDawnLabelOverlay : IOverlayProvider
    {
        public PtvDawnLabelOverlay(string baseUrl)
        {
            this.BaseUrl = baseUrl;
        }

        public string BaseUrl { get; set;}

        public Uri GetUri(double minX, double maxX, double minY, double maxY, double width, double height)
        {
            return new Uri(
                string.Format("{0}?left={1}&top={2}&right={3}&bottom={4}&width={5}&height={6}",
                BaseUrl,
                (int)Math.Round(minX), (int)Math.Round(minY), (int)Math.Round(maxX), (int)Math.Round(maxY), (int)Math.Round(width), (int)Math.Round(height)));
        }
    }
}