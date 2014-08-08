//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public class MapGadget : UserControl
    {
        public Map Map { get; set; }

        public MapGadget()
        {
            this.Loaded += new RoutedEventHandler(DimmerControl_Loaded);
        }

        void DimmerControl_Loaded(object sender, RoutedEventArgs e)
        {
            // already initialized
            if (Map != null)
                return;

            if(System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            // get parent 
            var fe = this.Parent as FrameworkElement;
            if (fe == null)
                return;

            // find map element as relative 
            Map = fe.FindRelative<Map>();

            Initialize();
        }

        protected virtual void Initialize()
        {
        }
    }
}
