/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public abstract class Resource : INotifyPropertyChanged, IXmlData
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [XmlAttribute]
        protected string label;
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

        public virtual XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }

        public virtual XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var resourceNode = doc.CreateElement("Resource");
            XmlHelper.StoreFields(typeof(Resource), this, doc, resourceNode);
            node.AppendChild(resourceNode);
            return resourceNode;
        }
    }
}
