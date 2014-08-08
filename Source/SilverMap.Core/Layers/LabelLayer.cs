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
using System.Windows.Media.Imaging;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Layers
{
    public class LabelLayer : ILayerPresenter
    {
        public LabelLayer()
        {
            Opacity = 1;
            ZIndex = 1000;
            Caption = Resources.Strings.Labels;

            var bmi = new BitmapImage();
            bmi.SetSource(Application.GetResourceStream(new Uri("Ptvag.Dawn.Controls.SilverMap.Core;component/Resources/Labels.png", UriKind.Relative)).Stream);
            Icon = bmi;
        }

        public IOverlayProvider OverlayProvider { get; set; }

        #region ILayer Members

        public string Name
        {
            get { return "Labels"; }
        }

        public void AddToMap(Map map)
        {
            var layer = new OverlayLayer {OverlayProvider = OverlayProvider, Name = this.Name, Opacity = Opacity};
            map.GeoCanvas.Children.Add(layer);
            layerInstances.Add(layer);
        }

        public string Copyright { get; set; }

        readonly List<OverlayLayer> layerInstances = new List<OverlayLayer>();
        public void RemoveFromMap(Map map)  
        {
            foreach(var element in map.GeoCanvas.Children)
            {
                if(element is FrameworkElement)
                {
                    var frameworkElement = element as FrameworkElement;
                    if (frameworkElement.Name == Name && frameworkElement is OverlayLayer)
                    {
                        var overlay = frameworkElement as OverlayLayer;
                        map.GeoCanvas.Children.Remove(overlay);
                        layerInstances.Remove(overlay);
                        break;
                    }
                }
            }
        }

        public string Category
        {
            get { return LayerCategory.BaseMap; }
        }

        public int ZIndex { get; set; }

        public string Caption { get; set; }

        public ImageSource Icon { get; set; }

        private double opacity;
        public double Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                opacity = value;
                foreach (var layer in this.layerInstances)
                    layer.Opacity = opacity;
            }
        }

        #endregion
    }
}
