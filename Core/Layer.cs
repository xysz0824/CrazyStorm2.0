/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CrazyStorm.Core
{
    public enum LayerColor
    {
        Blue,
        Purple,
        Red,
        Green,
        Yellow,
        Orange,
        Pink,
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LayerData
    {
        public bool visible;
        public LayerColor color;
        public int beginFrame;
        public int totalFrame;
    }
    public class Layer : INotifyPropertyChanged, IXmlData, IGeneratePlayData, ILoadPlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        [StringData]
        [XmlAttribute]
        string name;
        LayerData layerData;
        IList<Component> components;
        #endregion

        #region Public Members
        public string Name
        {
            get { return name; }
            set 
            { 
                name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public bool Visible
        {
            get { return layerData.visible; }
            set 
            { 
                layerData.visible = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Visible"));
            }
        }
        public LayerColor Color 
        { 
            get { return layerData.color; }
            set 
            { 
                layerData.color = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }
        public int BeginFrame 
        { 
            get { return layerData.beginFrame; }
            set 
            { 
                layerData.beginFrame = value > 0 ? value : 1;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("BeginFrame"));
            }
        }
        public int TotalFrame
        {
            get { return layerData.totalFrame; }
            set 
            {
                layerData.totalFrame = value > 0 ? value : 1;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("TotalFrame"));
            }
        }
        public IList<Component> Components { get { return components; } }
        #endregion

        #region Constructor
        public Layer() 
        {
            components = new GenericContainer<Component>();
        }
        public Layer(string name)
        {
            this.name = name;
            layerData.visible = true;
            layerData.beginFrame = 1;
            layerData.totalFrame = 200;
            components = new GenericContainer<Component>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as Layer;
            clone.components = new GenericContainer<Component>();
            foreach (var component in components) clone.components.Add(component.Clone() as Component);
            return clone;
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Layer";
            var layerNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName) layerNode = node;
            XmlHelper.BuildFromFields(this, layerNode);
            //layerData
            XmlHelper.BuildFromStruct(ref layerData, layerNode);
            //components
            var componentsNode = layerNode.SelectSingleNode("Components");
            if (componentsNode == null) throw new System.IO.FileLoadException("FileDataError");
            foreach (XmlElement componentNode in componentsNode.ChildNodes)
            {
                string specificType = componentNode.GetAttribute("specificType");
                if (!string.IsNullOrEmpty(specificType))
                {
                    Component component = ComponentFactory.Create(specificType);
                    component.BuildFromXml(componentNode);
                    components.Add(component);
                }
                else throw new System.IO.FileLoadException("FileDataError");
            }
            return layerNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var layerNode = doc.CreateElement("Layer");
            XmlHelper.StoreFields(this, doc, layerNode);
            //layerData
            XmlHelper.StoreStruct(layerData, doc, layerNode);
            //components
            XmlHelper.StoreObjectList(components, doc, layerNode, "Components");
            node.AppendChild(layerNode);
            return layerNode;
        }
        public List<byte> GeneratePlayData(File file)
        {
            var layerBytes = new List<byte>();
            //stringDataFields
            PlayDataHelper.GenerateStringDataFields(this, layerBytes);
            //layerData
            PlayDataHelper.GenerateStruct(layerData, layerBytes);
            //components
            PlayDataHelper.GenerateObjectList(file, components, layerBytes);
            return PlayDataHelper.CreateBlock(layerBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader layerReader = PlayDataHelper.GetBlockReader(reader))
            {
                //stringDataFields
                PlayDataHelper.ReadStringDataFields(this, layerReader);
                //layerData
                layerData = PlayDataHelper.ReadStruct<LayerData>(layerReader);
                //components
                using (BinaryReader componentsReader = PlayDataHelper.GetBlockReader(layerReader))
                {
                    while (!PlayDataHelper.EndOfReader(componentsReader))
                    {
                        long startPosition = componentsReader.BaseStream.Position;
                        using (BinaryReader componentReader = PlayDataHelper.GetBlockReader(componentsReader))
                        {
                            string specificType = PlayDataHelper.ReadString(componentReader);
                            Component component = ComponentFactory.Create(specificType);
                            //Back to start position of components block.
                            componentsReader.BaseStream.Position = startPosition;
                            component.LoadPlayData(componentsReader, version);
                            Components.Add(component);
                        }
                    }
                }
            }
        }
        public void SetComponentsID(int id)
        {
            for (int i = 0; i < Components.Count; ++i)
            {
                components[i].LayerID = id;
                components[i].LayerName = name;
            }
        }
        public bool NeedUpdate(float currentFrame)
        {
            if (!Visible) return false;
            if (currentFrame < BeginFrame || currentFrame >= BeginFrame + TotalFrame) return false;
            return true;
        }
        public void Reset()
        {
            if (Visible)
            {
                for (int i = 0; i < Components.Count; ++i)
                    Components[i].Reset();
            }
        }
        #endregion
    }
}
