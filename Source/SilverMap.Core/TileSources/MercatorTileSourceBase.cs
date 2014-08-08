//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    // a tile source in spheric Mercator projection
    public abstract class MercatorTileSourceBase : MapTileSourceBase, IScaledTileSource
    {
        public double Factor { get; set; }

        protected double earthHalfCircum;

        public MercatorTileSourceBase(double earthRadius, int maxZoom)
            : base(maxZoom)
        {
            this.earthHalfCircum = earthRadius * Math.PI;

            Factor = 1.0;
        }

        protected override Uri GetUri(int x, int y, int z)
        {
            double minX, minY, maxX, maxY;
            TileToMercatorAtZoom(x, y, z, out minX, out minY, out maxX, out maxY);

            return GetUri(minX, minY, maxX, maxY);
        }

        protected abstract Uri GetUri(double minX, double minY, double maxX, double maxY);

        protected void TileToMercatorAtZoom(
            int tileX, int tileY, int zoom,
            out double xMin, out double yMin, out double xMax, out double yMax)
        {
            double arc = this.earthHalfCircum / Math.Pow(2, zoom - 1);

            xMin = (tileX * arc) - earthHalfCircum;
            yMax = earthHalfCircum - (tileY * arc);

            xMax = xMin + arc;
            yMin = yMax - arc;

            xMin = xMin / Factor;
            yMax = yMax / Factor;
            xMax = xMax / Factor;
            yMin = yMin / Factor;
        }
    }

    /// <summary>
    /// Indicated that the tile sources uses a tiling system which differs by a scale factor from
    /// the standard tiling scheme. This interface is used for xServer tile-sources to avoid rounding
    /// errors at deep zoom levels.
    /// </summary>
    public interface IScaledTileSource
    {
        /// <summary>
        /// The factor relative to the standard tiling scheme
        /// </summary>
        double Factor { get; set; }
    }
}
