//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public class OverviewMap : Map
    {
        public int ZoomDelta { get; set; }

        public OverviewMap()
        {
            IsEnabled = false;
            ZoomDelta = 4;

            var c = new Canvas();
            dragRectangle = new Rectangle
                                {
                                    IsHitTestVisible = false,
                                    Fill = new SolidColorBrush(Color.FromArgb(0x80, 0xaa, 0xaa, 0xaa)),
                                    Stroke = new SolidColorBrush(Colors.Black),
                                    StrokeEndLineCap = PenLineCap.Round,
                                    StrokeDashCap = PenLineCap.Round,
                                    StrokeThickness = 1.5,
                                    RadiusX = 8,
                                    RadiusY = 8
                                };
            //            dragRectangle.StrokeDashArray = new DoubleCollection(new double[] { 1, 4 });

            c.Children.Add(dragRectangle);
            c.RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercator);
            Canvas.SetZIndex(c, 266);
            this.GeoCanvas.Children.Add(c);
        }

        private Map parentMap;
        public Map ParentMap
        {
            set
            {
                if (parentMap != null)
                {
                    this.ViewportBeginChanged -= new EventHandler(OverviewMap_ViewportBeginChanged);
                    parentMap.ViewportBeginChanged -= new EventHandler(parentMap_ViewportBeginChanged);
                    parentMap.ViewportWhileChanged -= new EventHandler(parentMap_ViewportWhileChanged);
                 }

                parentMap = value;

                if (parentMap != null)
                {
                    this.ViewportBeginChanged += new EventHandler(OverviewMap_ViewportBeginChanged);
                    parentMap.ViewportBeginChanged += new EventHandler(parentMap_ViewportBeginChanged);
                    parentMap.ViewportWhileChanged += new EventHandler(parentMap_ViewportWhileChanged);
//                    this.MinZoom = parentMap.MinZoom - ZoomDelta;

                    UpdateOverviewMap(parentMap.UseAnimation);
                }
            }
            get
            {
                return parentMap;
            }
        }

        void parentMap_ViewportWhileChanged(object sender, EventArgs e)
        {
            UpdateRect();
        }

        public void UpdateRect()
        {
            if (!MapElementExtensions.IsControlVisible(this))
                return;

            double minX, minY, maxX, maxY;
            parentMap.GetCurrentEnvelope(out minX, out minY, out maxX, out maxY);

            Canvas.SetLeft(dragRectangle, minX);
            Canvas.SetTop(dragRectangle, minY);
            dragRectangle.Width = maxX - minX;
            dragRectangle.Height = maxY - minY;
            dragRectangle.StrokeThickness = 10 * parentMap.CurrentScale;
        }

        void parentMap_ViewportBeginChanged(object sender, EventArgs e)
        {
            UpdateOverviewMap(parentMap.UseAnimation);
        }

        void OverviewMap_ViewportBeginChanged(object sender, EventArgs e)
        {
//            UpdateParentMap();
        }

        private bool selfNotify = false;
        Rectangle dragRectangle;
        public void UpdateOverviewMap(bool animate)
        {
            if (!MapElementExtensions.IsControlVisible(this))
                return;
            
            int newZoom = parentMap.Zoom - ZoomDelta;

            selfNotify = true;

            SetXYZ(parentMap.FinalX, parentMap.FinalY, newZoom, animate);

            selfNotify = false;
        }

        void UpdateParentMap()
        {
            if (selfNotify)
                return;

            int newZoom = Zoom + ZoomDelta;
            if (newZoom > parentMap.MaxZoom)
                newZoom = parentMap.MaxZoom;

            parentMap.SetXYZ(parentMap.FinalX, parentMap.FinalY, newZoom, parentMap.UseAnimation);
        }
    }
}