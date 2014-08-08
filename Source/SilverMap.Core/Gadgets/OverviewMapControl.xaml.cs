//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public partial class OverviewMapControl : MapGadget
    {
        public OverviewMapControl()
        {
            InitializeComponent();
        }

        protected override void Initialize()
        {
            overviewMap.ParentMap = Map;            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OverViewMapGrid.Visibility = (OverViewMapGrid.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
            overviewMap.IsEnabled = OverViewMapGrid.Visibility == Visibility.Visible;

            if (OverViewMapGrid.Visibility == Visibility.Visible)
            {
                overviewMap.UpdateOverviewMap(false);
                overviewMap.UpdateRect();
            }
        }
    }
}
