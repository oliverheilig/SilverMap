using System;
using System.Globalization;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    public class XServerInternetTileSource : MapTileSourceBase
    {
        public string BaseUrl { get; set; }

        public XServerInternetTileSource(string baseUrl)
            : base(19)
        {
            this.BaseUrl = baseUrl;
        }

        protected override Uri GetUri(int x, int y, int z)
        {
            return new Uri(string.Format(BaseUrl + "/WMS/GetTile/xmap-ajaxbg/{0}/{1}/{2}.png", x, y, z));
        }
    }

    public class XServerInternetTileOverlay : IOverlayProvider
    {
        public XServerInternetTileOverlay(string baseUrl, string token)
        {
            this.BaseUrl = baseUrl;
            this.Token = token;
        }

        public string BaseUrl { get; set; }

        public string Token { get; set; }

        public Uri GetUri(double minX, double maxX, double minY, double maxY, double width, double height)
        {
            return new Uri(string.Format("{0}/WMS/WMS?REQUEST=GetMap&width={1}&height={2}&bbox={3},{4},{5},{6}" + 
                "&format=image/png&version=1.1.1&layers=xmap-ajaxfg&transparent=true&srs=EPSG:505456&styles=&xtok={7}",
                 BaseUrl, (int)Math.Round(width), (int)Math.Round(height),
                 Convert.ToString(minX, CultureInfo.InvariantCulture), Convert.ToString(minY, CultureInfo.InvariantCulture),
                 Convert.ToString(maxX, CultureInfo.InvariantCulture), Convert.ToString(maxY, CultureInfo.InvariantCulture),
                 Token));
        }
    }
}
