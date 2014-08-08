//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public interface IOverlayProvider
    {
        Uri GetUri(double minX, double maxX, double minY, double maxY, double width, double height);
    }

    public class OverlayLayer : Canvas
    {
        private Timer timer;
        protected Map Map;

        // max map request size
        public int MaxSize = 2048;

        // delay for updating the overlays
        int updateDelay = 150;

        // maximum bbox for PTV_Mercator
        int envMinX = -20000000;
        int envMaxX = 20000000;
        int envMinY = -20000000;
        int envMaxY = 20000000;

        public IOverlayProvider OverlayProvider { get; set; }

        public OverlayLayer()
        {
            this.Loaded += new RoutedEventHandler(OverlayLayer_Loaded);
        }

        void OverlayLayer_Loaded(object sender, RoutedEventArgs e)
        {
            if (Map != null)
                return;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            Map = MapElementExtensions.FindParent<Map>(this);

            if (Map == null)
                return;

            IsHitTestVisible = false;
            RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercatorInvertedY);
            Canvas.SetZIndex(this, 64);

            Map.ViewportBeginChanged += new EventHandler(map_ViewportBeginChanged);
            Map.ViewportWhileChanged += new EventHandler(map_ViewportWhileChanged);

            UpdateOverlay();
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            foreach (Image image in this.Children)
            {
                var tag = (ImageTag) image.Tag;
                // fade out for big scale differences
                image.Opacity = 1.0 - Math.Min(1, .25 * Math.Abs(Map.CurrentZoomF - tag.Zoom));            
            }
        }

        void map_ViewportBeginChanged(object sender, EventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            if (updateDelay == 0)
                UpdateOverlay();
            else
                timer = new Timer(InvokeUpdate, null, updateDelay, Timeout.Infinite);
        }

        public void Remove()
        {
            Map.ViewportBeginChanged -= new EventHandler(map_ViewportBeginChanged);
            Map.ViewportWhileChanged -= new EventHandler(map_ViewportWhileChanged);

            Map.GeoCanvas.Children.Remove(this);
        }

        public void InvokeUpdate(Object stateInfo)
        {
            this.Dispatcher.BeginInvoke(new Action(UpdateOverlay));
        }

        double[] lastParams;
        public void UpdateOverlay()
        {
            //if (!force && lastImageTag != null)
            //    return;

            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            double minX, minY, maxX, maxY;
            Map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double width = Map.ActualWidth;
            double height = Map.ActualHeight;

            // clip the rectangle to the maximum rectangle
            if (minX < envMinX || maxX > envMaxX || minY < envMinY || maxY > envMaxY)
            {
                double leftClipped = (minX < envMinX) ? envMinX : minX;
                double rightClipped = (maxX > envMaxX) ? envMaxX : maxX;
                double topClipped = (minY < envMinY) ? envMinY : minY;
                double bottomClipped = (maxY > envMaxY) ? envMaxY : maxY;

                width = width * (rightClipped - leftClipped) / (maxX - minX);
                height = height * (bottomClipped - topClipped) / (maxY - minY);

                minX = leftClipped;
                maxX = rightClipped;
                minY = topClipped;
                maxY = bottomClipped;
            }

            double ratio = 1.0;
            if (width > MaxSize || height > MaxSize)
            {
                if (height > width)
                    ratio = MaxSize / height;
                else
                    ratio = MaxSize / width;

                width = width * ratio;
                height = height * ratio;
            } 
            
            if (minX == maxX || minY == maxY)
                return;

            if (width < 32 || height < 32)
                return;

            double[] tmpParams = new double[] { minX, maxX, minY, maxY, width, height };
            if (lastParams != null && tmpParams[0] == lastParams[0] && tmpParams[1] == lastParams[1] && tmpParams[2] == lastParams[2] && 
                tmpParams[3] == lastParams[3] && tmpParams[4] == lastParams[4] && tmpParams[5] == lastParams[5])
                return;
            else
                lastParams = tmpParams;

            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = OverlayProvider.GetUri(minX, maxX, minY, maxY, width, height);

            Image image = new Image();
            image.Tag = new ImageTag {Time = DateTime.Now, Zoom = Map.ZoomF};
            lastImageTag = image.Tag;
            this.Children.Add(image);

            image.Width = maxX - minX;
            image.Height = maxY - minY;
            Canvas.SetLeft(image, minX);
            Canvas.SetTop(image, -maxY);

            image.ImageOpened += new EventHandler<RoutedEventArgs>(image_ImageOpened);
            image.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(image_ImageFailed);
            image.Source = bmp;
        }

        void image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.Children.Remove(sender as Image);
        }

        object lastImageTag;
        private Image lastDisplayedImage;
        void image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;

            if (lastImageTag == image.Tag)
                lastImageTag = null;

            var tag = (ImageTag)image.Tag;

            // dispose obsolete images
            var oldImages = (from Image tmpImage in Children
                             let tmpTime = ((ImageTag)tmpImage.Tag).Time
                             where tmpTime < tag.Time select tmpImage).Cast<UIElement>().ToList();

            foreach (Image tmpImage in oldImages)
            {
                Children.Remove(tmpImage);
                tmpImage.ImageOpened -= new EventHandler<RoutedEventArgs>(image_ImageOpened);
                tmpImage.ImageFailed -= new EventHandler<ExceptionRoutedEventArgs>(image_ImageFailed);
            }
        }
    }

    public struct ImageTag
    {
        public DateTime Time { get; set; }
        public double Zoom { get; set; }
    }
}
