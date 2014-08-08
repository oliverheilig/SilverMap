//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ptvag.Dawn.Controls.SilverMap.Core.Gadgets
{
    /// <summary>
    /// Interaction logic for CopyrightControl.xaml
    /// </summary>
    public partial class CopyrightControl : UserControl
    {
        public CopyrightControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LayerManagerElement_Loaded);
        }

        LayerManager layerManager;
        void LayerManagerElement_Loaded(object sender, RoutedEventArgs e)
        {
            layerManager = this.FindRelative<LayerManagerElement>().layerManager;
            layerManager.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(layerManager_CollectionChanged);
            UpdateCopyrightText();
        }

        void layerManager_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCopyrightText();
        }

        private void UpdateCopyrightText()
        {
            this.TextStack.Children.Clear();

            var copyrightTexts = new HashSet<string>();
            foreach (var layer in layerManager.Where(layer => !(string.IsNullOrEmpty(layer.Copyright))))
                copyrightTexts.Add(layer.Copyright);

            foreach(string copyright in copyrightTexts)
            {
                var text = new TextBlock {Foreground = new SolidColorBrush(Colors.Black), Text = copyright};
                this.TextStack.Children.Add(text);
            }
        }
    }

#if PHONE7
    public class HashSet<T> : Dictionary<T, T>
    {
    }
#endif
}
