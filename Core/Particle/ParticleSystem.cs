/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class ParticleSystem : IXmlData, IPlayData
    {
        #region Private Members
        [PlayData]
        [XmlAttribute]
        string name;
        IList<ParticleType> customTypes;
        IList<Layer> layers;
        IList<Component> componentTree;
        [XmlAttribute]
        int customTypeIndex;
        [XmlAttribute]
        int layerIndex;
        IDictionary<string, int> componentIndex;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public IList<ParticleType> CustomTypes { get { return customTypes; } }
        public IList<Layer> Layers { get { return layers; } }
        public IList<Component> ComponentTree { get { return componentTree; } }
        public int CustomTypeIndex { get { return customTypeIndex++; } }
        public int LayerIndex { get { return layerIndex++; } }
        #endregion

        #region Constructor
        public ParticleSystem(string name)
        {
            this.name = name;
            customTypes = new ObservableCollection<ParticleType>();
            layers = new ObservableCollection<Layer>();
            componentTree = new ObservableCollection<Component>();
            componentIndex = new Dictionary<string, int>();
            layers.Add(new Layer("Main"));
        }
        #endregion

        #region Public Methods
        public int GetComponentIndex()
        {
            int index = 0;
            foreach (var pair in componentIndex)
                index += pair.Value;
            
            return index;
        }
        public int GetAndIncreaseComponentIndex(string componentType)
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
        public object Clone()
        {
            var clone = MemberwiseClone() as ParticleSystem;
            clone.customTypes = new ObservableCollection<ParticleType>();
            foreach (var type in customTypes)
                clone.customTypes.Add(type.Clone() as ParticleType);

            clone.layers = new ObservableCollection<Layer>();
            clone.componentTree = new ObservableCollection<Component>();
            foreach (var layer in layers)
                clone.layers.Add(layer.Clone() as Layer);

            clone.componentIndex = new Dictionary<string, int>();
            foreach (var index in componentIndex)
                clone.componentIndex[index.Key] = index.Value;

            return clone;
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "ParticleSystem";
            var particleSystemNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                particleSystemNode = node;

            XmlHelper.BuildFromFields(this, particleSystemNode);
            //customTypes
            XmlHelper.BuildFromObjectList(customTypes, new ParticleType(0), particleSystemNode, "CustomTypes");
            //layers
            XmlHelper.BuildFromObjectList(layers, new Layer(""), particleSystemNode, "Layers");
            //componentIndex
            XmlHelper.BuildFromDictionary(componentIndex, particleSystemNode, "ComponentIndex");
            return particleSystemNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {;
            var particleSystemNode = doc.CreateElement("ParticleSystem");
            XmlHelper.StoreFields(this, doc, particleSystemNode);
            //customTypes
            XmlHelper.StoreObjectList(customTypes, doc, particleSystemNode, "CustomTypes");
            //layers
            XmlHelper.StoreObjectList(layers, doc, particleSystemNode, "Layers");
            //componentIndex
            XmlHelper.StoreDictionary(componentIndex, doc, particleSystemNode, "ComponentIndex");
            node.AppendChild(particleSystemNode);
            return particleSystemNode;
        }
        public List<byte> GeneratePlayData()
        {
            var particleSystemBytes = new List<Byte>();
            PlayDataHelper.GenerateFields(this, particleSystemBytes);
            //customTypes
            PlayDataHelper.GenerateObjectList(customTypes, particleSystemBytes);
            //layers
            PlayDataHelper.GenerateObjectList(layers, particleSystemBytes);
            return PlayDataHelper.CreateBlock(particleSystemBytes);
        }
        #endregion
    }
}
