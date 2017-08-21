/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
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
        Gray
    }
    public class Layer : INotifyPropertyChanged, IXmlData, IGeneratePlayData, ILoadPlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        [PlayData]
        [XmlAttribute]
        string name;
        [PlayData]
        [XmlAttribute]
        bool visible;
        [XmlAttribute]
        LayerColor color;
        [PlayData]
        [XmlAttribute]
        int beginFrame;
        [PlayData]
        [XmlAttribute]
        int totalFrame;
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
            get { return visible; }
            set 
            { 
                visible = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Visible"));
            }
        }
        public LayerColor Color 
        { 
            get { return color; }
            set 
            { 
                color = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }
        public int BeginFrame 
        { 
            get { return beginFrame; }
            set 
            { 
                beginFrame = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("BeginFrame"));
            }
        }
        public int TotalFrame
        {
            get { return totalFrame; }
            set 
            {
                totalFrame = value > 0 ? value : 1;
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
            visible = true;
            totalFrame = 200;
            components = new GenericContainer<Component>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as Layer;
            clone.components = new GenericContainer<Component>();
            foreach (var component in components)
                clone.components.Add(component.Clone() as Component);
            
            return clone;
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Layer";
            var layerNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                layerNode = node;

            XmlHelper.BuildFromFields(this, layerNode);
            //components
            var componentsNode = layerNode.SelectSingleNode("Components");
            if (componentsNode == null)
                throw new System.IO.FileLoadException("FileDataError");

            foreach (XmlElement componentNode in componentsNode.ChildNodes)
            {
                string specificType = componentNode.GetAttribute("specificType");
                if (XmlHelper.FindNode(componentNode, specificType) != null)
                {
                    Component component = ComponentFactory.Create(specificType);
                    component.BuildFromXml(componentNode);
                    components.Add(component);
                }
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
            return layerNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var layerNode = doc.CreateElement("Layer");
            XmlHelper.StoreFields(this, doc, layerNode);
            //components
            XmlHelper.StoreObjectList(components, doc, layerNode, "Components");
            node.AppendChild(layerNode);
            return layerNode;
        }
        public List<byte> GeneratePlayData()
        {
            var layerBytes = new List<byte>();
            PlayDataHelper.GenerateFields(this, layerBytes);
            //components
            PlayDataHelper.GenerateObjectList(components, layerBytes);
            return PlayDataHelper.CreateBlock(layerBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader layerReader = PlayDataHelper.GetBlockReader(reader))
            {
                Name = PlayDataHelper.ReadString(layerReader);
                Visible = layerReader.ReadBoolean();
                BeginFrame = layerReader.ReadInt32();
                TotalFrame = layerReader.ReadInt32();
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
                            component.LayerName = Name;
                            Components.Add(component);
                        }
                    }
                }
            }
        }
        public bool Update(int currentFrame)
        {
            if (Visible)
            {
                if (currentFrame < BeginFrame || currentFrame >= BeginFrame + TotalFrame)
                    return false;

                for (int i = 0; i < Components.Count; ++i)
                    Components[i].Update(currentFrame - BeginFrame);
            }
            return Visible;
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
