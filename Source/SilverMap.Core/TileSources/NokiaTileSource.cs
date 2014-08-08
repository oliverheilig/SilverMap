using System;
using System.Linq;
using System.Text.RegularExpressions;
using Ptvag.Dawn.Controls.SilverMap.Core.Layers;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    public static class Nokia
    {
        /// <summary>
        /// The basic map types
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Complete map
            /// </summary>
            MapTile,  
            /// <summary>
            /// The basic map tile without streets and labels
            /// </summary>
            BaseTile,
            /// <summary>
            /// Street and label overlay onlay
            /// </summary>
            StreetTile,
            /// <summary>
            /// Label overlay only
            /// </summary>
            LabelTile,
        };

        /// <summary>
        /// The scheme for the tiles
        /// </summary>
        public enum Scheme
        {
            NormalDay,
            NormalDayCustom,
            CarNavDay,
            SatelliteDay,
            HybridDay,
            TerrainDay,
            NormalDayTransit,
            NormalDayGrey,
            CarnavDayGrey,
            NormalNightMobile,
            PedestrianDay,
            PedestrianNight,
        }

        /// <summary>
        /// Removes all nokia base map layers.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        public static void RemoveNokiaLayers(this LayerManager layers)
        {
            var nokiaLayers = from layer in layers where layer.Name.StartsWith("Nokia_") select layer;
            foreach (var layer in nokiaLayers.ToList())
                layers.Remove(layer);
        }

        /// <summary>
        /// Add a nokia layer to the layers collection of the map.
        /// </summary>
        /// <param name="layers">The layers collection.</param>
        /// <param name="type">The basic map type.</param>
        /// <param name="scheme">The scheme of the map.</param>
        /// <param name="appId">The application id.</param>
        /// <param name="token">The token.</param>
        public static void AddNokiaLayer(this LayerManager layers, Type type, Scheme scheme, string appId, string token)
        {
            string caption = (type == Nokia.Type.StreetTile) ? "Streets" : (type == Nokia.Type.LabelTile) ? "Labels" : "Base Map";
            bool isSatellite = scheme == Nokia.Scheme.SatelliteDay;
            string copyrightText = isSatellite ? "© 2012 DigitalGlobe" : "© 2012 NAVTEQ";
            bool isbaseMap = type == Nokia.Type.MapTile || type == Nokia.Type.BaseTile;

            // add OSM tile layer
            layers.Add(new BackgroundLayer
            {
                ZIndex = isbaseMap ? 1 : 100, 
                Name = "Nokia_" + type.ToString(),
                IsAerial = scheme == Nokia.Scheme.SatelliteDay,
                Caption = caption,
                TileSource = new NokiaTileSource(appId, token) { Type = type, Scheme = scheme },
                Copyright = copyrightText,
                TrilinearFilter = isSatellite, // only interpolate between tile levels for aerials
                TileThreshold = isSatellite? 1 : .8, // don't scale down more than 80%, so texts are still readable
            });
        }
    }

    public class NokiaTileSource : MapTileSourceBase
    {
        public NokiaTileSource(string appId, string token)
            : base(20) // maximum level for Nokia is 20
        {
            this.MinZoom = 0;

            this.appId = appId;
            this.token = token;

            Type = Nokia.Type.BaseTile;
            Scheme = Nokia.Scheme.NormalDay;
        }

        private string appId;
        private string token;

        public Nokia.Type Type { get; set; }
        public Nokia.Scheme Scheme { get; set; }

        protected override Uri GetUri(int x, int y, int z)
        {
            // request schema is
            // http://SERVER-URL/maptile/2.1/TYPE/MAPID/SCHEME/ZOOM/COL/ROW/RESOLUTION/FORMAT?param=value&...

            string schemeString = Regex.Replace(Scheme.ToString(), "[a-z][A-Z]", m => m.Value[0] + "." + m.Value[1]).ToLower();
            string typeString = Type.ToString().ToLower();

            return new Uri(string.Format(
                "http://{0}.maps.nlp.nokia.com/maptile/2.1/{1}/newest/{2}/{3}/{4}/{5}/256/png8?app_id={6}&token={7}",
                "1234"[(x ^ y) % 4], typeString, schemeString, z, x, y, appId, token));
        }
    }
}