//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Ptvag.Dawn.Controls.SilverMap.Core;
using Ptvag.Dawn.Controls.SilverMap.Core.Gadgets;
using Ptvag.Dawn.Controls.SilverMap.Core.Layers;
using Ptvag.Dawn.Controls.SilverMap.Core.Overlays;
using Ptvag.Dawn.Controls.SilverMap.Core.TileSources;

namespace SilverMap
{
    public partial class MapPage : UserControl
    {
        // add your bing maps key here
        private static string bingMapsKey = "";
        
        // add your nokia appId / tokenhere
        private static string nokiaAppId = "";
        private static string nokiaToken = "";

        private double panOffset = .25;

        private Map map
        {
            get
            {
                if (mapControl == null)
                    return null;
                else
                    return mapControl.Map;
            }
        }

        public MapPage()
        {
            InitializeComponent();

            SetBaseMapProvider(0);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // set pyramid as center
            map.SetLatLonZ(49.00814, 8.4037, 14);

            // trigger a use case by request parameter
            try
            {
                if (System.Windows.Browser.HtmlPage.Document.QueryString.ContainsKey("uc"))
                {
                    switch (System.Windows.Browser.HtmlPage.Document.QueryString["uc"])
                    {
                        case "locating":
                            checkBox6.IsChecked = true;
                            break;
                        case "routing":
                            checkBox7.IsChecked = true;
                            break;
                        case "xmap":
                            checkBox8.IsChecked = true;
                            break;
                        case "mapmarket":
                            checkBox9.IsChecked = true;
                            break;
                        case "wms":
                            checkBox10.IsChecked = true;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                // how to get OOB-mode?
            }

            buttonAbout.NavigateUri = new Uri(App.BaseUrl + "/XmapNetHandsOn.pdf");
        }

        #region navigation buttons

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            map.Zoom++;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            map.Zoom--;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            double minX, minY, maxX, maxY;

            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double dY = (maxY - minY) * panOffset;

            map.SetXYZ(map.FinalX, map.FinalY + dY, map.ZoomF, map.UseAnimation);
        }

        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            double minX, minY, maxX, maxY;

            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double dX = (maxX - minX) * panOffset;
 
            map.SetXYZ(map.FinalX - dX, map.FinalY, map.ZoomF, map.UseAnimation);
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
            double minX, minY, maxX, maxY;

            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double dX = (maxX - minX) * panOffset;

            map.SetXYZ(map.FinalX + dX, map.FinalY, map.ZoomF, map.UseAnimation);
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            double minX, minY, maxX, maxY;

            map.GetFinalEnvelope(out minX, out minY, out maxX, out maxY);
            double dY = (maxY - minY) * panOffset;

            map.SetXYZ(map.FinalX, map.FinalY - dY, map.ZoomF, map.UseAnimation);
        }

        #endregion

        #region projection, animation, fullscreen, themes

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            var pp = new PlaneProjection {CenterOfRotationY = 0};
            map.Layers.Projection = pp;
            var zoomStoryboard = new Storyboard();
            var zoomAnimation = new DoubleAnimation();

            zoomAnimation.EasingFunction = new QuinticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseInOut };
            zoomStoryboard.Children.Add(zoomAnimation);
            Storyboard.SetTargetProperty(zoomAnimation, new PropertyPath("(PlaneProjection.RotationX)"));
            Storyboard.SetTarget(zoomAnimation, pp);
            zoomAnimation.Duration = new TimeSpan(0, 0, 0, 0, 500);
            zoomAnimation.To = -45;

            // fire map viewport events manually
            zoomAnimation.Completed += new EventHandler(zoomAnimation_Completed);
            map.FireViewportBeginChanged();

            zoomStoryboard.Begin();
        }

        void zoomAnimation_Completed(object sender, EventArgs e)
        {
            map.FireViewportWhileChanged();
            map.FireViewportEndChanged();
            (sender as DoubleAnimation).Completed -= new EventHandler(zoomAnimation_Completed);
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            map.Layers.Projection = null;

            // fire map viewport events manually
            map.FireViewportBeginChanged();
            map.FireViewportWhileChanged();
            map.FireViewportEndChanged();
        }
        private void animationChaeckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (map != null)
                map.UseAnimation = true;
        }

        private void animationChaeckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            map.UseAnimation = false;
        }

        private void fullScreenCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Host.Content.IsFullScreen = true;
        }

        private void fullScreenCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Application.Current.Host.Content.IsFullScreen = false;
        }

        #endregion

        #region use cases

        SilverMap.UseCases.HowTos.SimpleShapeDemo simpleShapeDemo;
        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            simpleShapeDemo = new SilverMap.UseCases.HowTos.SimpleShapeDemo(map);
        }

        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            simpleShapeDemo.Remove();
        }

        SilverMap.UseCases.GeoXaml.GeoXamlDemo geoXamlDemo;
        private void checkBox3_Checked(object sender, RoutedEventArgs e)
        {
            geoXamlDemo = new SilverMap.UseCases.GeoXaml.GeoXamlDemo(map);
        }

        private void checkBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            geoXamlDemo.Remove();
        }

        SilverMap.UseCases.GeoRSS.GeoRssDemo geoRssDemo;
        private void checkBox4_Checked(object sender, RoutedEventArgs e)
        {
            geoRssDemo = new SilverMap.UseCases.GeoRSS.GeoRssDemo(this.map);
        }

        private void checkBox4_Unchecked(object sender, RoutedEventArgs e)
        {
            geoRssDemo.Remove();
        }

        SilverMap.UseCases.HowTos.ForePaneElements forePaneElements;
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            forePaneElements = new SilverMap.UseCases.HowTos.ForePaneElements(map);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            forePaneElements.Remove();
        }

        SilverMap.UseCases.Geocoding.GeocodingControl geocoding;
        private void checkBox6_Checked(object sender, RoutedEventArgs e)
        {       
            geocoding = new SilverMap.UseCases.Geocoding.GeocodingControl(UseCasePanel, map);
        }

        private void checkBox6_Unchecked(object sender, RoutedEventArgs e)
        {
            geocoding.Remove();
        }

        SilverMap.UseCases.Routing.RoutingControl routing;
        private void checkBox7_Checked(object sender, RoutedEventArgs e)
        {
            routing = new SilverMap.UseCases.Routing.RoutingControl(UseCasePanel, map);
        }

        private void checkBox7_Unchecked(object sender, RoutedEventArgs e)
        {
            routing.Remove();
        }

        SilverMap.UseCases.XmapContent.XmapContent xmapContent;
        private void checkBox8_Checked(object sender, RoutedEventArgs e)
        {
            xmapContent = new UseCases.XmapContent.XmapContent(map);
        }

        private void checkBox8_Unchecked(object sender, RoutedEventArgs e)
        {
            xmapContent.Remove();
        }

        SilverMap.UseCases.SharpMap.SharpMapUseCase sharpMapDemo;
        private void checkBox9_Checked(object sender, RoutedEventArgs e)
        {
            sharpMapDemo = new UseCases.SharpMap.SharpMapUseCase(UseCasePanel, mapControl);
        }

        private void checkBox9_Unchecked(object sender, RoutedEventArgs e)
        {
            sharpMapDemo.Remove();
        }

        SilverMap.UseCases.Wms.WmsUseCase wmsDemo;
        private void checkBox10_Checked(object sender, RoutedEventArgs e)
        {
            wmsDemo = new UseCases.Wms.WmsUseCase(UseCasePanel, mapControl.LayerManager, map);
        }

        private void checkBox10_Unchecked(object sender, RoutedEventArgs e)
        {
            wmsDemo.Remove();
        }

        #endregion

        #region select base map

        private Image bingLogo;
        private WatermarkControl watermarkControl;
        public void SetBaseMapProvider(int mapProvider)
        {
            if (map == null)
                return;

            // remove all previous base layers
            var baseLayers = from layer in mapControl.LayerManager where layer.Category == LayerCategory.BaseMap select layer;
            foreach (ILayerPresenter baseLayer in baseLayers.ToList<ILayerPresenter>())
                mapControl.LayerManager.Remove(baseLayer);

            if (watermarkControl != null)
            {
                map.Layers.Children.Remove(watermarkControl);
                watermarkControl = null;
            }
            // set bing logo
            if (bingLogo != null)
            {
                mapControl.BottomLeftPanel.Children.Remove(bingLogo);
                bingLogo = null;
            }
            if (mapProvider == 2 || mapProvider == 3 || mapProvider == 4)
            {
                bingLogo = new Image
                {
                    Stretch = System.Windows.Media.Stretch.None,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Source = new BitmapImage(new Uri("http://dev.virtualearth.net/Branding/logo_powered_by.png"))
                };
                mapControl.BottomLeftPanel.Children.Insert(0, bingLogo);

                if (string.IsNullOrEmpty(bingMapsKey))
                {
                    watermarkControl = new WatermarkControl();
                    map.Layers.Children.Add(watermarkControl);
                }
            }

            switch (mapProvider)
            {
                case 0:
                    {
                        // add xMap layers using ASP.NET
                        mapControl.LayerManager.Add(new BackgroundLayer
                        {
                            TileSource = new PtvDawnTileSource(App.BaseUrl + "/XmapBackgroundHandler.ashx") {  Mode = "bg"  },
                            Copyright = "© PTV, HERE",
                        });
                        mapControl.LayerManager.Add(new LabelLayer { OverlayProvider = new PtvDawnLabelOverlay(App.BaseUrl + "/XmapLabelHandler.ashx"), Copyright = "© PTV, HERE" });
                        break;
                    }
                case 1:
                    {
                        // add xServer internet layers
                        mapControl.LayerManager.Add(new BackgroundLayer { TileSource = new XServerInternetTileSource("http://xmap-eu-n-test.cloud.ptvgroup.com"), Copyright = "© PTV, HERE" });
                        mapControl.LayerManager.Add(new LabelLayer { OverlayProvider = new XServerInternetTileOverlay("http://xmap-eu-n-test.cloud.ptvgroup.com", <insert your token here>), Copyright = "© PTV, HERE" });
                        break;
                    }
                case 2:
                    {
                        // add bing maps aerials
                        var bingLayer = new BackgroundLayer
                        {
                            TrilinearFilter = false,
                            TileThreshold = .5,
                            IsAerial = true,
                            Copyright = "© 2011 Microsoft Corporation"
                        };
                        mapControl.LayerManager.Add(bingLayer);

                        // you need a bing maps key for official aerial support!
                        if (string.IsNullOrEmpty(bingMapsKey))
                            bingLayer.TileSource = new BingTileSource(new AerialTestMetaInfo());
                        else
                        {
                            // request meta info
                            BingMetaInfo metaInfo = new BingMetaInfo();
                            metaInfo.Requested += (o, e) => bingLayer.TileSource = new BingTileSource(metaInfo);
                            metaInfo.Request(BingImagerySet.Aerial, BingMapVersion.v1, bingMapsKey);
                        }
                        break;
                    }
                case 3:
                case 4:
                    {
                        // add bing maps aerials. 
                        var bingLayer = new BackgroundLayer
                        {
                            Name = "Aerials",
                            TrilinearFilter = false,
                            TileThreshold = .5,
                            IsAerial = true,
                            Copyright = "© 2011 Microsoft Corporation"
                        };
                        mapControl.LayerManager.Add(bingLayer);

                        // you need a bing maps key for official aerial support!
                        if (string.IsNullOrEmpty(bingMapsKey))
                            bingLayer.TileSource = new BingTileSource(new AerialTestMetaInfo());
                        else
                        {
                            // request meta info
                            BingMetaInfo metaInfo = new BingMetaInfo();
                            metaInfo.Requested += (o, e) => bingLayer.TileSource = new BingTileSource(metaInfo);
                            metaInfo.Request(BingImagerySet.Aerial, BingMapVersion.v1, bingMapsKey);
                        }

                        // add xMap layers using ASP.NET
                        mapControl.LayerManager.Add(new BackgroundLayer
                        {
                            TileSource = new PtvDawnTileSource(App.BaseUrl + "/XmapBackgroundHandler.ashx")
                            {
                                Mode = mapProvider == 3 ? "bg" : "street"              
                            },
                            Opacity = mapProvider == 3 ? .5 : 1,
                            ZIndex = mapProvider == 3 ? 1 : 100,
                            Copyright = "© PTV, HERE",                        
                        });
                        mapControl.LayerManager.Add(new LabelLayer { OverlayProvider = new PtvDawnLabelOverlay(App.BaseUrl + "/XmapLabelHandler.ashx"), Copyright = "© PTV, HERE" });
                        break;
                    }
                case 5:
                    {
                        // add OSM tile layer
                        mapControl.LayerManager.Add(new BackgroundLayer
                        {
                            TileSource = new OsmTileSource(),
                            Copyright = "© OpenStreetMap contributors, CC-BY-SA",
                            TrilinearFilter = false, // don't interpolate between tile levels
                            TileThreshold = .8, // don't scale down more than 80%, so texts are still readable
                        });
                        break;
                    }
                case 6:
                    {
                        mapControl.LayerManager.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.NormalDay, nokiaAppId, nokiaToken);
                        break;
                    }
                case 7:
                    {
                        mapControl.LayerManager.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.HybridDay, nokiaAppId, nokiaToken);
                        break;
                    }
                case 8:
                    {
                        mapControl.LayerManager.AddNokiaLayer(Nokia.Type.MapTile, Nokia.Scheme.TerrainDay, nokiaAppId, nokiaToken);
                        break;
                    }
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(0);
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(1);
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(2);
        }

        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(3);
        }

        private void RadioButton_Checked_5(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(4);
        }

        #endregion

        private void RadioButton_Checked_6(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(5);
        }

        private void RadioButton_Checked_7(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(6);
        }

        private void RadioButton_Checked_8(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(7);
        }

        private void RadioButton_Checked_9(object sender, RoutedEventArgs e)
        {
            SetBaseMapProvider(8);
        }
    }

    public class AerialTestMetaInfo : BingMetaInfo
    {
        public AerialTestMetaInfo()
        {
            MinZoom = 1;
            MaxZoom = 19;

            Key = string.Empty;
            ImageUrl = @"http://ecn.{subdomain}.tiles.virtualearth.net/tiles/a{quadkey}.jpeg?g=671&mkt={culture}";
            ImageUrlSubDomains = new string[] { "t1", "t2", "t3", "t4" };
        }
    }
}