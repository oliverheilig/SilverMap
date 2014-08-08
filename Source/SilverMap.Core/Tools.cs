//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public static class RectExtensions
    {
        public static Rect Inflate(this Rect rect, double factor)
        {
            double dx = rect.Width / 2 * (factor - 1);
            double dy = rect.Height / 2 * (factor - 1);

            return new Rect(new Point(rect.Left - dx, rect.Top - dy), new Point(rect.Right + dx, rect.Bottom + dy));
        }

        public static Rect CreateEnvelope(IEnumerable<Point> points)
        {
            // calucate bounds for points
            return (from point in points
                    select new Rect(
                        new Point(points.Min(p => p.X), points.Min(p => p.Y)),
                        new Point(points.Max(p => p.X), points.Max(p => p.Y)))).First();
        }
    }

    public static class WgsExtensions
    {
        public static Point CanvasToPtvMercator(this Map map, UIElement canvas, Point point)
        {
            Point geoCanvasPoint = canvas.TransformToVisual(map.GeoCanvas).Transform(point);

            return new Point(
               (geoCanvasPoint.X / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize) - 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2,
               -(geoCanvasPoint.Y / Map.ZoomAdjust * Map.LogicalSize / Map.ReferenceSize) + 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2);
        }

        public static Point CanvasToWgs(this Map map, UIElement canvas, Point point)
        {
            Point mercatorPoint = CanvasToPtvMercator(map, canvas, point);

            return GeoTransform.PtvMercatorToWGS(mercatorPoint);
        }

        public static Point PtvMercatorToCanvas(this Map map, UIElement canvas, Point mercatorPoint)
        {
            Point geoCanvasPoint = new Point(
               (mercatorPoint.X + 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2) * Map.ZoomAdjust / Map.LogicalSize * Map.ReferenceSize,
               (-mercatorPoint.Y + 1.0 / Map.ZoomAdjust * Map.LogicalSize / 2) * Map.ZoomAdjust / Map.LogicalSize * Map.ReferenceSize);

            return map.GeoCanvas.TransformToVisual(canvas).Transform(geoCanvasPoint);
        }

        public static Point WgsToCanvas(this Map map, UIElement canvas, Point wgsPoint)
        {
            Point mercatorPoint = GeoTransform.WGSToPtvMercator(wgsPoint);

            return PtvMercatorToCanvas(map, canvas, mercatorPoint);
        }

        public static void SetLatLonZ(this Map map, double latitude, double longitude, double z)
        {
            Point mercatorPoint = GeoTransform.WGSToPtvMercator(new Point(longitude, latitude));

            map.SetXYZ(mercatorPoint.X, mercatorPoint.Y, z);
        }

        public static void SetEnvelopeLatLon(this Map map, double minX, double minY, double maxX, double maxY, double factor)
        {
            // transform to map coordinates
            System.Windows.Point p1 = GeoTransform.WGSToPtvMercator(new Point(minX, minY));
            System.Windows.Point p2 = GeoTransform.WGSToPtvMercator(new Point(maxX, maxY));

            Rect rect = new Rect(p1, p2).Inflate(factor);

            // set envelope
            map.SetEnvelope(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static Rect GetCurrentEnvelopePtvMercator(this Map map)
        {
            double minX, minY, maxX, maxY;

            map.GetCurrentEnvelope(out minX, out minY, out maxX, out maxY);

            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        public static Rect GetCurrentEnvelopeLatLon(this Map map)
        {
            double minX, minY, maxX, maxY;

            map.GetCurrentEnvelope(out minX, out minY, out maxX, out maxY);

            Point p1 = new Point(minX, minY);
            Point p2 = new Point(maxX, maxY);

            return new Rect(GeoTransform.PtvMercatorToWGS(p1), GeoTransform.PtvMercatorToWGS(p2));
        }

        public static Rect GetFinalEnvelopeLatLon(this Map map)
        {
            double minX, minY, maxX, maxY;

            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);

            Point p1 = new Point(minX, minY);
            Point p2 = new Point(maxX, maxY);

            return new Rect(GeoTransform.PtvMercatorToWGS(p1), GeoTransform.PtvMercatorToWGS(p2));
        }
    }

    public static class MapElementExtensions
    {
        public static T FindRelative<T>(this FrameworkElement fe) where T : DependencyObject
        {
            if (fe.Parent is T)
                return fe.Parent as T;
            else
            {
                T child = FindVisualChild<T>(fe.Parent);
                if (child != null)
                    return child;
                else  if (fe.Parent is FrameworkElement)
                    return FindRelative<T>(fe.Parent as FrameworkElement);
            }

            return null;
        }

        public static T FindParent<T>(FrameworkElement fe) where T : DependencyObject
        {
            if (fe.Parent is T)
                return fe.Parent as T;
            else if (fe.Parent is FrameworkElement)
                return FindParent<T>(fe.Parent as FrameworkElement);
            else
                return null;
        }

        public static bool IsControlVisible(FrameworkElement element)
        {
            while (element != null)
            {
                Visibility visibility = (Visibility)element.GetValue(FrameworkElement.VisibilityProperty);

                if (visibility == Visibility.Collapsed)
                    return false;

                element = element.Parent as FrameworkElement;
            }

            return true;
        }

        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
