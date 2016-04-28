/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

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
    public class Layer : INotifyPropertyChanged, ICloneable, IXmlData
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        [XmlAttribute]
        string name;
        [XmlAttribute]
        bool visible;
        [XmlAttribute]
        LayerColor color;
        [XmlAttribute]
        int beginFrame;
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
        public Layer(string name)
        {
            this.name = name;
            visible = true;
            color = LayerColor.Blue;
            beginFrame = 0;
            totalFrame = 200;
            components = new ObservableCollection<Component>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as Layer;
            clone.components = new ObservableCollection<Component>();
            foreach (var component in components)
                clone.components.Add(component.Clone() as Component);
            
            return clone;
        }
        public XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var layerNode = doc.CreateElement("Layer");
            XmlHelper.StoreFields(this, doc, layerNode);
            //components
            XmlHelper.StoreObjectList(components, doc, layerNode, "Components");
            node.AppendChild(layerNode);
            return node;
        }
        #endregion
    }
}
