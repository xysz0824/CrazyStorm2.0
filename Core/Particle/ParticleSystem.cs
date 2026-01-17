/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CrazyStorm.Core
{
    public enum OrderType
    {
        FirstAsTop,
        LastAsTop
    }
    public class ParticleSystem : IXmlData, IGeneratePlayData, ILoadPlayData, IPlayable
    {
        #region Private Members
        [StringData]
        [XmlAttribute]
        string name;
        [XmlAttribute]
        OrderType orderType;
        IList<ParticleType> customTypes;
        IList<Layer> layers;
        IList<Component> componentTree;
        [XmlAttribute]
        int customTypeIndex;
        [XmlAttribute]
        int layerIndex;
        IDictionary<int, int> componentIndex;
        IDictionary<int, int> typeSoundMap;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public OrderType OrderType
        {
            get { return orderType; }
            set { orderType = value; }
        }
        public int CurrentFrame { get; set; }
        public int TotalFrame
        {
            get
            {
                var totalFrame = 0;
                for (int i = 0; i < Layers.Count; ++i)
                {
                    //Set the biggest number as totalFrame 
                    totalFrame = Layers[i].TotalFrame > totalFrame ? Layers[i].TotalFrame : totalFrame;
                }
                return totalFrame;
            }
        }
        public IList<ParticleType> CustomTypes { get { return customTypes; } }
        public IList<Layer> Layers { get { return layers; } }
        public IList<Component> ComponentTree { get { return componentTree; } }
        public int CustomTypeIndex { get { return customTypeIndex++; } }
        public int LayerIndex { get { return layerIndex++; } }
        public IDictionary<int, int> TypeSoundMap { get { return typeSoundMap; } }
        #endregion

        #region Constructor
        public ParticleSystem()
        {
            customTypes = new GenericContainer<ParticleType>();
            layers = new GenericContainer<Layer>();
            componentTree = new GenericContainer<Component>();
        }
        public ParticleSystem(string name)
        {
            this.name = name;
            customTypes = new GenericContainer<ParticleType>();
            layers = new GenericContainer<Layer>();
            componentTree = new GenericContainer<Component>();
            componentIndex = new Dictionary<int, int>();
            typeSoundMap = new Dictionary<int, int>();
        }
        public ParticleSystem(string name, string defaultLayerName) : this(name)
        {
            layers.Add(new Layer(defaultLayerName));
        }
        #endregion

        #region Public Methods
        public int GetComponentIndex()
        {
            int index = 0;
            foreach (var pair in componentIndex) index += pair.Value;
            return index;
        }
        public int GetAndIncreaseComponentIndex(string componentType)
        {
            var typeHash = StringUtil.StableHash32_Fnv1a(componentType);
            if (!componentIndex.ContainsKey(typeHash))
                componentIndex[typeHash] = 0;

            return componentIndex[typeHash]++;
        }
        public void AddComponentToLayer(Layer layer, Component component)
        {
            if (component.Parent == null)
                componentTree.Add(component);
            else
                component.Parent.Children.Add(component);

            foreach (var item in component.GetPosterity())
                layer.Components.Add(item);

            layer.Components.Add(component);
        }
        public void DeleteComponentFromLayer(Layer layer, Component component)
        {
            if (component.Parent == null)
                componentTree.Remove(component);
            else
                component.Parent.Children.Remove(component);

            foreach (var item in component.GetPosterity())
                layer.Components.Remove(item);


            foreach (var layerItem in layers)
            {
                foreach (var componentItem in layerItem.Components)
                {
                    if (componentItem.BindingTarget == component)
                        componentItem.BindingTarget = null;
                }
            }
            layer.Components.Remove(component);
        }
        public void AddLayer(Layer layer)
        {
            foreach (var component in layer.Components)
            {
                if (component.Parent == null)
                    componentTree.Add(component);
            }
            layers.Add(layer);
        }
        public void DeleteLayer(Layer layer)
        {
            foreach (var component in layer.Components)
            {
                if (component.Parent == null)
                    componentTree.Remove(component);
            }
            layers.Remove(layer);
        }
        public object Clone()
        {
            var clone = MemberwiseClone() as ParticleSystem;
            clone.customTypes = new GenericContainer<ParticleType>();
            foreach (var type in customTypes) clone.customTypes.Add(type.Clone() as ParticleType);
            clone.layers = new GenericContainer<Layer>();
            clone.componentTree = new GenericContainer<Component>();
            foreach (var layer in layers) clone.layers.Add(layer.Clone() as Layer);
            clone.componentIndex = new Dictionary<int, int>();
            foreach (var kv in componentIndex) clone.componentIndex[kv.Key] = kv.Value;
            clone.typeSoundMap = new Dictionary<int, int>();
            foreach (var kv in typeSoundMap) clone.typeSoundMap[kv.Key] = kv.Value;
            return clone;
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "ParticleSystem";
            var particleSystemNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName) particleSystemNode = node;
            XmlHelper.BuildFromFields(this, particleSystemNode);
            //customTypes
            XmlHelper.BuildFromObjectList(customTypes, new ParticleType(0), particleSystemNode, "CustomTypes");
            //layers
            XmlHelper.BuildFromObjectList(layers, new Layer(""), particleSystemNode, "Layers");
            //componentIndex
            XmlHelper.BuildFromDictionary(componentIndex, particleSystemNode, "ComponentIndex");
            //typeSoundMap
            XmlHelper.BuildFromDictionary(typeSoundMap, particleSystemNode, "TypeSoundMap");
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
            //typeSoundMap
            XmlHelper.StoreDictionary(typeSoundMap, doc, particleSystemNode, "TypeSoundMap");
            node.AppendChild(particleSystemNode);
            return particleSystemNode;
        }
        public List<byte> GeneratePlayData()
        {
            var particleSystemBytes = new List<byte>();
            //orderType
            PlayDataHelper.GenerateStruct(orderType, particleSystemBytes);
            //stringDataField
            PlayDataHelper.GenerateStringDataFields(this, particleSystemBytes);
            //customTypes
            PlayDataHelper.GenerateObjectList(customTypes, particleSystemBytes);
            //layers
            PlayDataHelper.GenerateObjectList(layers, particleSystemBytes);
            //typeSoundMap
            particleSystemBytes.AddRange(BitConverter.GetBytes(typeSoundMap.Count));
            foreach (var typeSound in typeSoundMap)
            {
                particleSystemBytes.AddRange(BitConverter.GetBytes(typeSound.Key));
                particleSystemBytes.AddRange(BitConverter.GetBytes(typeSound.Value));
            }
            return PlayDataHelper.CreateBlock(particleSystemBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader particleSystemReader = PlayDataHelper.GetBlockReader(reader))
            {
                //orderType
                orderType = PlayDataHelper.ReadStruct<OrderType>(particleSystemReader);
                //stringDataFields
                PlayDataHelper.ReadStringDataFields(this, particleSystemReader);
                //customTypes
                PlayDataHelper.ReadObjectList(CustomTypes, particleSystemReader, version);
                //layers
                PlayDataHelper.ReadObjectList(Layers, particleSystemReader, version);
                for (int i = 0; i < Layers.Count; ++i)
                {
                    //Set id of layer to components of layer
                    Layers[i].SetComponentsID(i);
                }
                //typeSoundMap
                typeSoundMap = new Dictionary<int, int>();
                var count = particleSystemReader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    var key = particleSystemReader.ReadInt32();
                    var value = particleSystemReader.ReadInt32();
                    typeSoundMap.Add(key, value);
                }
            }
        }
        public Vector2 GetCenterPositionOrDefault()
        {
            for (int i = 0; i < Layers.Count; ++i)
            {
                var layer = Layers[i];
                for (int k = 0; k < layer.Components.Count; ++k)
                {
                    if (layer.Components[k] is Center && layer.Components[k].Name == File.DefaultCenterName)
                    {
                        return layer.Components[k].Position;
                    }
                }
            }
            return Vector2.Zero;
        }
        public bool Update(int currentFrame = 1)
        {
            if (currentFrame != CurrentFrame)
            {
                Reset();
                for (int i = 0; i < currentFrame; ++i) Update(i);
                CurrentFrame = currentFrame;
            }
            var centerPosition = GetCenterPositionOrDefault();
            for (int i = 0; i < ComponentTree.Count; ++i)
            {
                ComponentTree[i].CenterPosition = centerPosition;
                UpdateComponent(ComponentTree[i], CurrentFrame);
            }
            if (++CurrentFrame > TotalFrame)
            {
                Reset();
            }
            return true;
        }
        public void UpdateComponent(Component component, int currentFrame)
        {
            var layer = Layers[component.LayerID];
            if (layer.NeedUpdate(currentFrame)) component.Update(currentFrame);
            for (int i = 0; i < component.Children.Count; ++i)
            {
                UpdateComponent(component.Children[i], currentFrame);
            }
        }
        public void Reset()
        {
            CurrentFrame = 1;
            for (int i = 0; i < Layers.Count; ++i)
                Layers[i].Reset();
        }
        #endregion
    }
}
