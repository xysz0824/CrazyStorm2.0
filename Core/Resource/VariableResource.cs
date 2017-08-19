/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CrazyStorm.Core
{
    public class VariableResource : Resource
    {
        #region Private Members
        [PlayData]
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
        public VariableResource() { }
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
        public override object Clone()
        {
            return MemberwiseClone();
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var variableResourceNode = (XmlElement)node.SelectSingleNode("VariableResource");
            XmlHelper.BuildFromFields(typeof(VariableResource), this, variableResourceNode);
            return variableResourceNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var variableResourceNode = doc.CreateElement("VariableResource");
            XmlHelper.StoreFields(typeof(VariableResource), this, doc, variableResourceNode);
            node.AppendChild(variableResourceNode);
            return variableResourceNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var variableResourceBytes = new List<byte>();
            PlayDataHelper.GenerateFields(typeof(VariableResource), this, variableResourceBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(variableResourceBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader variableResourceReader = PlayDataHelper.GetBlockReader(reader))
            {
                value = variableResourceReader.ReadSingle();
            }
        }
        #endregion
    }
}
