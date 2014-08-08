//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    public partial class DimmerControl : MapGadget
    {
        Grid dimmer;

        public DimmerControl()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(DimmerControl_Loaded);
#if !PHONE7
            this.dimmSlider.MouseRightButtonDown += new MouseButtonEventHandler(dimmSlider_MouseRightButtonDown);
#endif
        }

        void DimmerControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            dimmer = new Grid();
            Canvas.SetZIndex(dimmer, 32);
            Map.BackPaneCanvas.Children.Add(dimmer);
        }

        private void dimmSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dimmer.Background = dimmSlider.Value >= 0 ? 
                new SolidColorBrush(Color.FromArgb((byte)(dimmSlider.Value / 100.0 * 255.0), 255, 255, 255)) : 
                new SolidColorBrush(Color.FromArgb((byte)(-dimmSlider.Value / 100.0 * 255.0), 0, 0, 0));
        }

        private void dimmSlider_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            dimmSlider.Value = 0;

            e.Handled = true;
        }
    }
}
