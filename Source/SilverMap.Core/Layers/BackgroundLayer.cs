//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Layers
{
    public class BackgroundLayer : ILayerPresenter
    {
        public BackgroundLayer()
        {
            Name = "Background";
            Opacity = 1.0;
            ZIndex = 0;
            TrilinearFilter = true;
            TileThreshold = 1.0;

            InitializeResources();
        }

        protected void InitializeResources()
        {
            if (IsAerial)
            {
                Caption = Resources.Strings.Aerials;
                var bmi = new BitmapImage();
                bmi.SetSource(Application.GetResourceStream(new Uri("Ptvag.Dawn.Controls.SilverMap.Core;component/Resources/Aerials.png", UriKind.Relative)).Stream);
                this.Icon = bmi;
            }
            else
            {
                Caption = Resources.Strings.Background;
                var bmi = new BitmapImage();
                bmi.SetSource(Application.GetResourceStream(new Uri("Ptvag.Dawn.Controls.SilverMap.Core;component/Resources/Background.png", UriKind.Relative)).Stream);
                this.Icon = bmi;
            }
        }

        private MultiScaleTileSource tileSouce;
        public MultiScaleTileSource TileSource
        {
            get { return tileSouce; }
            set 
            { 
                tileSouce = value;
                foreach (TileLayer layer in this.layerInstances)
                    layer.Source = tileSouce;
            }
        }

        #region ILayer Members

        public string Name { get; set; }
        public int ZIndex { get; set; }

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
                foreach (TileLayer layer in this.layerInstances)
                    layer.msi.Opacity = Opacity;
            }
        }

        public string Copyright { get; set; }

        private bool isAerial;
        public bool IsAerial
        {
            get { return isAerial; }
            set
            {
                isAerial = value;
                InitializeResources();
            }
        }

        public bool TrilinearFilter { get; set; }
        public double TileThreshold { get; set; }

        List<TileLayer> layerInstances = new List<TileLayer>();
        public void AddToMap(Map map)
        {
            TileLayer tileLayer = new TileLayer { Name = Name };
            tileLayer.TrilinearFilter = this.TrilinearFilter;
            tileLayer.TileThreshold = this.TileThreshold;
            tileLayer.msi.Opacity = Opacity;
            Canvas.SetZIndex(tileLayer, ZIndex);
            map.BackPaneCanvas.Children.Add(tileLayer);

            tileLayer.Source = TileSource;

            layerInstances.Add(tileLayer);
        }

        public void RemoveFromMap(Map map)
        {
            foreach (UIElement element in map.BackPaneCanvas.Children)
            {
                if (element is FrameworkElement)
                {
                    FrameworkElement frameworkElement = element as FrameworkElement;
                    if (frameworkElement.Name == Name && frameworkElement is TileLayer)
                    {
                        TileLayer tileLayer = frameworkElement as TileLayer;
                        map.BackPaneCanvas.Children.Remove(tileLayer);
                        tileLayer.Source = null;
                        layerInstances.Remove(tileLayer);
                        break;
                    }
                }
            }
        }

        public string Category
        {
            get { return LayerCategory.BaseMap; }
        }

        public string Caption {get; set; }

        public ImageSource Icon {get; set;}

        #endregion
    }
}
