//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.IO;
using Ptvag.Dawn.SilverMap.Web.XmapService;
using System.Net;
using System.Collections.Generic;

namespace Ptvag.Dawn.SilverMap.Web
{
    /// <summary>
    /// XmapRenderer renders an xMapServer-Bitmap 
    /// This implementation fixes several problems which occur in combination with tiling and silverlight
    /// + it clips the request rectangles to avoid problems on the southern hemisphere
    /// + it internally resizes the image to avoid artifacts on tile borders
    /// + it makes the labels transparent
    /// + it converts gif to png
    /// </summary>
    public class XmapRenderer
    {
        int minX = -20000000;
        int maxX = 20000000;
        int minY = -10000000;
        int maxY = 20000000;

        private string url = string.Empty;
        private MapMode mapMode;

        public XmapRenderer(string url, MapMode mapMode)
        {
            // test of clipping
            //minX = 937117 -1000000;
            //maxX = 937117 + 1000000;
            //minY = 6270145 -1000000;
            //maxY = 6270145 + 1000000;

            this.mapMode = mapMode;
            this.url = url;
        }

        public Stream GetStream(int left, int top, int right, int bottom, int width, int height, ImageFileFormat format)
        {
            int trials = 0;

            while (true)
            {
                try
                {
                    return TryGetStream(left, top, right, bottom, width, height, format);
                }
                catch (WebException exception)
                {
                    // retry for 500 and 503
                    var result = (HttpWebResponse)exception.Response;
                    if (result.StatusCode == HttpStatusCode.InternalServerError ||
                        result.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        if (++trials < 3)
                        {
                            System.Threading.Thread.Sleep(50);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        public Stream TryGetStream(int left, int top, int right, int bottom, int width, int height, ImageFileFormat format)
        {
            var boundingBox = new BoundingBox
            {
                leftTop = new Point { point = new PlainPoint { x = left, y = top } },
                rightBottom = new Point { point = new PlainPoint { x = right, y = bottom } }
            };

            var mapParams = new MapParams
            {
                showScale = false,
                useMiles = false
            };

            var imageInfo = new ImageInfo { format = format, height = height, width = width };

            string profile = string.Empty;
            var layers = new List<Layer>();
            switch (mapMode)
            {
                // only streets
                case MapMode.Street:
                    {
                        layers.Add(new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 });
                        layers.Add(new StaticPoiLayer { name = "background", visible = false, category = -1, detailLevel = 0 });

                        profile = "ajax-bg";

                        break;
                    }
                // only labels
                case MapMode.Town:
                    {
                        profile = "ajax-fg";

                        break;
                    }
                // nothing
                case MapMode.Custom:
                    {
                        profile = "ajax-fg";
                        layers.Add(new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 });
                        layers.Add(new StaticPoiLayer { name = "street", visible = false, category = -1, detailLevel = 0 });
                        layers.Add(new StaticPoiLayer { name = "background", visible = false, category = -1, detailLevel = 0 });

                        break;
                    }
                // only background
                case MapMode.Background:
                    {
                        layers.Add(new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 });
                        profile = "ajax-bg";

                        break;
                    }
            }

            //// add custom xmap layrs
            //if (CustomXmapLayers != null)
            //    layers.AddRange(CustomXmapLayers);

            var callerContext = new CallerContext
            {
                wrappedProperties = new CallerContextProperty[]{
                        new CallerContextProperty{key = "CoordFormat", value="PTV_MERCATOR"},
                        new CallerContextProperty{key = "Profile", value = profile}
                    }
            };

            var client = new XMapWSClient("XMapWSPort", this.url);
            ((XMapWSClient)client).ClientCredentials.UserName.UserName = "xtok";
            ((XMapWSClient)client).ClientCredentials.UserName.Password = Ptvag.Dawn.SilverMap.Web.Properties.Settings.Default.Token; 
            var map = client.renderMapBoundingBox(boundingBox, mapParams, imageInfo, layers.ToArray(), true, callerContext);

            return new MemoryStream(map.image.rawImage);
        }

        private MemoryStream SaveAndConvert(System.Drawing.Bitmap image)
        {
            // make background transparent for overlays
            if (mapMode != MapMode.Background)
                image.MakeTransparent(System.Drawing.Color.FromArgb(255, 254, 185));

            // image has to be converted to png. Silverlight doesn't support gif
            // Saving a PNG image requires a seekable memory stream http://forums.asp.net/p/975883/3646110.aspx#1291641
            var ms = new System.IO.MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            return ms;
        }

        public MemoryStream GetImage(int left, int top, int right, int bottom, int width, int height, int border)
        {
            if (left < minX || right > maxX || top < minY || bottom > maxY || border > 0)
            {
                // request must be resized or clipped
                double leftResized, rightResized, topResized, bottomResized;

                // calculate resized bounds depending on border
                // the resize factor internally resizes requested tiles to avoid clipping problems
                if (border > 0)
                {
                    double resize = (double)border / width;
                    double lWidth = (right - left) * resize;
                    double lHeight = (bottom - top) * resize;

                    leftResized = (left - lWidth);
                    rightResized = (right + lWidth);
                    topResized = (top - lHeight);
                    bottomResized = (bottom + lHeight);
                }
                else
                {
                    leftResized = left;
                    rightResized = right;
                    topResized = top;
                    bottomResized = bottom;
                }

                // calculate clipped bounds
                double leftClipped = (leftResized < minX) ? minX : leftResized;
                double rightClipped = (rightResized > maxX) ? maxX : rightResized;
                double topClipped = (topResized < minY) ? minY : topResized;
                double bottomClipped = (bottomResized > maxY) ? maxY : bottomResized;

                // calculate corresponding pixel width and height 
                double rWidth = width * (rightClipped - leftClipped) / (right - left);
                double rHeight = height * (bottomClipped - topClipped) / (bottom - top);

                if (rWidth < 32 || rHeight < 32)
                {
                    // resulting image will be too small -> return empty image
                    using (var bmp = new System.Drawing.Bitmap(width, height))
                    {
                        return SaveAndConvert(bmp);
                    }
                }
                else using (System.IO.Stream stream = GetStream(
                    (int)Math.Round(leftClipped), (int)Math.Round(topClipped), (int)Math.Round(rightClipped), (int)Math.Round(bottomClipped),
                    (int)Math.Round(rWidth), (int)Math.Round(rHeight), ImageFileFormat.GIF))
                    {
                        // paste resized/clipped image on new image
                        using (var img = System.Drawing.Image.FromStream(stream))
                        {
                            using (var bmp = new System.Drawing.Bitmap(width, height))
                            {
                                using (var g = System.Drawing.Graphics.FromImage(bmp))
                                {
                                    double offsetX = (leftClipped - left) / (right - left) * width;
                                    double offsetY = (bottomClipped - bottom) / (top - bottom) * height;

                                    g.DrawImageUnscaled(img, (int)Math.Round(offsetX), (int)Math.Round(offsetY));
                                }

                                return SaveAndConvert(bmp);
                            }
                        }
                    }
            }
            else using (var stream = GetStream(left, top, right, bottom, width, height, ImageFileFormat.GIF))
                {
                    using (var img = System.Drawing.Image.FromStream(stream) as System.Drawing.Bitmap)
                    {
                        return SaveAndConvert(img);
                    }
                }

            //    else
            //    {
            //        // no resizing, clipping, transparency - can return png image directly 
            //        // (maybe not a good idea since xMap doesn't like png too much)
            //        return GetStream(left, top, right, bottom, width, height, ImageFileFormat.PNG);
            //    }
        }
    }

    public enum MapMode
    {
        Background,
        Town,
        Street,
        Custom
    }
}
