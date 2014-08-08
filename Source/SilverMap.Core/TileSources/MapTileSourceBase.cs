//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    /// <summary>
    /// a tile source which queries tiles according to the standard map tiling scheme
    /// </summary>
    public abstract class MapTileSourceBase : MultiScaleTileSource
    {
        /// <summary>
        /// the maximum tile level according to the standard map tiling scheme
        /// </summary>
        /// <param name="maxZoom">The maximum tile level according to the standard map tiling scheme </param>
        public MapTileSourceBase(int maxZoom)
            : base((1 << maxZoom) * 256, (1 << maxZoom) * 256, 256, 256, 0)
        {
        }

        /// <summary>
        /// Copyright text 
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// A maximum extend (in wgs coordinates) of the tile data can be specified 
        /// to optimize the number of tile queries
        /// </summary>
        public Rect? MaxRect { get; set; }

        /// <summary>
        /// A minimum tile level according to the standard map tiling scheme can be 
        /// specified to optimize the number of tile queries
        /// </summary>
        public int MinZoom { get; set; }

        protected override void GetTileLayers(int z, int x, int y, IList<object> tileImageLayerSources)
        {
            // map level is msi-level minus 8
            z = z - 8;

            // if MinZoom defined skip if level < MinZoom
            if (z < MinZoom)
                return;

            // skip if tile soutside Maximum geo extend
            if (MaxRect.HasValue)
            {
                Rect r = GeoTransform.TileToWgsAtZoom(x, y, z);
                r.Intersect(MaxRect.Value);
                if (r.IsEmpty)
                    return;
            }

            // get url of the layer handlers
            Uri uri = GetUri(x, y, z);

            if(uri != null)
                tileImageLayerSources.Add(uri);
        }

        /// <summary>
        /// override this method to return the tile uri
        /// </summary>
        /// <param name="x">x-key</param>
        /// <param name="y">y-key</param>
        /// <param name="z">tile level</param>
        /// <returns></returns>
        protected abstract Uri GetUri(int x, int y, int z);
    }
}
