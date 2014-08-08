//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------


namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    // a tile source in PTV Mercator projection
    // This tile source defines a bottom-up tiling-scheme where level 19 has a tile size of 80 PTV_Mercator units
    // This solves the tearing problem at deep zoom levels. 
    public abstract class PtvTileSourceBase : MercatorTileSourceBase
    {
        public PtvTileSourceBase()
            : base(6371000.0, 19)
        {
            double scalingEarthHalfCircum = 80 * (1 << 18); // new virtual earth circum

            // factor for scaling of the MSI
            Factor = earthHalfCircum / scalingEarthHalfCircum;
        }
    }
}
