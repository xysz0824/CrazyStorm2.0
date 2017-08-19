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
    public class FileResource : Resource
    {
        #region Private Members
        [PlayData]
        [XmlAttribute]
        int id;
        string absolutePath;
        [PlayData]
        [XmlAttribute]
        string relativePath;
        #endregion

        #region Public Members
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public string AbsolutePath { get { return absolutePath; } }
        public string RelatviePath { get { return relativePath; } }
        #endregion

        #region Constructor
        public FileResource(int id, string label, string absolutePath)
            : base(label)
        {
            this.id = id;
            this.absolutePath = absolutePath;
            relativePath = absolutePath;
        }
        #endregion

        #region Public Methods
        public override void CheckValid()
        {
            if (!StringUtil.IsNullOrWhiteSpace(File.CurrentDirectory))
                relativePath = relativePath.Replace(File.CurrentDirectory, "");
            
            if (relativePath.Contains(":"))
                absolutePath = relativePath;
            else
                absolutePath = File.CurrentDirectory + relativePath;
            
            isValid = System.IO.File.Exists(absolutePath);
        }
        public override object Clone()
        {
            return MemberwiseClone();
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var fileResourceNode = (XmlElement)node.SelectSingleNode("FileResource");
            XmlHelper.BuildFromFields(typeof(FileResource), this, fileResourceNode);
            return fileResourceNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var fileResourceNode = doc.CreateElement("FileResource");
            XmlHelper.StoreFields(typeof(FileResource), this, doc, fileResourceNode);
            node.AppendChild(fileResourceNode);
            return fileResourceNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var fileResourceBytes = new List<byte>();
            PlayDataHelper.GenerateFields(typeof(FileResource), this, fileResourceBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(fileResourceBytes));
            return bytes;
        }
        #endregion
    }
}
