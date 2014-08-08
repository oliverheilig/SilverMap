//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows.Controls;
using Ptvag.Dawn.Controls.SilverMap.Core;
using System.Windows.Media;
using System;
using System.Windows.Media.Imaging;
using System.Windows;

namespace SilverMap.UseCases.Wms
{
    public class WmsUseCase
    {
        private Image legend;
        private StackPanel workspace;
        LayerManager layerManager;
        WmsLayer layer;

        public WmsUseCase(StackPanel workspace, LayerManager layerManager, Map map)
        {
            this.workspace = workspace;
            this.layerManager = layerManager;

            // server-side legend not workin (GIF)
            // legend.Source = new BitmapImage(new Uri("http://rips-uis.lubw.baden-wuerttemberg.de/rips/brsweb_landnutzung/legende/ln2000.gif"));

            //BitmapImage bmi = new BitmapImage();
            //bmi.SetSource(Application.GetResourceStream(new Uri("SilverMap;component/Resources/ln2000.png", UriKind.Relative)).Stream);
            //legend = new Image { Source = bmi, Stretch = System.Windows.Media.Stretch.None};
            //workspace.Children.Add(legend);

            layer = new WmsLayer {ZIndex = 49};
            layerManager.Add(layer);

            map.SetLatLonZ(49.0136, 8.4277, 15);
        }

        public void Remove()
        {
            this.layerManager.Remove(layer);
//            workspace.Children.Remove(legend);
        }
    }
}
