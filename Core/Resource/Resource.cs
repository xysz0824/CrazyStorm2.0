/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public abstract class Resource : INotifyPropertyChanged, IXmlData, IPlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [PlayData]
        [XmlAttribute]
        private string label;
        protected bool isValid;
        public string Label
        {
            get { return label; }
            set
            {
                label = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Label"));
            }
        }
        public bool IsValid { get { CheckValid(); return isValid; } }

        public Resource(string label)
        {
            this.label = label;
        }

        public override string ToString()
        {
            return label;
        }

        public abstract void CheckValid();

        public abstract object Clone();

        public virtual XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Resource";
            var resourceNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                resourceNode = node;

            XmlHelper.BuildFromFields(typeof(Resource), this, resourceNode);
            return resourceNode;
        }

        public virtual XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var resourceNode = doc.CreateElement("Resource");
            XmlHelper.StoreFields(typeof(Resource), this, doc, resourceNode);
            node.AppendChild(resourceNode);
            return resourceNode;
        }

        public virtual List<byte> GeneratePlayData()
        {
            var resourceBytes = new List<byte>();
            PlayDataHelper.GenerateFields(typeof(Resource), this, resourceBytes);
            return PlayDataHelper.CreateBlock(resourceBytes);
        }
    }
}
