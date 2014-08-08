//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    public class PtvDawnNativeTileSource : MapTileSourceBase
    {
        protected string baseUrl;


        public PtvDawnNativeTileSource(string baseUrl, int maxZoom)
            : base(maxZoom)
        {
            this.baseUrl = baseUrl;
        }

        protected override Uri GetUri(int x, int y, int z)
        {
            return new Uri(string.Format("{0}?x={1}&y={2}&z={3}", baseUrl, x, y, z));
        }
    }
}
