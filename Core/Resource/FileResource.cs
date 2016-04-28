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
    public class FileResource : Resource
    {
        #region Private Members
        [XmlAttribute]
        int id;
        [XmlAttribute]
        string absolutePath;
        string relativePath;
        #endregion

        #region Public Members
        public int ID
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
            relativePath = absolutePath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
        }
        #endregion

        #region Public Methods
        public override void CheckValid()
        {
            if (relativePath != absolutePath)
                absolutePath = AppDomain.CurrentDomain.BaseDirectory + relativePath;
            
            isValid = System.IO.File.Exists(absolutePath);
        }
        public override XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var fileResourceNode = doc.CreateElement("FileResource");
            XmlHelper.StoreFields(typeof(FileResource), this, doc, fileResourceNode);
            node.AppendChild(fileResourceNode);
            return node;
        }
        #endregion
    }
}
