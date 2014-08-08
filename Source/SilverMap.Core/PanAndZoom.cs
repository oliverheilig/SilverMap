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
using System.Windows.Shapes;
using Ptvag.Dawn.Controls.SilverMap.Core.Gadgets;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public class PanAndZoom : MapGadget
    {
        private enum DragMode
        {
            None,
            Pan,
            Select
        }

        protected override void Initialize()
        {
            base.Initialize();

            WheelSpeed = .5;
            PanOffset = .25;

            this.map = Map;
            Setup();
        }

        private Point ScreenStartPoint = new Point(0, 0);
        private Point PixelStartPoint = new Point(0, 0);
        private Point startOffset;
        private Map map;
        private DragMode dragMode;

        public bool IsActive { get; set; }

        void Setup()
        {
            IsActive = true;
            map.IsTabStop = true;
            map.KeyDown += new KeyEventHandler(map_KeyDown);
            map.MouseMove += new MouseEventHandler(control_MouseMove);
            map.MouseLeftButtonDown += new MouseButtonEventHandler(source_MouseDown);
            map.MouseLeftButtonUp += new MouseButtonEventHandler(source_MouseUp);
            map.MouseWheel += new MouseWheelEventHandler(source_MouseWheel);
        }

        public double WheelSpeed { get; set; }
        public double PanOffset { get; set; }

        void map_KeyDown(object sender, KeyEventArgs e)
        {
            double minX, minY, maxX, maxY;

            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double dX = (maxX - minX) * PanOffset;
            double dY = (maxY - minY) * PanOffset;

            if (e.PlatformKeyCode == 187) //OEMPlus
            {
                map.Zoom++;
                return;
            }
            if (e.PlatformKeyCode == 189) //OEMMinus
            {
                map.Zoom--;
                return;
            }
            else switch (e.Key)
                {
                    case Key.Up:
                    case Key.W:
                    case Key.NumPad8:
                        map.SetXYZ(map.FinalX, map.FinalY + dY, map.ZoomF, map.UseAnimation);
                        e.Handled = true;
                        break;
                    case Key.Right:
                    case Key.D:
                    case Key.NumPad6:
                        map.SetXYZ(map.FinalX + dX, map.FinalY, map.ZoomF, map.UseAnimation);
                        e.Handled = true;
                        break;
                    case Key.Down:
                    case Key.S:
                    case Key.NumPad2:
                        map.SetXYZ(map.FinalX, map.FinalY - dY, map.ZoomF, map.UseAnimation);
                        e.Handled = true;
                        break;
                    case Key.Left:
                    case Key.A:
                    case Key.NumPad4:
                        map.SetXYZ(map.FinalX - dX, map.FinalY, map.ZoomF, map.UseAnimation);
                        e.Handled = true;
                        break;
                    case Key.Add:
                        map.Zoom++;
                        e.Handled = true;
                        break;
                    case Key.Subtract:
                        map.Zoom--;
                        e.Handled = true;
                        break;
                }
        }

        void source_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            double oldZoom = map.ZoomF;

            double delta = (double)e.Delta * WheelSpeed / 120;

            double newZoom = oldZoom + delta;

            Point p = map.CanvasToPtvMercator(map.GeoCanvas, e.GetPosition(map.GeoCanvas));

            map.ZoomAround(p, newZoom, map.UseAnimation);
        }

        void source_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // we're done.  reset the cursor and release the mouse pointer
            map.Cursor = Cursors.Arrow;
            map.ReleaseMouseCapture();

            if (dragMode == DragMode.Select)
            {
                map.ForePaneCanvas.Children.Remove(dragRectangle);
                dragRectangle = null;

                var physicalPoint = e.GetPosition(map.Layers);

                double minx = physicalPoint.X < PixelStartPoint.X ? physicalPoint.X : PixelStartPoint.X;
                double miny = physicalPoint.Y < PixelStartPoint.Y ? physicalPoint.Y : PixelStartPoint.Y;
                double maxx = physicalPoint.X > PixelStartPoint.X ? physicalPoint.X : PixelStartPoint.X;
                double maxy = physicalPoint.Y > PixelStartPoint.Y ? physicalPoint.Y : PixelStartPoint.Y;

                if (Math.Abs(maxx - minx) < 32 && Math.Abs(maxy - miny) < 32)
                    return;

                Point p1 = map.Layers.TransformToVisual(map.GeoCanvas).Transform(new Point(minx, miny));
                Point p2 = map.Layers.TransformToVisual(map.GeoCanvas).Transform(new Point(maxx, maxy));

                map.SetEnvelope(
                    (p1.X / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize) - 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2,
                    -(p1.Y / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize) + 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2,
                    (p2.X / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize) - 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2,
                    -(p2.Y / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize) + 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2);

                e.Handled = true;
            }
            if (wasPanned)
            {
                wasPanned = false;
                e.Handled = false;
            }

            dragMode = DragMode.None;
        }

        private DateTime clickTime = new DateTime();
        void source_MouseDown(object sender, MouseButtonEventArgs e)
        {
            map.Focus();

            if (DateTime.Now - clickTime < new TimeSpan(0, 0, 0, 0, 200))
            {
                Point p = map.CanvasToPtvMercator(map.GeoCanvas, e.GetPosition(map.GeoCanvas));
                map.ZoomAround(p, map.Zoom+1, map.UseAnimation);

                clickTime = new DateTime(); // reset timer
                e.Handled = true;
                return;
            }
            else
                clickTime = DateTime.Now;
     
            // Save starting point, used later when determining how much to scroll.
            this.ScreenStartPoint = e.GetPosition(map.GeoCanvas);
            this.PixelStartPoint = e.GetPosition(map.Layers);
            this.startOffset = new Point(map.translateTransform.X, map.translateTransform.Y);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
            {
                map.Cursor = Cursors.Arrow;

                //ToDo: how tu use Generic.xaml
                //object o = GetTemplateChild("PART_PixelBox");
                if (dragRectangle != null)
                    map.ForePaneCanvas.Children.Remove(dragRectangle);

                dragRectangle = new Rectangle
                                    {
                                        IsHitTestVisible = false,
                                        Fill = new SolidColorBrush(Color.FromArgb(0x3e, 0x11, 0x57, 0xdc)),
                                        Stroke = new SolidColorBrush(Color.FromArgb(0x55, 0x07, 0x81, 0xf7)),
                                        StrokeDashArray = new DoubleCollection {20, 8},
                                        StrokeEndLineCap = PenLineCap.Round,
                                        StrokeDashCap = PenLineCap.Round,
                                        StrokeThickness = 2,
                                        RadiusX = 4,
                                        RadiusY = 4
                                    };


                Canvas.SetZIndex(dragRectangle, 266);
                Canvas.SetLeft(dragRectangle, PixelStartPoint.X);
                Canvas.SetTop(dragRectangle, PixelStartPoint.Y);
                map.ForePaneCanvas.Children.Add(dragRectangle);
                dragMode = DragMode.Select;
                map.CaptureMouse();
            }
            else if (map.CaptureMouse())
            {
                map.Cursor = Cursors.Hand;
                dragMode = DragMode.Pan;
                wasPanned = false;
            }
        }
        private Rectangle dragRectangle;
        private bool wasPanned = false;

        void control_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsActive)
                return;

            if (dragMode == DragMode.Pan)
            {
                // if the mouse is captured then move the content by changing the translate transform.  
                // use the Pan Animation to animate to the new location based on the delta between the 
                // starting point of the mouse and the current point.
                var physicalPoint = e.GetPosition(map.GeoCanvas);

                if ((this.ScreenStartPoint.X == physicalPoint.X) && (this.ScreenStartPoint.Y == physicalPoint.Y))
                    return;

                double x = map.translateTransform.X - this.ScreenStartPoint.X + physicalPoint.X;
                double y = map.translateTransform.Y - this.ScreenStartPoint.Y + physicalPoint.Y;

                map.x = -x / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize;
                map.y = y / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize;

                wasPanned = true;
 
                map.FireViewportBeginChanged();
                map.DoPan(map.UseAnimation);
            }
            else if (dragMode == DragMode.Select)
            {
                if (dragRectangle == null)
                    return;

                var physicalPoint = e.GetPosition(map.Layers);

                Canvas.SetLeft(dragRectangle, physicalPoint.X < PixelStartPoint.X ? physicalPoint.X : PixelStartPoint.X);
                Canvas.SetTop(dragRectangle, physicalPoint.Y < PixelStartPoint.Y ? physicalPoint.Y : PixelStartPoint.Y);
                dragRectangle.Width = Math.Abs(physicalPoint.X - PixelStartPoint.X);
                dragRectangle.Height = Math.Abs(physicalPoint.Y - PixelStartPoint.Y);
            }
        }
    }
}