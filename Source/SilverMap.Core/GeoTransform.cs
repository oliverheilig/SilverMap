//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public class GeoTransform
    {
        /// <summary>
        /// converts a wgs84 point (lon, lat) to a ptv mercator point
        /// </summary>
        /// <param name="point">the wgs84 point</param>
        /// <returns>the mercator point</returns>
        public static Point WGSToPtvMercator(Point point)
        {
            double x = 6371000.0 * point.X * Math.PI / 180;
            double y = 6371000.0 * Math.Log(Math.Tan((Math.PI / 4) + (Math.PI / 360) * point.Y));

            return new System.Windows.Point(x, y);
        }

        /// <summary>
        /// converts ptv mercator point to a wgs84 point (lon, lat) 
        /// </summary>
        /// <param name="point">the mercator point</param>   
        /// <returns>the wgs84 point</returns>
        public static Point PtvMercatorToWGS(Point point)
        {
            double x = (180 / Math.PI) * (point.X / 6371000.0);
            double y = (360 / Math.PI) * (Math.Atan(Math.Exp(point.Y / 6371000.0)) - (Math.PI / 4));

            return new Point(x, y);
        }

        /// <summary>
        /// formats a lat/lon coordinate to a display string
        /// </summary>
        /// <param name="lat">the latitude</param>
        /// <param name="lon">the longitude</param>
        /// <returns>the display string</returns>
        public static string LatLonToString(double lat, double lon)
        {
            bool latIsNeg = lat < 0;
            lat = Math.Abs(lat);
            int degLat = (int)(lat);
            int minLat = (int)((lat - degLat) * 60);
            double secLat = (lat - degLat - (double)minLat / 60) * 3600;
     
            bool lonIsNeg = lon < 0;
            lon = Math.Abs(lon);
            int degLon = (int)(lon);
            int minLon = (int)((lon - degLon) * 60);
            double secLon = (lon - degLon - (double)minLon / 60) * 3600;

            return string.Format("{0}° {1:00}′ {2:00}″ {3}, {4}° {5:00}′ {6:00}″ {7}",
                degLat, minLat, Math.Floor(secLat), latIsNeg ? Resources.Strings.South : Resources.Strings.North,
                degLon, minLon, Math.Floor(secLon), lonIsNeg ? Resources.Strings.West : Resources.Strings.East);
        }

        /// <summary>
        /// returns a wgs rectangle for a given tile key
        /// </summary>
        /// <param name="tileX">x-key</param>
        /// <param name="tileY">y-key</param>
        /// <param name="zoom">tile level</param>
        /// <returns></returns>
        public static Rect TileToWgsAtZoom(int tileX, int tileY, int zoom)
        {
            double arc = Math.PI / Math.Pow(2, zoom - 1);

            double xMin = (tileX * arc) - Math.PI;
            double yMax = Math.PI - (tileY * arc);

            double xMax = xMin + arc;
            double yMin = yMax - arc;

            xMin = (180 / Math.PI) * xMin;
            yMax = (180 / Math.PI) * (Math.Atan(Math.Exp(yMax)) - (Math.PI / 4)) / 0.5;
            xMax = (180 / Math.PI) * xMax;
            yMin = (180 / Math.PI) * (Math.Atan(Math.Exp(yMin)) - (Math.PI / 4)) / 0.5;

            return new Rect(new Point(xMin, yMin), new Point(xMax, yMax));
        }

        // calculates a ptv mercator bounding box for a tile key
        public static Rect TileToPtvMercatorAtZoom(int tileX, int tileY, int zoom)
        {
            double earthRadius = 6371000.0;
            double earthCircum = earthRadius * 2.0 * Math.PI;
            double earthHalfCircum = earthCircum / 2;

            double arc = earthCircum / (1 << zoom);

            double xMin = (tileX * arc) - earthHalfCircum;
            double xMax = ((tileX + 1) * arc) - earthHalfCircum;

            double yMin = earthHalfCircum - ((tileY + 1) * arc);
            double yMax = earthHalfCircum - (tileY * arc);

            return new Rect(new Point(xMin, yMin), new Point(xMax, yMax));
        }
    }
}