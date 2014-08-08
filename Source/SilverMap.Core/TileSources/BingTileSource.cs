using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    public class BingTileSource : MapTileSourceBase
    {
        private BingMetaInfo metaInfo;

        public BingTileSource(BingMetaInfo metaInfo)
            : base(metaInfo.MaxZoom)
        {
            this.metaInfo = metaInfo;

            this.MinZoom = metaInfo.MinZoom;
        }

        protected override Uri GetUri(int x, int y, int z)
        {
            string requestString = metaInfo.ImageUrl;

            // set subdomain
            requestString = requestString.Replace("{subdomain}", metaInfo.ImageUrlSubDomains[((x ^ y) % metaInfo.ImageUrlSubDomains.Length)]);

            // set quad key
            requestString = requestString.Replace("{quadkey}", TileXYToQuadKey(x, y, z));

            // set culture
            if (requestString.Contains("{culture}"))
                requestString = requestString.Replace("{culture}", Thread.CurrentThread.CurrentUICulture.Name.ToLower());

            // set key
            if (!string.IsNullOrEmpty(metaInfo.Key))
                requestString = requestString + "&key=" + metaInfo.Key;

            // don't return "not available" image
            requestString = requestString + "&n=z";

            return new Uri(requestString);
        }

        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>A string containing the QuadKey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }
    }

    public static class BingImagerySet
    {
        public static string Aerial = "Aerial";

        public static string AerialWithLabels = "AerialWithLabels";

        public static string Road = "Road";
    }

    public static class BingMapVersion
    {
        public static string v0 = "v0";

        public static string v1 = "v1";
    }

    public class BingMetaInfo
    {
        public void Request(string imagerySet, string mapVersion, string key)
        {
            this.Key = key;

            System.Net.WebRequest request = System.Net.WebRequest.Create(
                string.Format(@"http://dev.virtualearth.net/REST/v1/Imagery/Metadata/{0}?mapVersion={1}&o=xml&key={2}", imagerySet, mapVersion, key));
            request.BeginGetResponse(new AsyncCallback(GetBingInfo), new object[] { request });
        }

        public event EventHandler Requested;

        private void GetBingInfo(IAsyncResult asyncRes)
        {
            object[] state = (object[])asyncRes.AsyncState;
            System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)state[0];

            if (!httpRequest.HaveResponse) { return; }

            System.Net.HttpWebResponse httpResponse = (System.Net.HttpWebResponse)httpRequest.EndGetResponse(asyncRes);
            if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK) { return; }
            Stream stream = httpResponse.GetResponseStream();

            // parse xml using linq
            System.Xml.Linq.XNamespace restns = "http://schemas.microsoft.com/search/local/ws/rest/v1";
            System.Xml.Linq.XDocument metaXml = System.Xml.Linq.XDocument.Load(stream);
            var resourceSets = from resourceSet in metaXml.Descendants(restns + "ResourceSets")
                               select new
                               {
                                   Resource = from resource in resourceSet.Descendants(restns + "Resources")
                                              select new
                                                        {
                                                            ImageryMetaData = from meta in resource.Descendants(restns + "ImageryMetadata")
                                                                              select new
                                                                              {
                                                                                  ImagerUrl = meta.Element(restns + "ImageUrl").Value,
                                                                                  MinZoom = System.Convert.ToInt32(meta.Element(restns + "ZoomMin").Value),
                                                                                  MaxZoom = System.Convert.ToInt32(meta.Element(restns + "ZoomMax").Value),
                                                                                  SubDomains = from subDomain in meta.Descendants(restns + "ImageUrlSubdomains")
                                                                                                    select subDomain.Elements(restns + "string")
                                                                              }
                                                        }
                               };

            // initialize properties
            var imageMeta = resourceSets.First().Resource.First().ImageryMetaData.First();
          
            this.ImageUrl = imageMeta.ImagerUrl;
            this.MinZoom = imageMeta.MinZoom;
            this.MaxZoom = imageMeta.MaxZoom;

            this.ImageUrlSubDomains = imageMeta.SubDomains.First().Select(subDomain => subDomain.Value).ToArray();

            // fire requested-event. Use dispatcher to get back to the UI-thread
            if (Requested != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(Requested, this, EventArgs.Empty);                
            }
        }

        public int MinZoom { get; set; }
        public int MaxZoom { get; set; }

        public string Key { get; set; }
        public string ImageUrl { get; set; }

        public string[] ImageUrlSubDomains { get; set; }
    }
}
