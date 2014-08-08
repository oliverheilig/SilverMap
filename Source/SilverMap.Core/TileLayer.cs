//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ptvag.Dawn.Controls.SilverMap.Core.TileSources;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public class TileLayer : Canvas
    {
        public MultiScaleImage msi;
        private Map Map;

        public TileLayer()
        {
            msi = new MultiScaleImage();
            msi.UseSprings = false;
            this.Children.Add(msi);
 
            this.TrilinearFilter = true;
            this.TileThreshold = 1.0;

            this.Loaded += new RoutedEventHandler(Control_Loaded);
        }

        void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (Map != null)
                return;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            var fe = this.Parent as FrameworkElement;

            if (fe == null)
                return;

            while (!(fe is Map))
            {
                fe = fe.Parent as FrameworkElement;
            }

            Map = fe as Map;

            Initialize();
        }

        public void Initialize()
        {
            Map.ViewportWhileChanged += new EventHandler(map_ViewportWhileChanged);

            UpdateMSI();
        }

        public void Remove()
        {
            if (this.Map != null)
            {
                Map.ViewportWhileChanged -= new EventHandler(map_ViewportWhileChanged);

                this.Map.BackPaneCanvas.Children.Remove(this);
            }
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            UpdateMSI();
        }

        public MultiScaleTileSource Source
        {
            set { msi.Source = value; }
            get { return msi.Source; }
        }

        /// <summary>
        /// This value defines the threshold where tile levels are switched TrilinearFilter is off
        /// For TrilinearFilter mode (default) this value defines the blur factor of the msi
        /// </summary>
        public double TileThreshold { get; set; }

        /// <summary>
        /// If this flag is set to true, the msi isn't interpolated over the tile levels
        /// Use this flag for tile layers which contain texts (e.g. OSM)
        /// </summary>
        public bool TrilinearFilter { get; set; }

        private void UpdateMSI()
        {
            // adapt blur factor, so only one level is visible
            if (!TrilinearFilter)
            {
                double blur = 1 + Map.CurrentZoomF - Math.Floor(Map.CurrentZoomF);

                while (blur > TileThreshold * 2)
                    blur = blur * .5;
                while (blur < TileThreshold / 2)
                    blur = blur * 2;

                msi.BlurFactor = blur;
            }
            else
                msi.BlurFactor = TileThreshold;

            // adapt render transform if the tile source uses a different (scaled) tile system
            double f = 1.0;
            if (Source is IScaledTileSource)
            {
                f = (Source as IScaledTileSource).Factor;
                msi.RenderTransform = new ScaleTransform { ScaleX = 1 / f, ScaleY = 1 / f, CenterX = Map.ActualWidth / 2, CenterY = Map.ActualHeight / 2 };
            }

            double rx = .5 + Map.CurrentX / Map.LogicalSize * f;
            double ry = .5 - Map.CurrentY / Map.LogicalSize * f;

            msi.Height = Map.ActualHeight;
            msi.Width = Map.ActualWidth;
            msi.ViewportWidth = Map.CurrentScale / Map.LogicalSize * Map.ActualWidth;
            msi.ViewportOrigin = new Point(rx - Map.CurrentScale / Map.LogicalSize * Map.ActualWidth / 2, ry - Map.CurrentScale / Map.LogicalSize * Map.ActualHeight / 2);           
        }   
    }
}
