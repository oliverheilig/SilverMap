//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    // a tile source which queries the ajax servlet with smart unit coordinates
    // This tile source defines a bottom-up tiling-scheme where the bounding box has always a width of power-of-two
    // This solves the tearing problem with at deep zoom levels. 
    public class PtvSmartUnitTileSource : MapTileSourceBase, IScaledTileSource
    {
        protected string baseUrl;

        public PtvSmartUnitTileSource(string baseUrl, int maxZoom)
            : base(maxZoom)
        {
            this.baseUrl = baseUrl;

            // factor for scaling of the MSI
            Factor = 127.0 / 128.0;
        }

        protected virtual Uri GetUri(int minX, int minY, int maxX, int maxY)
        {
            string str = string.Format("{0}/MapServlet?left={1}&top={2}&right={3}&bottom={4}&width=256&height=256&hiddenLayers=Town&coordformat=PTV_SMARTUNITS",
                baseUrl,
                minX, minY, maxX, maxY);

            return new Uri(str);
        }

        public double Factor { get; set; }

        protected override Uri GetUri(int x, int y, int z)
        {
            int minX, minY, maxX, maxY;
            TileToPtvSmartUnitAtZoom(x, y, z, out minX, out minY, out maxX, out maxY);

            return GetUri(minX, minY, maxX, maxY);
        }

        protected void TileToPtvSmartUnitAtZoom(
            int tileX, int tileY, int zoom,
            out int ixMin, out int iyMin, out  int ixMax, out int iyMax)
        {
            double earthRadius = 6371000.0;
            double earthHalfCircum = earthRadius * Math.PI;

            double arc = earthHalfCircum / Math.Pow(2, zoom - 1);

            double xMin = (tileX * arc) - earthHalfCircum;
            double yMax = earthHalfCircum - (tileY * arc);

            double xMax = xMin + arc;
            double yMin = yMax - arc;

            xMin = xMin / Factor;
            yMax = yMax / Factor;
            xMax = xMax / Factor;
            yMin = yMin / Factor;

            double cSMART_UNIT = 4.809543;

            ixMin = (int)Math.Round((xMin + earthHalfCircum) / cSMART_UNIT);
            iyMin = (int)Math.Round((yMin + earthHalfCircum) / cSMART_UNIT);
            ixMax = (int)Math.Round((xMax + earthHalfCircum) / cSMART_UNIT);
            iyMax = (int)Math.Round((yMax + earthHalfCircum) / cSMART_UNIT);
        }
    }
}
