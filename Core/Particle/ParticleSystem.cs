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
        ObservableCollection<ParticleType> customType;
        ObservableCollection<Layer> layers;
        ObservableCollection<Component> components;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public ObservableCollection<ParticleType> CustomType { get { return customType; } }
        public ObservableCollection<Layer> Layers { get { return layers; } }
        public ObservableCollection<Component> Components { get { return components; } }
        #endregion

        #region Constructor
        public ParticleSystem(string name)
        {
            this.name = name;
            customType = new ObservableCollection<ParticleType>();
            layers = new ObservableCollection<Layer>();
            components = new ObservableCollection<Component>();
            layers.Add(new Layer("Main"));
        }
        #endregion

        #region Public Methods
        public void AddComponentToLayer(Layer layer, Component component)
        {
            components.Add(component);
            layer.Components.Add(component);
        }
        public void DeleteComponentFromLayer(Layer layer, Component component)
        {
            components.Remove(component);
            layer.Components.Remove(component);
        }
        public void AddLayer(Layer layer)
        {
            foreach (var component in layer.Components)
                components.Add(component);
            layers.Add(layer);
        }
        public void DeleteLayer(Layer layer)
        {
            foreach (var component in layer.Components)
                components.Remove(component);
            layers.Remove(layer);
        }
        #endregion
    }
}
