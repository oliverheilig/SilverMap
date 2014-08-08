//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ptvag.Dawn.Controls.SilverMap.Core;
using Ptvag.Dawn.Controls.WpfMap.Symbols;
using SilverMap.Tools;

namespace SilverMap.UseCases.GeoRSS
{
    public class GeoRssDemo : Canvas
    {
        private Map map;
        private ScaleTransform adjustTransform;

        public GeoRssDemo(Map map)
        {
            this.map = map;

            // initialize canvas
            Canvas.SetZIndex(this, 100); // place on top of labels
            this.RenderTransform = TransformFactory.CreateTransform(SpatialReference.PtvMercatorInvertedY);
            this.map.GeoCanvas.Children.Add(this);

            // initialize tranformation for power-law scaling of symbols
            adjustTransform = new ScaleTransform();
            AdjustTransform();
            map.ViewportWhileChanged += new EventHandler(map_ViewportWhileChanged);

            System.Net.WebRequest request = System.Net.WebRequest.Create(
                 new Uri("http://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_week.atom", UriKind.Absolute));
            request.BeginGetResponse(new AsyncCallback(CreateRssRequest), new object[] { request, this });
        }

        void map_ViewportWhileChanged(object sender, EventArgs e)
        {
            AdjustTransform();
        }

        public void Remove()
        {
            map.GeoCanvas.Children.Remove(this);
            map.ViewportWhileChanged -= new EventHandler(map_ViewportWhileChanged);
        }

        private void CreateRssRequest(IAsyncResult asyncRes)
        {
            object[] state = (object[])asyncRes.AsyncState;
            System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)state[0];

            if (!httpRequest.HaveResponse) { return; }

            System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)httpRequest.EndGetResponse(asyncRes);
            if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK) { return; }
            Stream stream = httpResponse.GetResponseStream();

            this.Dispatcher.BeginInvoke(() => { ParseAtomUsingLinq(stream); });
        }

        private void ParseAtomUsingLinq(System.IO.Stream stream)
        {

            System.Xml.Linq.XDocument feedXML = System.Xml.Linq.XDocument.Load(stream);
            System.Xml.Linq.XNamespace xmlns = "http://www.w3.org/2005/Atom"; //Atom namespace
            System.Xml.Linq.XNamespace georssns = "http://www.georss.org/georss"; //GeoRSS Namespace

            // time to learn some LINQ
            var posts = (from item in feedXML.Descendants(xmlns + "entry")
                         select new
                                    {
                                        Title = item.Element(xmlns + "title").Value,
                                        Published = DateTime.Parse(item.Element(xmlns + "updated").Value),
                                        Url = item.Element(xmlns + "link").Attribute("href").Value,
                                        Description = item.Element(xmlns + "summary").Value,
                                        Location = CoordinateGeoRssPoint(item.Element(georssns + "point")),
                                        //Simple GeoRSS <georss:point>X Y</georss.point>
                                    }).ToList();

            //// calucate bounds for georss points
            //var postBounds = new
            //{
            //    MinX = posts.Min(p => p.Location.X),
            //    MaxX = posts.Max(p => p.Location.X),
            //    MinY = posts.Min(p => p.Location.Y),
            //    MaxY = posts.Max(p => p.Location.Y)
            //};

            //// set envelope
            this.map.SetLatLonZ(0, 0, 1);

            int i = 0;
            // order posts by latitude, so they overlap nicely on the map
            foreach (var post in from post in posts orderby post.Location.Y descending select post)
            {
                if (!double.IsNaN(post.Location.X) && !double.IsNaN(post.Location.Y))
                {
                    // transform wgs to ptv mercator coordinate
                    System.Windows.Point mapPoint = GeoTransform.WGSToPtvMercator(post.Location);

                    // create button and set pin template
                    var pin = new Pin
                                  {
                                      // a bug in SL throws an obscure exception if children share the same name        
                                      // http://forums.silverlight.net/forums/t/134299.aspx
                                      // the name is needed in xaml for data binding, so just create a unique name at runtime
                                      Name = "pin" + (i++).ToString(),
                                      // set render transform for power-law scaling
                                      RenderTransform = adjustTransform,
                                      // scale around lower right
                                      RenderTransformOrigin = new Point(1, 1)
                                  };

                    // set size by magnitude
                    double magnitude = MagnitudeFromTitle(post.Title);
                    pin.Height = magnitude * 10;
                    pin.Width = magnitude * 10;

                    // calculate a value between 0 and 1 and use it for a blend color
                    double relativeDanger = Math.Max(0, Math.Min(1, (magnitude - 2.5) / 4));
                    pin.Color = ColorBlend.Danger.GetColor((float)relativeDanger);

                    // set tool tip information
                    ToolTipService.SetToolTip(pin, post.Title);

                    // set position and add to canvas (invert y-ordinate)
                    // set lower right (pin-tip) as position
                    Canvas.SetLeft(pin, mapPoint.X - pin.Width);
                    Canvas.SetTop(pin, -(mapPoint.Y + pin.Height));
                    this.Children.Add(pin);
                }
            }
        }

        /// <summary>
        /// Adjust the transformation for logarithmic scaling
        /// </summary>
        private void AdjustTransform()
        {
            // if the factor is 1.0 the elements have pixel size
            // if the factor is 0.0 the elements have mercator size
            // for a factor between the elements are scaled with a logarithmic multiplicator
            double logicalScaleFactor = .75;

            adjustTransform.ScaleX = 10 * System.Math.Pow(map.CurrentScale, logicalScaleFactor);
            adjustTransform.ScaleY = 10 * System.Math.Pow(map.CurrentScale, logicalScaleFactor);
        }

        private double MagnitudeFromTitle(string title)
        {
            try
            {
                string pattern = @"^M (?<number>[0-9].[0-9]),*";

                Match numberMatch = Regex.Match(title, pattern);
                string number = numberMatch.Groups["number"].Value;

                return System.Convert.ToDouble(number, NumberFormatInfo.InvariantInfo);
            }
            catch (Exception ex)
            {
                return 0.1;
            }
        }

        private System.Windows.Point CoordinateGeoRssPoint(System.Xml.Linq.XElement elm)
        {
            System.Windows.Point emptyPoint = new Point(Double.NaN, double.NaN);

            if (elm == null) return emptyPoint;
            string val = elm.Value;
            string[] vals = val.Split(new char[] { ' ' });
            if (vals.Length != 2) return emptyPoint;

            double x, y;

            if (double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                return new System.Windows.Point(x, y);
            else
                return emptyPoint;
        }
    }
}
