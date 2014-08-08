//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;

namespace Ptvag.Dawn.Controls.SilverMap.Core.TileSources
{
    public class PtvDawnTileSource : PtvTileSourceBase
    {
        protected string baseUrl;

        public PtvDawnTileSource(string baseUrl)
        {
            this.baseUrl = baseUrl;
            this.Mode = "bg";
        }

        public string Mode { get; set; }

        protected override Uri GetUri(double minX, double minY, double maxX, double maxY)
        {
            return new Uri(
                string.Format("{0}?mode={1}&left={2}&top={3}&right={4}&bottom={5}&width=256&height=256",
                baseUrl, Mode,
                (int)Math.Round(minX), (int)Math.Round(minY), 
                (int)Math.Round(maxX), (int)Math.Round(maxY)));
        }
    }
}
