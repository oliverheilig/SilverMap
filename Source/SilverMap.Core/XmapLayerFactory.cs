using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ptvag.Dawn.Controls.SilverMap.Core.Layers;
using Ptvag.Dawn.Controls.SilverMap.Core;
using Ptvag.Dawn.Controls.SilverMap.Core.TileSources;
using Ptvag.Dawn.Controls.SilverMap.Core.Overlays;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    /// <summary>
    /// helper extension class to inser xmap base layers
    /// </summary>
    public static class XmapLayerFactory
    {
        public static void InsertXmapBaseLayers(this LayerManager layerManager, string url, string copyrightText, string token = "")
        {
            // add tile layer
            layerManager.Add(new BackgroundLayer
            {
                TileSource = new PtvAjaxTileSource(url),
                Copyright = copyrightText
            });

            // add label overlay layer
            layerManager.Add(new LabelLayer
            {
                OverlayProvider = new PtvAjaxLabelOverlay(url, token),   
                Copyright = copyrightText
            });
        }
    }
}
