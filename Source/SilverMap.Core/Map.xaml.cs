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
using System.Windows.Media.Animation;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    /// <summary>
    /// The Main Control for SilverMap
    /// </summary>
    public partial class Map : UserControl
    {
        /// <summary>
        /// The earth radius of the map is 6371000.0 (so the main projection is PTV_Merctor)
        /// </summary>
        public const double EARTH_RADIUS = 6371000.0;

        /// <summary>
        /// the size of the Map in logical coordinates 
        /// </summary>
        public static double LogicalSize = EARTH_RADIUS * 2.0 * Math.PI;

        /// <summary>
        /// an arbitrary value 
        /// </summary>
        public static double ReferenceSize = 512;

        /// <summary>
        /// the zoom adjust is used to minimize rounding errros in deep zoom levels
        /// </summary>
        public static double ZoomAdjust = Math.Pow(2, 24);

        private double envMinX, envMinY, envMaxX, envMaxY;

        internal TranslateTransform logicalOffsetTransform;
        internal TranslateTransform translateTransform;
        internal ScaleTransform zoomTransform;
        internal TranslateTransform screenOffsetTransform;
        internal TranslateTransform logicalWheelOffsetTransform;
        internal TranslateTransform physicalWheelOffsetTransform;
        internal TransformGroup transformGroup;

        // internal map  parameters
        public double x = 0;
        public double y = 0;
        public double z = 1;

        // viewport events
        public event EventHandler ViewportBeginChanged;
        public event EventHandler ViewportWhileChanged;
        public event EventHandler ViewportEndChanged;

        // animations
        private Storyboard panStoryboard;
        private PointAnimation panAnimation;
        private Storyboard zoomStoryboard;
        private DoubleAnimation zoomAnimation;

        private bool animatePan, animateZoom;

        private int maxZoom;
        public int MaxZoom
        {
            get
            {
                return maxZoom;
            }
            set
            {
                maxZoom = value;
                if (Zoom > maxZoom)
                    Zoom = maxZoom;
            }
        }

        private int minZoom;
        public int MinZoom
        {
            get
            {
                return minZoom;
            }
            set
            {
                minZoom = value;
                if (Zoom < minZoom)
                    Zoom = minZoom;
            }
        }

        /// <summary>
        /// Creates a new instance of the map.
        /// </summary>
        public Map()
        {
            InitializeComponent();

            MaxZoom = 19;
            MinZoom = 1;
            UseAnimation = true;

            // initialize transformation stack
            this.logicalOffsetTransform = new TranslateTransform { X = -ReferenceSize / 2, Y = -ReferenceSize / 2 };
            this.translateTransform = new TranslateTransform();
            double zoomScale = Math.Pow(2, z + 8) / ReferenceSize / ZoomAdjust;
            this.zoomTransform = new ScaleTransform { ScaleX = zoomScale, ScaleY = zoomScale };

            this.screenOffsetTransform = new TranslateTransform();
            this.logicalWheelOffsetTransform = new TranslateTransform();
            this.physicalWheelOffsetTransform = new TranslateTransform();

            this.transformGroup = new TransformGroup();
            this.transformGroup.Children.Add(this.logicalOffsetTransform);
            this.transformGroup.Children.Add(this.translateTransform);
            this.transformGroup.Children.Add(this.logicalWheelOffsetTransform);
            this.transformGroup.Children.Add(this.zoomTransform);
            this.transformGroup.Children.Add(this.physicalWheelOffsetTransform);
            this.transformGroup.Children.Add(this.screenOffsetTransform);
            this.GeoCanvas.RenderTransform = this.transformGroup;

            this.SizeChanged += new SizeChangedEventHandler(Map_SizeChanged);

            // initialize storyboards
            this.panStoryboard = new Storyboard();
            this.panAnimation = new PointAnimation
                                    {
                                        EasingFunction =
                                            new CubicEase
                                                {EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut}
                                    };
            Storyboard.SetTargetProperty(panAnimation, new PropertyPath("(Map.Center)"));
            Storyboard.SetTarget(panAnimation, this);
            this.panAnimation.Completed += new EventHandler(panAnimation_Completed);
            this.panStoryboard.Children.Add(panAnimation);

            this.zoomStoryboard = new Storyboard();
            this.zoomAnimation = new DoubleAnimation
                                     {
                                         EasingFunction =
                                             new CubicEase
                                                 {EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut}
                                     };
            this.zoomAnimation.Completed += new EventHandler(zoomAnimation_Completed);
            this.zoomStoryboard.Children.Add(zoomAnimation);
            Storyboard.SetTargetProperty(zoomAnimation, new PropertyPath("(Map.ZoomScale)"));
            Storyboard.SetTarget(zoomAnimation, this);
        }

        void zoomAnimation_Completed(object sender, EventArgs e)
        {
            animateZoom = false;

            FireViewportEndChanged();
        }

        void panAnimation_Completed(object sender, EventArgs e)
        {
            animatePan = false;

            FireViewportEndChanged();
        }

        /// <summary>
        /// Positions the map to a center and zoom factor
        /// </summary>
        /// <param name="x">x-coordinate in logical units</param>
        /// <param name="y">y-coordinate in logical units</param>
        /// <param name="zoom">the zoom factor according to the standard tiling/zooming scheme.</param>
        public void SetXYZ(double x, double y, double zoom)
        {
            SetXYZ(x, y, zoom, UseAnimation);
        }

        /// <summary>
        /// Gets the bounding box of the visible map section while the map is in motion.
        /// </summary>
        /// <param name="minX">The lower x-value</param>
        /// <param name="minY">The lower y-value</param>
        /// <param name="maxX">The upper x-value</param>
        /// <param name="maxY">The upper y-value</param>
        public void GetCurrentEnvelope(out double minX, out double minY, out  double maxX, out double maxY)
        {
            minX = CurrentX - ActualWidth * CurrentScale / 2;
            maxY = CurrentY + ActualHeight * CurrentScale / 2;
            maxX = CurrentX + ActualWidth * CurrentScale / 2;
            minY = CurrentY - ActualHeight * CurrentScale / 2;
        }

        /// <summary>
        /// Gets the anticipated bounding box of the visible map section after the map was in motion / the
        /// current bbox while the map is in motion
        /// </summary>
        /// <param name="minX">The lower x-value</param>
        /// <param name="minY">The lower y-value</param>
        /// <param name="maxX">The upper x-value</param>
        /// <param name="maxY">The upper y-value</param>
        public void GetFinalEnvelope(out double minX, out double minY, out  double maxX, out double maxY)
        {
            minX = FinalX - ActualWidth * FinalScale / 2;
            maxY = FinalY + ActualHeight * FinalScale / 2;
            maxX = FinalX + ActualWidth * FinalScale / 2;
            minY = FinalY - ActualHeight * FinalScale / 2;
        }

        /// <summary>
        /// Sets the visible map section to a bouding box, so the bbox is contained in the map sectiion.
        /// </summary>
        /// <param name="minX">The lower x-value</param>
        /// <param name="minY">The lower y-value</param>
        /// <param name="maxX">The upper x-value</param>
        /// <param name="maxY">The upper y-value</param>
        public void SetEnvelope(double minX, double minY, double maxX, double maxY)
        {
            SetEnvelope(minX, minY, maxX, maxY, UseAnimation);
        }

        /// <summary>
        /// Sets the visible map section to a bouding box, so the bbox is contained in the map sectiion.
        /// </summary>
        /// <param name="minX">The lower x-value</param>
        /// <param name="minY">The lower y-value</param>
        /// <param name="maxX">The upper x-value</param>
        /// <param name="maxY">The upper y--value</param>
        /// <param name="animate">Animate the transition to new new map section</param>
        public void SetEnvelope(double minX, double minY, double maxX, double maxY, bool animate)
        {
            if (ActualHeight == 0 || ActualWidth == 0)
            {
                envMinX = minX;
                envMaxX = maxX;
                envMinY = minY;
                envMaxY = maxY;
                return;
            }

            double dx = Math.Abs(maxX - minX);
            double dy = Math.Abs(maxY - minY);

            double zoomX;
            if (dx == 0)
                zoomX = MaxZoom;
            else
            {
                double scale = (dx / LogicalSize) * 256 / ActualWidth;
                double z = Math.Log(scale, 2);
                zoomX = -z;
                //                zoomX = System.Convert.ToInt32(Math.Floor(-z));
            }

            double zoomY;
            if (dy == 0)
                zoomY = MaxZoom;
            else
            {
                double scale = (dy / LogicalSize) * 256 / ActualHeight;
                double z = Math.Log(scale, 2);
                zoomY = -z;
//                zoomY = System.Convert.ToInt32(Math.Floor(-z));
            }

            double tmpZoom = Math.Min(zoomX, zoomY);

            if (tmpZoom > MaxZoom)
                tmpZoom = MaxZoom;
            if (tmpZoom < MinZoom)
                tmpZoom = MinZoom;

            SetXYZ((minX + maxX) / 2, (minY + maxY) / 2, tmpZoom, animate);
        }

        private void FlyTo(Point point, double zoom, bool useAnimation)
        {
            double zTo = Math.Pow(2, zoom + 8) / ReferenceSize / ZoomAdjust;
            double zCurrent = zoomTransform.ScaleX;

            double dZ = (zTo / zCurrent);
            dZ = dZ / (dZ - 1);

            double newX = (point.X - CurrentX) * dZ + CurrentX;
            double newY = (point.Y - CurrentY) * dZ + CurrentY;

            ZoomAround(new Point(newX, newY), zoom, useAnimation);
        }

        // zooms around a point
        public void ZoomAround(Point p, double zoom, bool animate)
        {
            ResetOffset();

            zoom = Math.Max(zoom, minZoom);
            zoom = Math.Min(zoom, maxZoom);

            this.z = zoom;

            double dxa = p.X * Map.ZoomAdjust / Map.LogicalSize * Map.ReferenceSize;
            double dya = -p.Y * Map.ZoomAdjust / Map.LogicalSize * Map.ReferenceSize;

            double logX = dxa + Map.ReferenceSize / 2;
            double logY = dya + Map.ReferenceSize / 2;
            Point pp = GeoCanvas.RenderTransform.Transform(new Point(logX, logY));

            logicalWheelOffsetTransform.X = -translateTransform.X - dxa;
            logicalWheelOffsetTransform.Y = -translateTransform.Y - dya;

            physicalWheelOffsetTransform.X = pp.X - ActualWidth / 2;
            physicalWheelOffsetTransform.Y = pp.Y - ActualHeight / 2;

            FireViewportBeginChanged();
            DoZoom(animate);
        }

        /// <summary>
        /// Positions the map to a center and zoom factor
        /// </summary>
        /// <param name="x">x-coordinate in logical units</param>
        /// <param name="y">y-coordinate in logical units</param>
        /// <param name="zoom">the zoom factor according to the standard tiling/zooming scheme.</param>       
        /// <param name="animate">Animates the pan transition</param>
        public void SetXYZ(double x, double y, double zoom, bool animate)
        {
            zoom = Math.Max(zoom, minZoom);
            zoom = Math.Min(zoom, maxZoom);

            // animating zoom and pan at the same time somehow looks strange
            // a special fly-to mode only uses the zoom animation with a speciial offset
            if (Math.Abs(zoom - CurrentZoomF) > .125)
            {
                FlyTo(new Point(x, y), zoom, animate);
            }
            else
            {
                ResetOffset();

                bool doPan = (this.x != x || this.y != y);
                bool doZoom = (this.z != zoom);

                this.x = x;
                this.y = y;
                this.z = zoom;

                FireViewportBeginChanged();

                if (doPan)
                    DoPan(animate);

                if (doZoom)
                    DoZoom(animate);
            }
        }

        private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (envMinX != 0 || envMinY != 0 || envMaxX != 0 || envMaxY != 0)
            {
                SetEnvelope(envMinX, envMinY, envMaxX, envMaxY);
                envMinX = envMinY = envMaxX = envMaxY = 0;
            }

            this.screenOffsetTransform.X = ActualWidth / 2;
            this.screenOffsetTransform.Y = ActualHeight / 2;

            this.Layers.Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };

            FireViewportBeginChanged();
            FireViewportWhileChanged();
            FireViewportEndChanged();
        }

        /// <summary>
        /// True if transitions to new map sections should be animatied
        /// </summary>
        public bool UseAnimation { get; set; }

        /// <summary>
        /// The level of detail according to the standard tiling scheme
        /// </summary>
        public int Zoom
        {
            get
            {
                return (int)Math.Round(ZoomF);
            }
            set
            {
                ZoomF = value;
            }
        }


        /// <summary>
        /// The level of detail according to the standard tiling scheme
        /// </summary>
        public double ZoomF
        {
            get
            {
                return z;
            }
            set
            {
                if (value > MaxZoom || value < MinZoom)
                    return;

                ResetOffset();

                z = value;

                FireViewportBeginChanged();
                DoZoom(UseAnimation);
            }
        }

        private static readonly DependencyProperty ScaleProperty =
                DependencyProperty.RegisterAttached("ZoomScale",
                typeof(double), typeof(Map), new PropertyMetadata(OnZoomScaleChanged));

        private static void OnZoomScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Map map = d as Map;
            double zoomScale = (double)e.NewValue;

            map.SetZoomScale(zoomScale);
        }

        private void SetZoomScale(double zoomScale)
        {
            this.zoomTransform.ScaleX = zoomScale;
            this.zoomTransform.ScaleY = zoomScale;

            FireViewportWhileChanged();
        }

        private static readonly DependencyProperty CenterProperty =
                DependencyProperty.RegisterAttached("Center",
                typeof(Point), typeof(Map), new PropertyMetadata(OnCenterChanged));

        private static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Map map = d as Map;
            Point center = (Point)e.NewValue;

            map.SetCenter(center);
        }

        private void SetCenter(Point center)
        {
            this.translateTransform.X = center.X;
            this.translateTransform.Y = center.Y;

            FireViewportWhileChanged();
        }

        public void DoPan(bool animate)
        {
            double tmpX = -x * ZoomAdjust * ReferenceSize / LogicalSize;
            double tmpY = y * ZoomAdjust * ReferenceSize / LogicalSize;

            animatePan = true;
            panAnimation.Duration = new TimeSpan(0, 0, 0, 0, animate ? 500 : 0);
            panAnimation.From = new Point(translateTransform.X, translateTransform.Y);
            panAnimation.To = new Point(tmpX, tmpY);

            panStoryboard.Begin();
        }

        public void DoZoom(bool animate)
        {
            double zoomScale = Math.Pow(2, z + 8) / ReferenceSize / ZoomAdjust;

            animateZoom = true;
            zoomAnimation.Duration = new TimeSpan(0, 0, 0, 0, animate ? 500 : 0);
            zoomAnimation.From = zoomTransform.ScaleX;
            zoomAnimation.To = zoomScale;

            zoomStoryboard.Begin();
        }

        public void FireViewportBeginChanged()
        {
            if (ViewportBeginChanged != null)
                ViewportBeginChanged(this, EventArgs.Empty);
        }

        public void FireViewportWhileChanged()
        {
            if (ViewportWhileChanged != null)
                ViewportWhileChanged(this, EventArgs.Empty);
        }

        public void FireViewportEndChanged()
        {
            if (ViewportEndChanged != null && !IsAnimating)
            {
                ViewportEndChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// returns true while the map performs a transition to a new map section
        /// </summary>
        public bool IsAnimating
        {
            get
            {
                return animatePan || animateZoom;
            }
        }

        /// <summary>
        /// The logical x-coordinate while the map is in motion
        /// </summary>
        public double CurrentX
        {
            get
            {
                return -(translateTransform.X + logicalWheelOffsetTransform.X) * LogicalSize / ReferenceSize / ZoomAdjust - physicalWheelOffsetTransform.X * CurrentScale;
            }
        }

        /// <summary>
        /// The logical y-coordinate while the map is in motion
        /// </summary>
        public double CurrentY
        {
            get
            {
                return (translateTransform.Y + logicalWheelOffsetTransform.Y) * LogicalSize / ReferenceSize / ZoomAdjust + physicalWheelOffsetTransform.Y * CurrentScale;
            }
        }

        /// <summary>
        /// The x-coordinate after the map was in motion / the anticipated x-coordinate while the map is in motion
        /// </summary>
        public double FinalX
        {
            get
            {
                return x - logicalWheelOffsetTransform.X * LogicalSize / ReferenceSize / ZoomAdjust - physicalWheelOffsetTransform.X * FinalScale;
            }
        }

        /// <summary>
        /// The y-coordinate after the map was in motion / the anticipated y-coordinate while the map is in motion
        /// </summary>
        public double FinalY
        {
            get
            {
                return y + logicalWheelOffsetTransform.Y * LogicalSize / ReferenceSize / ZoomAdjust + physicalWheelOffsetTransform.Y * FinalScale;
            }
        }

        /// <summary>
        /// The scale factor after the map was in motion / the anticipated scale factor while the map is in motion.
        /// Defined in Logical units per pixel
        /// </summary>
        public double FinalScale
        {
            get
            {
                return 1.0 / Math.Pow(2, z + 8) * LogicalSize;
            }
        }

        /// <summary>
        /// Calculates the floating tile level while in motion
        /// </summary>
        public double CurrentZoomF
        {
            get
            {
                return Math.Log(LogicalSize / CurrentScale, 2) - 8;
            }
        }

        /// <summary>
        /// The scale factor while the map is in motion.
        /// Defined in Logical units per pixel
        /// </summary>
        public double CurrentScale
        {
            get { return 1.0 / zoomTransform.ScaleX * LogicalSize / ReferenceSize / ZoomAdjust; }
        }

        protected void ResetOffset()
        {
            if (animatePan)
            {
                panAnimation.Duration = new TimeSpan(0, 0, 0, 0, 0);
                panAnimation.From = new Point(this.translateTransform.X, this.translateTransform.Y);
                panAnimation.To = new Point(this.translateTransform.X, this.translateTransform.Y);

                animatePan = false;
                panStoryboard.Begin();
                panStoryboard.SkipToFill();
            }

            this.x = CurrentX;
            this.y = CurrentY;

            this.translateTransform.X = -x * ZoomAdjust * ReferenceSize / LogicalSize;
            this.translateTransform.Y = y * ZoomAdjust * ReferenceSize / LogicalSize;

            logicalWheelOffsetTransform.X = 0;
            logicalWheelOffsetTransform.Y = 0;
            physicalWheelOffsetTransform.X = 0;
            physicalWheelOffsetTransform.Y = 0;
        }
    }
}
