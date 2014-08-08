//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows.Controls;
using Ptvag.Dawn.Controls.SilverMap.Core;

namespace SilverMap.UseCases.SharpMap
{
    public class SharpMapUseCase
    {
        private Legend legend = new Legend();
        private StackPanel workspace;
        SharpMapLayer layer;
        MapControl mapControl;

        public SharpMapUseCase(StackPanel workspace, MapControl mapControl)
        {
            this.workspace = workspace;
            this.mapControl = mapControl;

            workspace.Children.Add(legend);
            workspace.UpdateLayout();
            layer = new SharpMapLayer {MapControl = mapControl, ZIndex = 48};
            mapControl.LayerManager.Add(layer);

            mapControl.Map.SetLatLonZ(51, 10, 6);

        }

        public void Remove()
        {
            mapControl.LayerManager.Remove(layer);
            workspace.Children.Remove(legend);
        }
    }
}
