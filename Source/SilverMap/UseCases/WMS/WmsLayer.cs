//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ptvag.Dawn.Controls.SilverMap.Core;
using Ptvag.Dawn.Controls.SilverMap.Core.TileSources;
using System.Windows.Media.Imaging;
using System;

namespace SilverMap.UseCases.Wms
{
    public class WmsLayer : ILayerPresenter
    {
        Wms900913TileSource tileSource;
        public WmsLayer()
        {
            //BitmapImage bmi = new BitmapImage();
            //bmi.SetSource(Application.GetResourceStream(new Uri("SilverMap;component/Resources/BaWue.png", UriKind.Relative)).Stream);
            //Icon = bmi;

            //#region doc:wms
            //// http://ows.terrestris.de/dienste.html
            //string wmsUrl = "http://ows.terrestris.de/osm-haltestellen?LAYERS=OSM-Bushaltestellen&TRANSPARENT=true&FORMAT=image%2Fpng&SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&STYLES=&SRS=EPSG%3A900913";
            //wpfMap.Layers.Add(new UntiledLayer("WmsOverlayLayerTest") { Caption = "WMS Overlay Layer", UntiledProvider = new WmsUntiledProvider(wmsUrl, 19) });
            //#endregion

            // Zoom to Karlsruhe.
//            wpfMap.SetMapLocation(new System.Windows.Point(8.4, 49), 15);

            tileSource = new Wms900913TileSource(
                "http://ows.terrestris.de/osm-haltestellen?LAYERS=OSM-Bushaltestellen&TRANSPARENT=true&FORMAT=image%2Fpng&SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&STYLES=",
                19) { MinZoom = 5};
       }

        public string Name
        {
            get { return "Stations"; }
        }

        public string Category
        {
            get { return LayerCategory.UseCase; }
        }

        public int ZIndex { get; set; }

        public string Caption
        {
            get { return "Stations"; }
        }

        public ImageSource Icon { get; set; }

        double opacity = .9;
        public double Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                opacity = value;

                foreach (OverlayLayer layer in this.layerInstances)
                    layer.Opacity = opacity;
            }
        }

        List<OverlayLayer> layerInstances = new List<OverlayLayer>();
        public void AddToMap(Map map)
        {
            OverlayLayer tileLayer = new OverlayLayer { Name = "ln2010", OverlayProvider = tileSource };
            Canvas.SetZIndex(tileLayer, this.ZIndex);
            map.GeoCanvas.Children.Add(tileLayer);
            tileLayer.Opacity = this.Opacity;
            layerInstances.Add(tileLayer);
        }

        public void RemoveFromMap(Map map)
        {
            OverlayLayer tileLayer = map.FindName("ln2010") as OverlayLayer;
            tileLayer.Remove();
            layerInstances.Remove(tileLayer);
        }

        public string Copyright
        {
            get { return "© LUBW"; }
        }
    }
}
