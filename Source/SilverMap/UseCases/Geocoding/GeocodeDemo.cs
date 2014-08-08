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
using Ptvag.Dawn.Controls.SilverMap.Core;
using SilverMap.XlocateService;

namespace SilverMap.UseCases.Geocoding
{
    public class GeoCodeResultsLayer : Canvas
    {
        private Map map;
        private ScaleTransform adjustTransform;

        public GeoCodeResultsLayer(Map map, AddressResponse result)
        {
            this.map = map;

            // initialize canvas
            Canvas.SetZIndex(this, 100); // place on top of labels
            this.RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercatorInvertedY);
            this.map.GeoCanvas.Children.Add(this);

            var myresourcedictionary = new ResourceDictionary
            {
                Source = new Uri("/SilverMap;component/IconDictionary.xaml", UriKind.RelativeOrAbsolute)
            };

            var pinTemplate = myresourcedictionary["pinTemplate"] as ControlTemplate;

            // initialize tranformation for power-law scaling of symbols
            adjustTransform = new ScaleTransform();
            AdjustTransform();
            map.ViewportWhileChanged += map_ViewportWhileChanged;

            var allPoints = new List<System.Windows.Point>();

            foreach (ResultAddress address in result.wrappedResultList)
            {
                // transform wgs to ptv mercator coordinate
                var mapPoint = new System.Windows.Point(address.coordinates.point.x, address.coordinates.point.y);

                allPoints.Add(mapPoint);

                // create button and set pin template
                Control pin = new Button();
                pin.Template = pinTemplate;

                // set render transform for power-law scaling
                pin.RenderTransform = adjustTransform;
                pin.RenderTransformOrigin = new System.Windows.Point(1, 1); // scale around lower right

                // set size by magnitude
                pin.Height = 10;
                pin.Width = 10;

                // set tool tip information
                ToolTipService.SetToolTip(pin, string.Format("{0} {1} {2} {3} {4}",
                    address.postCode, address.city, address.city2, address.street, address.houseNumber));

                // set position and add to canvas (invert y-ordinate)
                // set lower right (pin-tip) as position
                Canvas.SetLeft(pin, mapPoint.X - pin.Width);
                Canvas.SetTop(pin, -(mapPoint.Y + pin.Height));
                this.Children.Add(pin);
            }

            if (allPoints.Count > 0)
            {
                Rect rect = RectExtensions.CreateEnvelope(allPoints).Inflate(1.1);

                map.SetEnvelope(rect.Left, rect.Top, rect.Right, rect.Bottom);
            }
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            AdjustTransform();
        }

        public void Remove()
        {
            map.ViewportWhileChanged -= map_ViewportWhileChanged;
            this.map.GeoCanvas.Children.Remove(this);
        }

        /// <summary>
        /// Adjust the transformation for logarithmic scaling
        /// </summary>
        private void AdjustTransform()
        {
            // if the factor is 1.0 the elements have pixel size
            // if the factor is 0.0 the elements have mercator size
            // for a factor between the elements are scaled with a logarithmic multiplicator
            double logicalScaleFactor = .9;

            adjustTransform.ScaleX = 5 * System.Math.Pow(map.CurrentScale, logicalScaleFactor);
            adjustTransform.ScaleY = 5 * System.Math.Pow(map.CurrentScale, logicalScaleFactor);
        }
    }
}
