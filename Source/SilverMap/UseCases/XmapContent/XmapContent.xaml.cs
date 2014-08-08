//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ptvag.Dawn.Controls.SilverMap.Core;
using SilverMap.XmapService;

namespace SilverMap.UseCases.XmapContent
{
    /// <summary>
    /// This sample demonstrates the use of xMapServer as a pure overlay server
    /// It also shows how to avoid the crossdomain-problem of silverlight by
    /// accessing a proxy xMapServer at the ASP.NET project, see XServerProxy.aspx.
    /// The sample works similar to the label overlay, but it also uses the ObjectInformation
    /// from the xMap response to build tooltip-information.
    /// </summary>
    public partial class XmapContent : Canvas
    {
        private Ptvag.Dawn.Controls.SilverMap.Core.Map map;
        private Timer timer;

        // delay for updating the overlays
        int updateDelay = 250;

        // maximum bbox for PTV_Mercator
        int envMinX = -20000000;
        int envMaxX = 20000000;
        int envMinY = -10000000;
        int envMaxY = 20000000;

        XMapWS xMapWSClient;

        public XmapContent(Ptvag.Dawn.Controls.SilverMap.Core.Map map)
        {
            InitializeComponent();

            this.map = map;

            RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercatorInvertedY);
            Canvas.SetZIndex(this, 100);
            map.GeoCanvas.Children.Add(this);

            map.ViewportBeginChanged += map_ViewportBeginChanged;

   //         xMapWSClient = new XMapWSClient();
            xMapWSClient = new XMapWSClient(new BasicHttpBinding { MaxReceivedMessageSize = 2147483647 }, 
                new EndpointAddress(App.BaseUrl + "/XServerProxy.ashx?type=xmap"));

            map.SetLatLonZ(52.5, 13.4, 14);

            UpdateOverlay();
        }

        public void Remove()
        {
            map.ViewportBeginChanged -= map_ViewportBeginChanged;

            map.GeoCanvas.Children.Remove(this);
        }

        public void TimerElapsed(Object stateInfo)
        {
            this.Dispatcher.BeginInvoke(UpdateOverlay);
        }

        void map_ViewportBeginChanged(object sender, EventArgs e)
        {
            if (updateDelay == 0)
                UpdateOverlay();
            else if (timer == null)
            {
                timer = new Timer(TimerElapsed);
                timer.Change(updateDelay, 0);
            }
        }

        public void UpdateOverlay()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            double minX, minY, maxX, maxY;
            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double width = map.ActualWidth;
            double height = map.ActualHeight;

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

            if (minX == maxX || minY == maxY)
                return;

            if (width < 32 || height < 32)
                return;

            // this request returns an empty map with a single ptv-traffic layer
            var request = new renderMapBoundingBoxRequest
            {
                BoundingBox_1 = new BoundingBox
                {
                    leftTop = new Point { point = new PlainPoint { x = minX, y = maxY } },
                    rightBottom = new Point { point = new PlainPoint { x = maxX, y = minY } }
                },
                MapParams_2 = new MapParams
                {
                    showScale = false,
                    useMiles = false
                },
                ImageInfo_3 = new ImageInfo { format = ImageFileFormat.PNG, height = (int)height, width = (int)width },
                ArrayOfLayer_4 = new Layer[]{
                      new RoadEditorLayer{name="truckattributes", visible=true, objectInfos=ObjectInfoType.GEOMETRYCLIPPED},
                      // disable all base layers
                      new StaticPoiLayer { name = "town", visible = false, category = -1, detailLevel = 0 },
                      new StaticPoiLayer { name = "street", visible = true, category = -1, detailLevel = 0 },
                      new StaticPoiLayer { name = "background", visible = false, category = -1, detailLevel = 0 },
                },
                boolean_5 = true,
                CallerContext_6 = new CallerContext
                {
                    wrappedProperties = new CallerContextProperty[]{
                        new CallerContextProperty{key = "CoordFormat", value="PTV_MERCATOR"},
                        new CallerContextProperty{key = "Profile", value="truckattributes"}
                }}
            };

            lastStamp = DateTime.Now;
            using (var scope = new OperationContextScope(((XMapWSClient)xMapWSClient).InnerChannel))
            {
                var prop = new HttpRequestMessageProperty();
                prop.Headers["Authorization"] = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("xtok:" + <insert your token here>));

                OperationContext context = OperationContext.Current;
                context.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = prop;

                xMapWSClient.BeginrenderMapBoundingBox(request, new AsyncCallback(Invoke), new ImageParams
            {
                TimeStamp = lastStamp,
                minX = minX,
                minY = minY,
                maxX = maxX,
                maxY = maxY,
                Width = width,
                Height = height
            });
            }
        }
        DateTime lastStamp;

        public void Invoke(IAsyncResult result)
        {
            var imageParams = result.AsyncState as ImageParams;

            var response = xMapWSClient.EndrenderMapBoundingBox(result);

            Dispatcher.BeginInvoke(new Action<renderMapBoundingBoxResponse, ImageParams>(AddImage), response, imageParams);
        }

        public void AddImage(renderMapBoundingBoxResponse response, ImageParams imageParams)
        {
            if (imageParams.TimeStamp < lastStamp)
                return;

            var bmp = new BitmapImage();

            bmp.SetSource(new MemoryStream(response.result.image.rawImage));
            var image = new System.Windows.Controls.Image { Width = imageParams.maxX - imageParams.minX, Height = imageParams.maxY - imageParams.minY };

            Canvas.SetLeft(image, imageParams.minX);
            Canvas.SetTop(image, -imageParams.maxY);
            image.Source = MakeTransparent(bmp);
            image.IsHitTestVisible = false;

            this.Children.Clear();
            this.Children.Add(image);

            // this code adds tool tip information by adding 'invisible' wpf-elements
            // with a tool tip attached
            foreach (ObjectInfos objectInfos in response.result.wrappedObjects)
            {

                foreach (LayerObject layerObject in objectInfos.wrappedObjects)
                {
                    double x = layerObject.@ref.point.x;
                    double y = layerObject.@ref.point.y;
                    string descr = layerObject.descr;//.Split('#')[1];

                    var toolTip = new ToolTip
                                      {
                                          Template = Resources["ToolTipTemplate"] as ControlTemplate,
                                          Content = new TextBlock
                                                        {
                                                            Text = descr,
                                                            FontFamily = new FontFamily("Georgia"),
                                                            FontSize = 14,
                                                            TextWrapping = System.Windows.TextWrapping.Wrap
                                                        }
                                      };

                    // add tool tip for reference point
                    var rect = new Rectangle
                                   {
                                       Width = 28 * map.FinalScale,
                                       Height = 28 * map.FinalScale,
                                       Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255))
                                   };

                    Canvas.SetZIndex(rect, 10);
                    Canvas.SetLeft(rect, x - rect.Width * 1.2);
                    Canvas.SetTop(rect, -y - rect.Height * 1.2);
                    ToolTipService.SetToolTip(rect, toolTip);
                    this.Children.Add(rect);

                    // add tool tip for line string if geometry is returned
                    if (layerObject.geometry != null && layerObject.geometry.pixelGeometry is PlainLineString)
                    {
                        var lineString = layerObject.geometry.pixelGeometry as PlainLineString;

                        var polyLine = new Polyline
                                           {
                                               Stroke = new SolidColorBrush(Colors.Transparent),
                                               StrokeLineJoin = PenLineJoin.Round,
                                               StrokeStartLineCap = PenLineCap.Round,
                                               StrokeEndLineCap = PenLineCap.Round,
                                               StrokeThickness = 16 * map.FinalScale
                                           };

                        foreach (var plainPoint in lineString.wrappedPoints)
                        {
                            if (plainPoint.x == 0 && plainPoint.y == 0) // xMap returns some (0,0) at the end - fixed with 1.14
                                continue;

                            var pixelPoint = new System.Windows.Point(plainPoint.x, plainPoint.y); // convert to wpf point
                            var mercatorPoint = map.CanvasToPtvMercator(map.Layers, pixelPoint); // convert to mercator
                            polyLine.Points.Add(new System.Windows.Point(mercatorPoint.X, -mercatorPoint.Y)); // add to polyLine, invert y
                        }

                        this.Children.Add(polyLine);
                        ToolTipService.SetToolTip(polyLine, toolTip);
                    }
                }
            }
        }

        // Uses the WriteableBitmap to make the image transparent
        WriteableBitmap MakeTransparent(BitmapImage img)
        {
            byte[] alphakey = new byte[] { 185, 254, 255 }; // xMap bg color
            var bitmap = new WriteableBitmap(img);
            int pixelLocation = 0;

            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    int pixel = bitmap.Pixels[pixelLocation];

                    byte[] pixelBytes = BitConverter.GetBytes(pixel);
                    if (pixelBytes[0] == alphakey[0] && pixelBytes[1] == alphakey[1] && pixelBytes[2] == alphakey[2])
                    {
                        bitmap.Pixels[pixelLocation] = 0;
                    }

                    pixelLocation++;
                }
            }

            return bitmap;
        }

        public class ImageParams
        {
            public double minX { get; set; }
            public double minY { get; set; }
            public double maxX { get; set; }
            public double maxY { get; set; }

            public double Width { get; set; }
            public double Height { get; set; }

            public DateTime TimeStamp { get; set; }
        }
    }
}

