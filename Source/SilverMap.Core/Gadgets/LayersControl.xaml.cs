//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    /// <summary>
    /// Interaction logic for LayersControl.xaml
    /// </summary>
    public partial class LayersControl : UserControl
    {
        public LayersControl()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(LayersControl_Loaded);
        }

        private LayerManager layerManager;
        void LayersControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.layerManager = this.FindRelative<LayerManagerElement>().layerManager;
            this.layerManager.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LayerManager_CollectionChanged);

            UpdateLayerList();
        }

        void LayerManager_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLayerList();
        }

        void UpdateLayerList()
        {
            LayersStack.Children.Clear();
            LayersStack.RowDefinitions.Clear();

            int cnt = layerManager.Count;
            foreach (ILayerPresenter layer in this.layerManager)
            {
                cnt--;

                LayersStack.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

                var textBox = new TextBlock {Text = layer.Caption, Margin = new Thickness(2)};
                Grid.SetColumn(textBox, 1);
                Grid.SetRow(textBox, cnt);
                this.LayersStack.Children.Add(textBox);

                var checkBox = new CheckBox
                                   {
                                       Tag = layer.Name,
                                       IsChecked = layerManager.IsVisible(layer),
                                       Margin = new Thickness(2)
                                   };
                checkBox.Checked += new RoutedEventHandler(checkBox_Checked);
                checkBox.Unchecked += new RoutedEventHandler(checkBox_Unchecked);
                Grid.SetColumn(checkBox, 2);
                Grid.SetRow(checkBox, cnt);
                this.LayersStack.Children.Add(checkBox);

                var slider = new Slider {Tag = layer.Name, Width = 80, Minimum = 0, Maximum = 100};
                slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
                slider.Value = layer.Opacity * 100;
                Grid.SetColumn(slider, 3);
                Grid.SetRow(slider, cnt);
                this.LayersStack.Children.Add(slider);

                var img = layer.Icon;
                if (img != null)
                {
                    var image = new Image {Source = img, Margin = new Thickness(2)};
                    Grid.SetColumn(image, 0);
                    Grid.SetRow(image, cnt);
                    this.LayersStack.Children.Add(image);
                }
            }
        }

        void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.layerManager[(sender as Slider).Tag as string].Opacity = e.NewValue / 100.0;
        }

        void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string layerName = checkBox.Tag as string;

            this.layerManager.SetVisible(layerManager[layerName], false);
        }

        void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string layerName = checkBox.Tag as string;

            this.layerManager.SetVisible(layerManager[layerName], true);
        }
    }
}
