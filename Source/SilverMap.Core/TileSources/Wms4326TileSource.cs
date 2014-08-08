//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    /// <summary>
    /// Note: Using an Overlay in "WGS Projection" on a mercator map is discouraged.
    /// It fits for deep zoom levels, because mercator and equirectangular projection both
    /// maintain shapes for small areas. Only use it if the WMS doesn't support 900913.
    /// </summary>
    public class Wms4326Source : MapTileSourceBase
    {
        private string baseUrl;

        public Wms4326Source(string baseUrl, int maxZoom)
            : base(maxZoom)
        {
          this.baseUrl = baseUrl;
        }

        public double OffsetLat { get; set; }
        public double OffsetLon { get; set; }

        protected override Uri GetUri(int x, int y, int z)
        {
            Rect rect = GeoTransform.TileToWgsAtZoom(x, y, z);

            return new Uri(string.Format(
                "{0}&SRS=EPSG%3A4326&&BBOX={1},{2},{3},{4}&WIDTH=256&HEIGHT=256",
                baseUrl,
              System.Convert.ToString(rect.Left + OffsetLat, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(rect.Top + OffsetLat, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(rect.Right + OffsetLat, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(rect.Bottom + OffsetLat, NumberFormatInfo.InvariantInfo)));
        }
    }
}
