//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    /// <summary>
    /// Interaction logic for ZoomSliderControl.xaml
    /// </summary>
    public partial class ZoomSliderControl : MapGadget
    {
        public ZoomSliderControl()
        {
            InitializeComponent();
        }

        protected override void Initialize()
        {
            zoomSlider.Value = Map.ZoomF * 100;

            zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(zoomSlider_ValueChanged);
            Map.ViewportBeginChanged += new EventHandler(map_MapChangedEvent);

            base.Initialize();
        }

        bool selfNotify = false;
        void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (selfNotify)
                return;

            if (Math.Abs(Map.ZoomF * 100 - MapZoom) > Double.Epsilon)
                Map.ZoomF = (double)e.NewValue / 100;
        }


        void map_MapChangedEvent(object sender, EventArgs e)
        {
            MapZoom = (int)(Map.ZoomF * 100);
        }

        public int MapZoom
        {
            get
            {
                return (int)GetValue(MapZoomProperty);
            }
            set
            {
                selfNotify = true;
                SetValue(MapZoomProperty, value);
                selfNotify = false;
            }
        }

        public static readonly DependencyProperty MapZoomProperty =
DependencyProperty.Register
("MapZoom", typeof(int), typeof(ZoomSliderControl), null);
    }
}
