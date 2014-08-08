//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public class LayerManagerElement : FrameworkElement
    {
        public LayerManager layerManager = new LayerManager();

        public LayerManagerElement()
        {
            this.Loaded += new RoutedEventHandler(LayerManagerElement_Loaded);
        }

        FrameworkElement fe;
        void LayerManagerElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            if (fe != null)
                return;

            fe = this.Parent as FrameworkElement;

            if (fe == null)
                return;

            while ((fe.FindName("Map")) == null)
            {
                fe = fe.Parent as FrameworkElement;
            }

            foreach (Map map in MapElementExtensions.FindVisualChildren<Map>(fe))
                layerManager.RegisterMap(map);
        }
    }
}
