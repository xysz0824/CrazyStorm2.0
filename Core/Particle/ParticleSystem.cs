/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CrazyStorm.Core
{
    public class ParticleSystem
    {
        #region Private Members
        string name;
        IList<ParticleType> customType;
        IList<Layer> layers;
        IList<Component> componentTree;
        int layerIndex;
        IDictionary<Type, int> componentIndex;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public IList<ParticleType> CustomType { get { return customType; } }
        public IList<Layer> Layers { get { return layers; } }
        public IList<Component> ComponentTree { get { return componentTree; } }
        public int LayerIndex { get { return layerIndex++; } }
        #endregion

        #region Constructor
        public ParticleSystem(string name)
        {
            this.name = name;
            customType = new ObservableCollection<ParticleType>();
            layers = new ObservableCollection<Layer>();
            componentTree = new ObservableCollection<Component>();
            componentIndex = new Dictionary<Type, int>();
            layers.Add(new Layer("Main"));
        }
        #endregion

        #region Public Methods
        public int GetComponentIndex(Type componentType)
        {
            if (!componentIndex.ContainsKey(componentType))
                componentIndex[componentType] = 0;

            return componentIndex[componentType]++;
        }
        public void AddComponentToLayer(Layer layer, Component component)
        {
            if (component.Parent == null)
                componentTree.Add(component);
            else
                component.Parent.Children.Add(component);

            foreach (var item in component.GetPosterity())
            {
                layer.Components.Add(item);
            }
            layer.Components.Add(component);
        }
        public void DeleteComponentFromLayer(Layer layer, Component component)
        {
            if (component.Parent == null)
                componentTree.Remove(component);
            else
                component.Parent.Children.Remove(component);

            foreach (var item in component.GetPosterity())
            {
                layer.Components.Remove(item);
            }
            layer.Components.Remove(component);
        }
        public void AddLayer(Layer layer)
        {
            foreach (var component in layer.Components)
                if (component.Parent == null)
                    componentTree.Add(component);

            layers.Add(layer);
        }
        public void DeleteLayer(Layer layer)
        {
            foreach (var component in layer.Components)
                if (component.Parent == null)
                    componentTree.Remove(component);

            layers.Remove(layer);
        }
        #endregion
    }
}
