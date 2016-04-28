/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class VariableResource : Resource, ICloneable
    {
        #region Private Members
        [XmlAttribute]
        float value;
        #endregion

        #region Public Members
        public float Value 
        { 
            get { return value; }
            set { this.value = value; }
        }
        #endregion

        #region Constructor
        public VariableResource(string label)
            : base(label)
        {
        }
        #endregion

        #region Public Methods
        public override void CheckValid()
        {
            isValid = true;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public override XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var variableResourceNode = doc.CreateElement("VariableResource");
            XmlHelper.StoreFields(typeof(VariableResource), this, doc, variableResourceNode);
            node.AppendChild(variableResourceNode);
            return node;
        }
        #endregion
    }
}
