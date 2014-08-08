//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------


namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    /// <summary>
    /// a tile source in Web Mercator projection
    /// </summary>
    public abstract class WebTileSourceBase : MercatorTileSourceBase
    {
        public WebTileSourceBase(int maxZoom)
            : base(6378137.0, maxZoom)
        {
        }
    }
}
