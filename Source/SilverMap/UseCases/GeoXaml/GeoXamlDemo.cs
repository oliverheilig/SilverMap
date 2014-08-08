//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using Ptvag.Dawn.Controls.SilverMap.Core;

namespace SilverMap.UseCases.GeoXaml
{
    public class GeoXamlDemo
    {
        private Map map;
        GeoXamlLayer xamlLayer;

        public GeoXamlDemo(Map map)
        {
            this.map = map;

            string serverUrl = App.BaseUrl + "/GeoXaml/DistrictRastatt.xaml";
            xamlLayer = new GeoXamlLayer(map, serverUrl);
            map.SetXYZ(911656, 6230022, 10);
        }

        public void Remove()
        {
            map.GeoCanvas.Children.Remove(xamlLayer);
            xamlLayer.Dispose();
            xamlLayer = null;
        }
    }
}
