//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public interface ILayerPresenter
    {
        string Name { get; }

        string Category { get; }

        int ZIndex { get; set; }

        string Caption { get; }

        ImageSource Icon { get;}

        double Opacity { get; set; }

        void AddToMap(Map map);

        void RemoveFromMap(Map map);

        string Copyright { get; }
    }

    public static class LayerCategory
    {
        public static string BaseMap = "BaseMap";

        public static string POI = "POI";

        public static string UseCase = "UseCase";
    }
}
