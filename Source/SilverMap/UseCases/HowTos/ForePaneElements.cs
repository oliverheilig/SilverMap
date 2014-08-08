//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using Ptvag.Dawn.Controls.SilverMap.Core;

namespace SilverMap.UseCases.HowTos
{
    /// <summary>
    /// Demonstrates the use of fore pane canvas. The elements of the fore pane canvas have to be re-positioned
    /// whenever the map viewport changes
    /// </summary>
    public class ForePaneElements 
    {
        Button button;
        private Map map;

        // set ptv building as center
        double latitude = 49.0136;
        double longitude = 8.4277;

        public ForePaneElements(Map map)
        {
            this.map = map;

            // initialize button
            button = new Button {Content = "PTV headquarters"};

            // set the first position on LayoutUpdated
            button.UpdateLayout();
            button.LayoutUpdated += new EventHandler(button_LayoutUpdated);

            // add to TopPaneCanvas
            map.TopPaneCanvas.Children.Add(button);
        
            // we have to reposition the button while the viewport changes
            map.ViewportWhileChanged += new EventHandler(map_ViewportWhileChanged);

            // center map to button position
            map.SetLatLonZ(latitude, longitude, 16);
        }

        void button_LayoutUpdated(object sender, EventArgs e)
        {
            button.LayoutUpdated -= new EventHandler(button_LayoutUpdated);

            SetButtonPosition();
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            SetButtonPosition();
        }

        private void SetButtonPosition()
        {
            Point geoPoint = new Point(longitude, latitude);

            // set the button invisible if outside the current envelope
            button.Visibility = map.GetCurrentEnvelopeLatLon().Contains(geoPoint) ? Visibility.Visible : Visibility.Collapsed;

            // convert to coordinates of the TopPaneCanvas
            Point pixel = map.WgsToCanvas(map.TopPaneCanvas, geoPoint);

            Canvas.SetLeft(button, pixel.X - button.ActualWidth / 2);
            Canvas.SetTop(button, pixel.Y - button.ActualHeight / 2);
        }

        public void Remove()
        {
            map.TopPaneCanvas.Children.Remove(button);
            map.ViewportWhileChanged -= new EventHandler(map_ViewportWhileChanged);
        }
    }
}