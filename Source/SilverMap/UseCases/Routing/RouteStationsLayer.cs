//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ptvag.Dawn.Controls.SilverMap.Core;
using Ptvag.Dawn.Controls.WpfMap.Symbols;

namespace SilverMap.UseCases.Routing
{
    public class RouteStationsLayer : Canvas
    {
        private Map map;
        private ScaleTransform adjustTransform;

        public RouteStationsLayer(Map map)
        {
            this.map = map;

            // initialize canvas
            this.map.TopPaneCanvas.Children.Add(this);
                
            // initialize transformation for power-law scaling of symbols
            adjustTransform = new ScaleTransform();
            map.ViewportWhileChanged += map_ViewportWhileChanged;
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveStation != null)
                RemoveStation(selectedElement.Tag as SilverMap.XrouteService.PlainPoint);
        }

        public delegate void RemoveStationDelegate(SilverMap.XrouteService.PlainPoint p);
        public RemoveStationDelegate RemoveStation;

        ObservableCollection<SilverMap.XrouteService.PlainPoint> points;
        public ObservableCollection<SilverMap.XrouteService.PlainPoint> Points
        {
            get { return points; }
            set
            {
                if (points != null)
                    points.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(points_CollectionChanged);

                this.points = value;

                if (points != null)
                    points.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(points_CollectionChanged);

                UpdateStationPins();
            }
        }

        private void UpdateStationPins()
        {
            this.Children.Clear();

            int index = 0;
            foreach (var wayPoint in points)
            {
                Pin pin = new Pin();
                if (index == 0)
                    pin.Color = Colors.Green;
                else if (index == points.Count - 1)
                    pin.Color = Colors.Red;
                else
                    pin.Color = Colors.Blue;

                pin.MouseRightButtonDown += new MouseButtonEventHandler(pin_MouseRightButtonDown);

                ContextMenu cm = new ContextMenu();
                MenuItem item = new MenuItem() { Header = "Remove" };
                item.Click += new RoutedEventHandler(item_Click);
                cm.Items.Add(item);
                ContextMenuService.SetContextMenu(pin, cm);

                pin.Width = 100;
                pin.Height = 100;

                // set render transform for power-law scaling
                pin.RenderTransform = adjustTransform;
                pin.RenderTransformOrigin = new System.Windows.Point(1, 1); // scale around lower right

                pin.Tag = wayPoint;

                // set tool tip information
                ToolTipService.SetToolTip(pin, "Waypoint " + (++index).ToString());

                // have to set a unique name for styleable pins (see also GeoRSS)
                pin.Name = "Waypoint" + index.ToString();

                this.Children.Add(pin);
            }

            UpdatePinPostitions();
        }

        void points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateStationPins();
        }

        FrameworkElement selectedElement = null;
        void pin_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedElement = sender as FrameworkElement;
            e.Handled = false;
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            UpdatePinPostitions();
        }

        public void Remove()
        {
            map.ViewportWhileChanged -= map_ViewportWhileChanged;
            this.map.TopPaneCanvas.Children.Remove(this);
        }

        /// <summary>
        /// Update 
        /// </summary>
        private void UpdatePinPostitions()
        {
            Rect envelope = map.GetCurrentEnvelopePtvMercator();

            foreach (Control pin in this.Children)
            {
                SilverMap.XrouteService.PlainPoint wayPoint = pin.Tag as SilverMap.XrouteService.PlainPoint;
                Point point = new Point { X = wayPoint.x, Y = wayPoint.y };

                pin.Visibility = envelope.Contains(point)? Visibility.Visible : Visibility.Collapsed;

                Point canvasPoint = map.PtvMercatorToCanvas(map.TopPaneCanvas, point);

                Canvas.SetLeft(pin, canvasPoint.X - pin.Width);
                Canvas.SetTop(pin, canvasPoint.Y - pin.Height);
            }

            // if the factor is 1.0 the elements have mercator size
            // if the factor is 0.0 the elements have pixel size
            // for a factor between the elements are scaled with a logarithmic multiplicator
            double logicalScaleFactor = .1;

            double scale = 1.0 / System.Math.Pow(map.CurrentScale, logicalScaleFactor);

            adjustTransform.ScaleX = scale;
            adjustTransform.ScaleY = scale;
        }
    }
}
