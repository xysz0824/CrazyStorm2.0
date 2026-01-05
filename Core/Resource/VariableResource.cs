/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.CodeDom;

namespace CrazyStorm.Core
{
    public enum SpecialVariableType
    {
        None,
        BodyPositionX,
        BodyPositionY,
    }
    public struct VariableResourceData
    {
        public float value;
        public SpecialVariableType type;
    }
    public class VariableResource : Resource
    {
        #region Private Members
        VariableResourceData data;
        #endregion

        #region Public Members
        public float Value 
        { 
            get { return data.value; }
            set 
            { 
                data.value = value;
                OnPropertyChanged("Value");
            }
        }
        public SpecialVariableType Type
        {
            get { return data.type; }
            set { data.type = value; }
        }
        #endregion

        #region Constructor
        public VariableResource() { }
        public VariableResource(string label) : base(label) { }
        public VariableResource(string label, SpecialVariableType type)
            : this(label)
        {
            Type = type;
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
            XmlHelper.BuildFromFields(this, variableResourceNode);
            //variableResourceData
            XmlHelper.BuildFromStruct(ref data, variableResourceNode);
            return variableResourceNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var variableResourceNode = doc.CreateElement("VariableResource");
            XmlHelper.StoreFields(this, doc, variableResourceNode);
            //variableResourceData
            XmlHelper.StoreStruct(data, doc, variableResourceNode);
            node.AppendChild(variableResourceNode);
            return variableResourceNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var variableResourceBytes = new List<byte>();
            //variableResourceData
            PlayDataHelper.GenerateStruct(data, variableResourceBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(variableResourceBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader variableResourceReader = PlayDataHelper.GetBlockReader(reader))
            {
                //variableResourceData
                data = PlayDataHelper.ReadStruct<VariableResourceData>(variableResourceReader);
            }
        }
        #endregion
    }
}
