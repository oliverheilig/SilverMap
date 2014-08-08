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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GeoAPI.Geometries;
using Ptvag.Dawn.Controls.SilverMap.Core;
using SilverMap.DistrictFeatureService;
using System.Linq;

namespace SilverMap.UseCases.SharpMap
{
    /// <summary>
    /// Demonstrates map-interaction (selection)
    /// </summary>
    public class SelectInteractor : Canvas
    {
        private MapControl mapControl;
        private Map map;
        private Dictionary<string, UIElement> selectedElements = new Dictionary<string, UIElement>();
        private LinearGradientBrush selectionBrush;

        private Polygon dragPolygon;

        public List<string> SelectedIds
        {
            get
            {
                return selectedElements.Keys.ToList();
            }
        }

        public void ClearSelection()
        {
            this.selectedElements.Clear();
            this.Children.Clear();
        }

        public SelectInteractor(MapControl mapControl)
        {
            this.Name = "SelectInteractor";

            this.mapControl = mapControl;
            this.map = mapControl.Map;
            map.GeoCanvas.Children.Add(this);
            this.RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercator);

            map.MouseLeftButtonUp += new MouseButtonEventHandler(map_MouseLeftButtonUp);
            map.MouseLeftButtonDown += new MouseButtonEventHandler(map_MouseLeftButtonDown);
            map.MouseMove += new MouseEventHandler(map_MouseMove);
            map.ViewportWhileChanged += new EventHandler(map_ViewportWhileChanged);

            var g = new GradientStopCollection
                        {
                            new GradientStop {Color = Color.FromArgb(192, 64, 64, 64), Offset = 0},
                            new GradientStop {Color = Color.FromArgb(192, 255, 0, 0), Offset = 1}
                        };

            selectionBrush = new LinearGradientBrush(g, 45);
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            if (dragPolygon != null && polyPoints != null && polyPoints.Count > 0)
            {
                dragPolygon.Points.Clear();
                foreach (var point in polyPoints)
                {
                    dragPolygon.Points.Add(map.PtvMercatorToCanvas(map, point));
                }
            }
        }

        void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectMode == SelectMode.Polygon)
            {
                polyPoints.Add(map.CanvasToPtvMercator(map, e.GetPosition(map)));

                dragPolygon.Points.Clear();
                foreach (var point in polyPoints)
                {
                    dragPolygon.Points.Add(map.PtvMercatorToCanvas(map, point));
                }
            }
        }

        Point p1;
        Point g1;
        SelectMode selectMode = SelectMode.None;
        List<Point> polyPoints;

        void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            p1 = e.GetPosition(map.Layers);
            g1 = map.CanvasToPtvMercator(map, e.GetPosition(map));
            polyPoints = new List<Point> { p1 };

            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                selectMode = SelectMode.Polygon;
                mapControl.PanAndZoom.IsActive = false;
                polyPoints = new List<Point> { g1 };
                dragPolygon = new Polygon
                                  {
                                      IsHitTestVisible = false,
                                      Fill = new SolidColorBrush(Color.FromArgb(0x3e, 0xa0, 0x00, 0x00)),
                                      Stroke = new SolidColorBrush(Color.FromArgb(0x55, 0xff, 0x00, 0x00))
                                  };

                map.TopPaneCanvas.Children.Add(dragPolygon);
            }
            else
                selectMode = SelectMode.Click;
        }

        void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (selectMode == SelectMode.Click)
            {
                Point p2 = e.GetPosition(map.Layers);
                if (Math.Abs(p1.X - p2.X) < 4 && Math.Abs(p1.Y - p2.Y) < 4)
                {
                    Point geoPoint = map.CanvasToPtvMercator(map, e.GetPosition(map));

                    AddDistrict(geoPoint, ((Keyboard.Modifiers & ModifierKeys.Control) != 0) ? SetMode.Xor : SetMode.Set);
                }
            }
            else if (selectMode == SelectMode.Polygon)
            {
                //map.TopPaneCanvas.Children.Remove(dragRectangle);
                //dragRectangle = null;
                map.TopPaneCanvas.Children.Remove(dragPolygon);
                dragPolygon = null;

                Point g2 = map.CanvasToPtvMercator(map, e.GetPosition(map));
                mapControl.PanAndZoom.IsActive = true;
                polyPoints.Add(polyPoints[0]);

                AddDistricts(g1, g2, ((Keyboard.Modifiers & ModifierKeys.Control) != 0) ? SetMode.Xor : SetMode.Set);
            }

            selectMode = SelectMode.None;
        }

        public void Remove()
        {
            ClearSelection();
            map.GeoCanvas.Children.Remove(this);

            map.MouseLeftButtonUp -= new MouseButtonEventHandler(map_MouseLeftButtonUp);
            map.MouseLeftButtonDown -= new MouseButtonEventHandler(map_MouseLeftButtonDown);
            map.MouseMove -= new MouseEventHandler(map_MouseMove);
            map.ViewportWhileChanged -= new EventHandler(map_ViewportWhileChanged);
        }

        public void AddDistricts(Point p1, Point p2, SetMode setMode)
        {
            SilverMap.DistrictFeatureService.IDistrictFeatureService districtFeatures =
                new SilverMap.DistrictFeatureService.DistrictFeatureServiceClient(new System.ServiceModel.BasicHttpBinding { MaxReceivedMessageSize = 2147483647 },
                    new System.ServiceModel.EndpointAddress(App.BaseUrl + "/DistrictFeatureService.svc"));
            var c = new System.Collections.ObjectModel.ObservableCollection<PolyPoint>(from p in polyPoints select new PolyPoint { X = p.X, Y = p.Y });

            districtFeatures.BeginGetDistrictFeaturePolygon("eur_PLZ", c,
            new AsyncCallback(GetFeaturePolyAsync),
                            Tuple.Create<IDistrictFeatureService, SetMode>(districtFeatures, setMode));
        }


        public void AddDistrict(Point p, SetMode setMode)
        {
            SilverMap.DistrictFeatureService.IDistrictFeatureService districtFeatures =
                new SilverMap.DistrictFeatureService.DistrictFeatureServiceClient(new System.ServiceModel.BasicHttpBinding { MaxReceivedMessageSize = 2147483647 },
                    new System.ServiceModel.EndpointAddress(App.BaseUrl + "/DistrictFeatureService.svc"));

            districtFeatures.BeginGetDistrictFeaturePoint("eur_PLZ", p.X, p.Y, new AsyncCallback(GetFeaturePointAsync),
                Tuple.Create<IDistrictFeatureService, SetMode>(districtFeatures, setMode));
        }

        public void GetFeaturePolyAsync(IAsyncResult result)
        {
            var state = result.AsyncState as Tuple<IDistrictFeatureService, SetMode>;

            var feature = state.Item1.EndGetDistrictFeaturePolygon(result);

            map.Dispatcher.BeginInvoke(new Action<IList<DistrictFeature>, SetMode>(ShowDistrictInfo), feature, state.Item2);
        }

        public void GetFeaturePointAsync(IAsyncResult result)
        {
            var state = result.AsyncState as Tuple<IDistrictFeatureService, SetMode>;

            var feature = state.Item1.EndGetDistrictFeaturePoint(result);

            map.Dispatcher.BeginInvoke(new Action<IList<DistrictFeature>, SetMode>(ShowDistrictInfo), feature, state.Item2);
        }

        public void ShowDistrictInfo(IList<DistrictFeature> features, SetMode setMode)
        {
            if (setMode == SetMode.Set)
            {
                ClearSelection();
            }

            if (features.Count == 0)
                return;

            foreach (var feature in features)
            {
                IGeometry geometry = new GisSharpBlog.NetTopologySuite.IO.WKBReader().Read(feature.GeometryWkb);

                if (!(geometry is IPolygon) && !(geometry is IMultiPolygon))
                    continue;

                if (setMode == SetMode.Add && selectedElements.ContainsKey(feature.Id))
                {
                    continue;
                }

                if (setMode == SetMode.Xor && selectedElements.ContainsKey(feature.Id))
                {
                    this.Children.Remove(selectedElements[feature.Id]);
                    this.selectedElements.Remove(feature.Id);
                    continue;
                }

                var path = new Path {Fill = selectionBrush, Data = Ogc2Xaml.ConvertPolygon(geometry)};

                ToolTipService.SetToolTip(path, feature.Name);
                this.Children.Add(path);
                this.selectedElements[feature.Id] = path;

                // add some fancy animation
                var sc = new ScaleTransform {CenterX = geometry.Centroid.X, CenterY = geometry.Centroid.Y};
                path.RenderTransform = sc;
                var sb = new Storyboard();
                var animation1 = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(500), From = 0, To = 1, EasingFunction = new QuinticEase() };
                var animation2 = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(500), From = 0, To = 1, EasingFunction = new QuinticEase() };
                sb.Children.Add(animation1);
                sb.Children.Add(animation2);
                Storyboard.SetTarget(animation1, sc);
                Storyboard.SetTarget(animation2, sc);
                Storyboard.SetTargetProperty(animation1, new PropertyPath("(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(animation2, new PropertyPath("(ScaleTransform.ScaleY)"));
                sb.Begin();
            }
        }
    }
        
    public enum SetMode
    {
        Set,
        Add,
        Xor
    }

    public enum SelectMode
    {
        None,
        Click,
        Polygon
    }
}
