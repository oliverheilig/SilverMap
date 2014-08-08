//--------------------------------------------------------------
// Copyright (c) 2011 PTV Planung Transport Verkehr AG
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Ptvag.Dawn.Controls.SilverMap.Core
{
    public interface ILayerSettings
    {
        void ShowSettingsDialog(ILayerPresenter layer);
    }

    public class LayerManager : ObservableCollection<ILayerPresenter>, IDisposable
    {
        public ILayerSettings LayerSettings { get; set; }

        private Dictionary<ILayerPresenter, bool> visiblities = new Dictionary<ILayerPresenter, bool>();
        private Dictionary<ILayerPresenter, bool> selectabilities = new Dictionary<ILayerPresenter, bool>();

        public LayerManager()
        {
            CollectionChanged += LayerManager_CollectionChanged;
        }

        public ILayerPresenter this[string layerName]
        {
            get { return this.FirstOrDefault(layer => layer.Name == layerName); }
        }

        public void ShowSettingsDialog(ILayerPresenter layer)
        {
            if (LayerSettings != null)
                LayerSettings.ShowSettingsDialog(layer);
        }

        void LayerManager_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (Map map in maps)
                        if (map.IsEnabled)
                            foreach (ILayerPresenter layer in visiblities.Keys)
                                if (IsVisible(layer))
                                    layer.RemoveFromMap(map);

                    foreach (ILayerPresenter layer in this)
                    {
                        selectabilities.Remove(layer);
                        visiblities.Remove(layer);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (ILayerPresenter layer in e.NewItems)
                    {
                        selectabilities[layer] = true;
                        visiblities[layer] = true;

                        foreach (Map map in maps)
                            if (map.IsEnabled && IsVisible(layer))
                                layer.AddToMap(map);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (ILayerPresenter layer in e.OldItems)
                    {
                        foreach (Map map in maps)
                            if (map.IsEnabled && IsVisible(layer))
                                layer.RemoveFromMap(map);

                        selectabilities.Remove(layer);
                        visiblities.Remove(layer);
                    }
                    break;
            }
        }

        protected override void InsertItem(int index, ILayerPresenter item)
        {
            int i;
            for (i = 0; i < this.Count; i++)
            {
                if (this[i].ZIndex > item.ZIndex)
                    break;
            }

            base.InsertItem(i, item);
        }

        List<Map> maps = new List<Map>();
        public void RegisterMap(Map map)
        {
            map.IsEnabledChanged += map_IsEnabledChanged;
            maps.Add(map);

            if (map.IsEnabled)
                foreach (var layer in this.Where(IsVisible))
                    layer.AddToMap(map);
        }

        void map_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Map map = sender as Map;

            if ((bool)e.NewValue)
            {
                foreach (ILayerPresenter layer in this.Where(IsVisible))
                    layer.AddToMap(map);
            }
            else
            {
                foreach (ILayerPresenter layer in this.Where(IsVisible))
                    layer.RemoveFromMap(map);
            }
        }

        public void UnregisterMap(Map map)
        {
            map.IsEnabledChanged -= map_IsEnabledChanged;
            maps.Remove(map);

            if (map.IsEnabled)
                foreach (ILayerPresenter layer in this.Where(IsVisible))
                    layer.RemoveFromMap(map);
        }

        public bool IsVisible(ILayerPresenter layer)
        {
            if (!visiblities.ContainsKey(layer))
                return false;

            return visiblities[layer];
        }

        public void SetVisible(ILayerPresenter layer, bool visible)
        {
            visiblities[layer] = visible;

            if (visible)
            {
                foreach (Map map in maps.Where(map => map.IsEnabled))
                    layer.AddToMap(map);
            }
            else
            {
                foreach (Map map in maps.Where(map => map.IsEnabled))
                    layer.RemoveFromMap(map);
            }
        }

        /// <summary>
        /// Selectability of the layer without taking the exclusive selectability into account.
        /// </summary>
        /// <param name="layerName">Layer name of the layer, which should be requested.</param>
        /// <returns>True, if the selectable flag is set to true without taking the exclusive selectability into account.</returns>
        public bool IsSelectableBase(ILayerPresenter layer)
        {
            return selectabilities.ContainsKey(layer) && selectabilities[layer];
        }

        /// <summary>
        /// Selectability of the layer taking the exclusive selectability into account.
        /// </summary>
        /// <param name="layerName">Layer name of the layer, which should be requested.</param>
        /// <returns>True, if the selectable flag is set to true, taking the exclusive selectability into account.
        /// It is false, if another layer is marked as exclusive selectable.
        /// </returns>
        public bool IsSelectable(ILayerPresenter layer)
        {
            if (exclusiveSelectableLayer == null)
                return IsSelectableBase(layer);
            else
                return (layer == exclusiveSelectableLayer);
        }

        public void SetSelectable(ILayerPresenter layer, bool selectable)
        {
            selectabilities[layer] = selectable;
        }

        private ILayerPresenter exclusiveSelectableLayer;

        /// <summary>
        /// Setting of a layer as the one-and-only selectable layer.
        /// </summary>
        /// 
        public ILayerPresenter ExclusiveSelectableLayer
        {
            get { return exclusiveSelectableLayer; }
            set
            {
                if (value != null && !selectabilities.ContainsKey(value))
                    value = null;

                exclusiveSelectableLayer = value;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            var clone = new LayerManager();
            foreach (ILayerPresenter layer in this)
                clone.Add(layer);

            return clone;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            foreach (ILayerPresenter layer in this)
            {
                if (layer is IDisposable)
                    (layer as IDisposable).Dispose();
            }
        }

        #endregion
    }
}
