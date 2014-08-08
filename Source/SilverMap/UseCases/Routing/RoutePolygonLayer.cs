//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Ptvag.Dawn.Controls.SilverMap.Core;
using Ptvag.Dawn.Controls.SilverMap.Core.Algorithms;

namespace SilverMap.UseCases.Routing
{
    /// <summary>
    /// Demonstrates display of the route as Xaml Polyline
    /// </summary>
    public class RoutePolygonLayer : Canvas
    {
        private Map map;
        private XrouteService.PlainPoint[] routePoints;
        private string toolTip;

        public RoutePolygonLayer(Map map, SilverMap.XrouteService.Route result)
        {
            // store map
            this.map = map;

            // initialize canvas
            Canvas.SetZIndex(this, 50); // place between bg and labels
            this.RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercatorInvertedY);
            this.map.GeoCanvas.Children.Add(this);

            // store points, initialize toolTip
            this.routePoints = result.polygon.lineString.wrappedPoints;
            this.toolTip = string.Format("{0:0,0.0}km\n{1}", result.info.distance / 1000.0, TimeSpan.FromSeconds(result.info.time));

            // attach event handlers
            map.ViewportWhileChanged += map_ViewportWhileChanged;

            // initially create the polylines out of routePoints
            UpdatePolyline();

            // set map section to show complete route
            ZoomToRoute();
        }

        void ZoomToRoute()
        {
            var winPoints = from plainPoint in routePoints select new Point(plainPoint.x, plainPoint.y);

            Rect rect = RectExtensions.CreateEnvelope(winPoints).Inflate(1.2);

            map.SetEnvelope(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            UpdatePolyline();
        }

        public void Remove()
        {
            map.ViewportWhileChanged -= map_ViewportWhileChanged;
            this.map.GeoCanvas.Children.Remove(this);
        }

        /// <summary>
        /// Adjust the transformation for logarithmic scaling
        /// </summary>
        private void UpdatePolyline()
        {
            // remove previously inserted elements
            this.Children.Clear();

            // clip and shrink routing points according to current viewport and logical map section
            ICollection<Polyline> polys = LineReduction.ClipPolylineReducePoints<Polyline, XrouteService.PlainPoint>(
                ActualSize, ActualClipRect, routePoints,
                (p) => new Point(p.x, p.y), 
                (poly, pnt) => poly.Points.Add(new Point(pnt.X, -pnt.Y))
            );

            foreach (Polyline p in polys)
                Children.Add(StylePolyline(p));

            SetAnimation();
        }

        /// <summary>
        /// Demonstrates the display of directions using an animated ofsset stroke
        /// </summary>
        public void SetAnimation()
        {
            // clip and shrink routing points according to current viewport and logical map section
            ICollection<Polyline> polys = LineReduction.ClipPolylineReducePoints<Polyline, XrouteService.PlainPoint>(
                ActualSize, ActualClipRect, routePoints,
                (p) => new Point(p.x, p.y),
                (poly, pnt) => poly.Points.Add(new Point(pnt.X, -pnt.Y))
            );

            Storyboard sb = new Storyboard();
            foreach (Polyline p in polys)
            {
                p.StrokeThickness = ActualThickness * .75;
                p.Stroke = new SolidColorBrush(Color.FromArgb(64, 0, 0, 0));
                p.StrokeLineJoin = PenLineJoin.Round;
                p.StrokeStartLineCap = PenLineCap.Round;
                p.StrokeEndLineCap = PenLineCap.Triangle;
                p.StrokeDashCap = PenLineCap.Triangle;
                var dc = new DoubleCollection {2, 2};
                p.IsHitTestVisible = false;
                p.StrokeDashArray = dc;

                Children.Add(p);

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 4,
                    To = 0,
                    Duration = new Duration(new TimeSpan(25000000)),
                    FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                sb.Children.Add(animation);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Line.StrokeDashOffset)"));
                Storyboard.SetTarget(animation, p);
            }

            sb.Begin();
        }
    
        private Polyline StylePolyline(Polyline p)
        {
            p.StrokeThickness = ActualThickness;
            p.Stroke = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
            p.StrokeLineJoin = PenLineJoin.Round;
            p.StrokeStartLineCap = PenLineCap.Round;
            p.StrokeEndLineCap = PenLineCap.Triangle;

            ToolTipService.SetToolTip(p, toolTip);

            return p;
        }

        private double ActualThickness
        {
            get
            {
                // if the factor is 1.0 the elements have pixel size
                // if the factor is 0.0 the elements have mercator size
                // for a factor between the elements are scaled with a logarithmic multiplicator
                double logicalScaleFactor = .8;

                // thickness, in logical units
                return 65 * System.Math.Pow(map.CurrentScale, logicalScaleFactor);
            }
        }

        private Size ActualSize
        {
            get
            {
                return new Size(map.ActualWidth, map.ActualHeight);
            }
        }

        private Rect ActualClipRect
        {
            get
            {
                // acutal thickness with 5% extra space
                double thickness = ActualThickness * 1.05;

                // width and height of current logical map section, extended by thickness in each direction. 
                double w = map.CurrentScale * map.ActualWidth + thickness * 2;
                double h = map.CurrentScale * map.ActualHeight + thickness * 2;

                // create and return clip rect
                return new Rect(map.CurrentX - w / 2, map.CurrentY - h / 2, w, h);
            }
        }
    }

}
