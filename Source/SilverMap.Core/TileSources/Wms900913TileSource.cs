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
    /// A Tile Source for WMS which support 900913
    /// </summary>
    public class Wms900913TileSource : WebTileSourceBase, IOverlayProvider
    {
        private string baseUrl;

        public Wms900913TileSource(string baseUrl, int maxZoom)
            : base(maxZoom)
        {
            this.baseUrl = baseUrl;
        }

        protected override Uri GetUri(double minX, double minY, double maxX, double maxY)
        {
            return new Uri(string.Format(
                "{0}&SRS=EPSG%3A900913&&BBOX={1},{2},{3},{4}&WIDTH=256&HEIGHT=256",
                baseUrl,
              System.Convert.ToString(minX, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(minY, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(maxX, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(maxY, NumberFormatInfo.InvariantInfo)));
        }

        #region IOverlayProvider Members

        public Uri GetUri(double minX, double maxX, double minY, double maxY, double width, double height)
        {
            return new Uri(string.Format(
                "{0}&SRS=EPSG%3A900913&&BBOX={1},{2},{3},{4}&WIDTH={5}&HEIGHT={6}",
                baseUrl,
              System.Convert.ToString(minX, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(minY, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(maxX, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(maxY, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(width, NumberFormatInfo.InvariantInfo),
              System.Convert.ToString(height, NumberFormatInfo.InvariantInfo)));
        }

        #endregion
    }
}
