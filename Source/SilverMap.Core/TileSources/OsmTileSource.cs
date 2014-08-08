//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    public class OsmTileSource : MapTileSourceBase
    {
        public OsmTileSource()
            : base(18) // maximum level for OSM is 18
        {
        }

        protected override Uri GetUri(int x, int y, int z)
        {
            return new Uri(string.Format(
                "http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png",
                "abc"[(x ^ y) % 3], z, x, y));
        }
    }
}
