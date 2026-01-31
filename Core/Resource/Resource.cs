/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
    public abstract class Resource : INotifyPropertyChanged, IXmlData, IGeneratePlayData, ILoadPlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [StringData]
        [XmlAttribute]
        private string label;
        protected bool isValid;
        public string Label
        {
            get { return label; }
            set
            {
                label = value;
                OnPropertyChanged("Label");
            }
        }
        public bool IsValid { get { CheckValid(); return isValid; } }

        public Resource() { }
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
            if (node.Name == nodeName) resourceNode = node;
            XmlHelper.BuildFromFields(this, resourceNode);
            return resourceNode;
        }

        public virtual XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var resourceNode = doc.CreateElement("Resource");
            XmlHelper.StoreFields(this, doc, resourceNode);
            node.AppendChild(resourceNode);
            return resourceNode;
        }

        public virtual List<byte> GeneratePlayData(File file)
        {
            var resourceBytes = new List<byte>();
            PlayDataHelper.GenerateStringDataFields(this, resourceBytes);
            return PlayDataHelper.CreateBlock(resourceBytes);
        }
        public virtual void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader resourceReader = PlayDataHelper.GetBlockReader(reader))
            {
                PlayDataHelper.ReadStringDataFields(this, resourceReader);
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
