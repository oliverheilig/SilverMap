//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Web;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Layers;
using Ptvag.Dawn.Controls.Map.MapMarket;

namespace Ptvag.Dawn.SilverMap.Web
{
    /// <summary>
    /// Renders thematic map tiles which display buying power for districts in germany
    /// It uses sharpmap http://sharpmap.codeplex.com/ in combination with the ptv sharpmap provider for map&market regions.
    /// You'll find the sources of the provider in the sister project WpfMap.
    /// Note: Using Jet (= MS Access) as data source is not recommended for a server scenario. Better use a "real" database
    /// (or even better a spatial data base). 
    /// Note2: To deploy the handler on a 64-Bit IIS using JET, the project must run in a 32-bit application pool.
    /// </summary>
    public class DistrictsTilesHandler : IHttpHandler
    {
        // store map as instance variable. It is used in custom theme to determine the map scale
        private Map sharpMap;

        public void ProcessRequest(HttpContext context)
        {
            int x, y, z;

            //Parse request parameters
            if (!int.TryParse(context.Request.Params["x"], out x))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["y"], out y))
                throw (new ArgumentException("Invalid parameter"));
            if (!int.TryParse(context.Request.Params["z"], out z))
                throw (new ArgumentException("Invalid parameter"));

            context.Response.ContentType = "image/png";
            string cacheKey = string.Format("DistrictTile{0}/{1}/{2}", x, y, z);
            byte[] buffer = context.Cache[cacheKey] as byte[];
            if (buffer != null)
            {
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                return;
            }

            // create a transparent sharpmap map with a size of 256x256
            using (sharpMap = new Map(new System.Drawing.Size(256, 256)) { BackColor = Color.Transparent })
            {
                // the map contains only one layer
                var districts = new VectorLayer("BuyingPowerDistricts")
                {
                    // CoordinateTransformation is not needed for map&market, regions are stored as PTV_Mercator
                    // districts.CoordinateTransformation = PTVMercatorProjection.TransformToMercator(GeographicCoordinateSystem.WGS84);

                    // set the sharpmap provider for map&market regions as data source
                    DataSource = new MMProvider(
                         @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DATADIRECTORY|\Districts.mdb",
                         "KRE", "GID", "XMIN", "YMIN", "XMAX", "YMAX", "WKB_GEOMETRY"),

                    // use a dynamic theme for thematic mapping
                    Theme = new SharpMap.Rendering.Thematics.CustomTheme(GetBuyingPowerStyle),
                };

                // add the layer to the map
                sharpMap.Layers.Add(districts);

                // calculate the bbox for the tile key and zoom the map 
                double xMin, yMin, xMax, yMax;
                TileToPtvMercatorAtZoom(x, y, z, out xMin, out yMin, out xMax, out yMax);
                sharpMap.ZoomToBox(new BoundingBox(
                    new SharpMap.Geometries.Point(xMin, yMin),
                    new SharpMap.Geometries.Point(xMax, yMax)));

                // generate the map image
                using (var img = (Bitmap)sharpMap.GetMap())
                {
                    //Stream the image to the client
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        // Saving a PNG image requires a seekable stream, first save to memory stream http://forums.asp.net/p/975883/3646110.aspx#1291641
                        img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        buffer = memoryStream.ToArray();

                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Cache[cacheKey] = buffer;
                    }
                }
            }
        }

        // demonstrates the use of dynamic styles (themes) for vector layers
        private SharpMap.Styles.VectorStyle GetBuyingPowerStyle(SharpMap.Data.FeatureDataRow row)
        {
            // set a linear gradient brush for each country according to the bounding pixel rectangle
            SharpMap.Geometries.BoundingBox bbox = row.Geometry.GetBoundingBox();
            var leftupper = new SharpMap.Geometries.Point(bbox.Left, bbox.Top);
            var rightbottom = new SharpMap.Geometries.Point(bbox.Right, bbox.Bottom);
            PointF luf = SharpMap.Utilities.Transform.WorldtoMap(leftupper, sharpMap);
            PointF rbf = SharpMap.Utilities.Transform.WorldtoMap(rightbottom, sharpMap);
            var p1 = new System.Drawing.Point((int)luf.X, (int)luf.Y);
            var p2 = new System.Drawing.Point((int)rbf.X, (int)rbf.Y);

            // colorize the polygon according to buying power; map 7->0.0 and 1->1.0
            float scale = (7.0f - System.Convert.ToSingle(row["KK_KAT"], NumberFormatInfo.InvariantInfo)) / 6.0f;
            Color c = SharpMap.Rendering.Thematics.ColorBlend.Rainbow7.GetColor(scale);
//            c = Color.FromArgb(180, c.R, c.G, c.B);

            // set the border width depending on the map scale
            Pen pen = new Pen(Brushes.Black, (int)(50.0 / sharpMap.PixelSize)) { LineJoin = LineJoin.Round };
       
            var style = new SharpMap.Styles.VectorStyle {Outline = pen, EnableOutline = true};

            if (p1 != p2) // will throw overflow exception for p1 == p2
                style.Fill = new LinearGradientBrush(p1, p2, Color.FromArgb(255, 255, 255, 255), c);

            return style;
        }

        // calculates a ptv mercator bounding box for a tile key
        protected void TileToPtvMercatorAtZoom(
             int tileX, int tileY, int zoom,
             out double xMin, out double yMin, out double xMax, out double yMax)
        {
            double earthRadius = 6371000.0;
            double earthCircum = earthRadius * 2.0 * Math.PI;
            double earthHalfCircum = earthCircum / 2;

            double arc = earthCircum / (1 << zoom);

            xMin = (tileX * arc) - earthHalfCircum;
            xMax = ((tileX + 1) * arc) - earthHalfCircum;

            yMin = earthHalfCircum - ((tileY + 1) * arc);
            yMax = earthHalfCircum - (tileY * arc);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}