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
    public struct VariableResourceData
    {
        public int id;
        public float value;
    }
    public class VariableResource : Resource
    {
        #region Private Members
        VariableResourceData data;
        #endregion

        #region Public Members
        public int ID
        {
            get { return data.id; }
            set
            {
                data.id = value;
                OnPropertyChanged("ID");
            }
        }
        public float Value 
        { 
            get { return data.value; }
            set 
            { 
                data.value = value;
                OnPropertyChanged("Value");
            }
        }
        #endregion

        #region Constructor
        public VariableResource() { }
        public VariableResource(int id, string label) : base(label) { }
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
        public override List<byte> GeneratePlayData(File file)
        {
            var bytes = base.GeneratePlayData(file);
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
