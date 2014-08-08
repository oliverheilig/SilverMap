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

namespace SilverMap.UseCases.SharpMap
{
    public class SharpMapLayer : ILayerPresenter
    {
        PtvDawnNativeTileSource tileSource;
        public SharpMapLayer()
        {
            BitmapImage bmi = new BitmapImage();
            bmi.SetSource(Application.GetResourceStream(new Uri("SilverMap;component/Resources/MapMarket.png", UriKind.Relative)).Stream);
            Icon = bmi;

            tileSource = new PtvDawnNativeTileSource(App.BaseUrl + "/SharpMapTilesHandler.ashx", 19) { MaxRect = new Rect(5, 47, 10, 8), MinZoom = 3 };
        }

        public MapControl MapControl { get; set; }

        public string Name
        {
            get { return "Districts"; }
        }

        public string Category
        {
            get { return LayerCategory.UseCase; }
        }

        public int ZIndex { get; set; }

        public string Caption
        {
            get { return "Districts"; }
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

                foreach (TileLayer layer in this.layerInstances)
                    layer.msi.Opacity = opacity;
            }
        }

        List<TileLayer> layerInstances = new List<TileLayer>();
        public void AddToMap(Map map)
        {
            TileLayer tileLayer = new TileLayer { Name = "DistrictTiles", Source = tileSource };
            Canvas.SetZIndex(tileLayer, this.ZIndex);
            map.BackPaneCanvas.Children.Add(tileLayer);
            tileLayer.msi.Opacity = this.Opacity;
            layerInstances.Add(tileLayer);

            if (MapControl.Map == map) // main Map
                new SelectInteractor(MapControl);
        }

        public void RemoveFromMap(Map map)
        {
            TileLayer tileLayer = map.FindName("DistrictTiles") as TileLayer;
            tileLayer.Remove();
            layerInstances.Remove(tileLayer);

            if (MapControl.Map == map) // main Map
                (map.FindName("SelectInteractor") as SelectInteractor).Remove();
        }

        public string Copyright
        {
            get { return "© Digital Data Services GmbH"; }
        }
    }
}
