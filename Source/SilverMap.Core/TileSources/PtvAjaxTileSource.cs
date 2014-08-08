//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    // a tile source which queries the ptv ajax servlet
    public class PtvAjaxTileSource : PtvTileSourceBase
    {
        protected string baseUrl;

        public PtvAjaxTileSource(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        protected override Uri GetUri(double minX, double minY, double maxX, double maxY)
        {
            string str = string.Format("{0}/MapServlet?left={1}&top={2}&right={3}&bottom={4}&width=256&height=256&hiddenLayers=Town",
                baseUrl,
                (int)Math.Round(minX), (int)Math.Round(minY), (int)Math.Round(maxX), (int)Math.Round(maxY));

            return new Uri(str);
        }
    }
}
