//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Ptvag.Dawn.Controls.SilverMap.Core;

namespace SilverMap.UseCases.HowTos
{
    /// <summary>
    /// Demontrates the use of the geo canvas. The geo canvas can add any child canvas element 
    /// which can be matched to the geo canvas via a render transorm. For the default map this
    /// is the case for canvases which hold elements in spherical PTV/Web Mercator coordinates.
    /// </summary>
    public class SimpleShapeDemo : Canvas
    {
        private Map map;

        public SimpleShapeDemo(Map map)
        {
            this.map = map;

            // Create a new Wpf Canvas and initialize it with the render transform for ptv mercator
            // This means all child objects are defined with ptv mercator coordinates
            RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercator);
            Canvas.SetZIndex(this, 50); // place it between background (32) and labels (64)
            map.GeoCanvas.Children.Add(this);

            SetEllipse();
        }

        void SetEllipse()
        {
            // set ptv building as center
            double latitude = 49.0136;
            double longitude = 8.4277;

            // calculate ptv location in ptv mercator units
            Point mercatorPoint = GeoTransform.WGSToPtvMercator(new Point(longitude, latitude));

            // we want to display a circle with a radius of 250 meters around the ptv location
            // calculate the corrected distance which takes the mercator projection into account
            double radius = 250; // radius in meters
            double cosB = Math.Cos((latitude / 360.0) * (2 * Math.PI)); // factor depends on latitude
            double ellipseSize = Math.Abs(1.0 / cosB * radius) * 2; // size mercator units

            // Create the ellipse and insert it to our canvas
            Ellipse ellipse = new Ellipse
            {
                Width = ellipseSize,
                Height = ellipseSize,
                Fill = new SolidColorBrush(Color.FromArgb(192, 0, 0, 255)),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 20
            };

            // set position and add to map
            Canvas.SetLeft(ellipse, mercatorPoint.X - ellipseSize / 2);
            Canvas.SetTop(ellipse, mercatorPoint.Y - ellipseSize / 2);

            // add ellipse to canvas
            this.Children.Add(ellipse);

            // set tool tip
            ToolTipService.SetToolTip(ellipse, "250 meters around ptv headquarters");

            // center map
            //same as map.SetLatLonZ(latituce, longitude, 160);
            map.SetXYZ(mercatorPoint.X, mercatorPoint.Y, 16);
            
            // some additional fancy effects

            // gradient fill
            var g = new GradientStopCollection
                        {
                            new GradientStop {Color = Color.FromArgb(192, 0, 0, 255), Offset = 0},
                            new GradientStop {Color = Color.FromArgb(192, 255, 255, 255), Offset = 1}
                        };
            ellipse.Fill = new LinearGradientBrush(g, 45);

            // mouse-over effect
            ellipse.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);

            // dropshadows are a performance-killer and seem to have some problems in SL
            // http://forums.silverlight.net/forums/p/120302/282307.aspx (but seems to be fixed in SL5)
            //ellipse.Effect = new System.Windows.Media.Effects.DropShadowEffect
            //{
            //    Color = Colors.Black,
            //    ShadowDepth = 15,
            //    Opacity = .8,
            //    BlurRadius = 3,
            //    Direction = 45,
            //};
        }

        void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = (Ellipse)sender;

            double scale = 2;
            var sc = new ScaleTransform { CenterX = ellipse.Width / 2, CenterY = ellipse.Height / 2 };
            ellipse.RenderTransform = sc;
            var sb = new Storyboard();
            var animation1 = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(250), To = scale, AutoReverse = true, EasingFunction = new QuinticEase() };
            var animation2 = new DoubleAnimation { Duration = TimeSpan.FromMilliseconds(250), To = scale, AutoReverse = true, EasingFunction = new QuinticEase() };
            sb.Children.Add(animation1);
            sb.Children.Add(animation2);
            Storyboard.SetTarget(animation1, sc);
            Storyboard.SetTarget(animation2, sc);
            Storyboard.SetTargetProperty(animation1, new PropertyPath("(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(animation2, new PropertyPath("(ScaleTransform.ScaleY)"));
            sb.Begin();
        }

        public void Remove()
        {
            map.GeoCanvas.Children.Remove(this);
        }
    }
}
