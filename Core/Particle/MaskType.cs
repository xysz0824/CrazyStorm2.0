/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MaskTypeData
    {
        public int id;
        public Vector2 startPoint;
        public int width;
        public int height;
        public int frames;
        public int delay;
    }
    public class MaskType : INotifyPropertyChanged, IXmlData, IGeneratePlayData, ILoadPlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        [XmlAttribute]
        [StringData]
        string name;
        MaskTypeData data;
        #endregion

        #region Public Members
        public int ID
        {
            get { return data.id; }
            set { data.id = value; }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (!StringUtil.IsNullOrWhiteSpace(value)) name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public Vector2 StartPoint
        {
            get { return data.startPoint; }
            set
            {
                data.startPoint = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPoint"));
            }
        }
        public float StartPointX
        {
            get { return data.startPoint.x; }
            set
            {
                data.startPoint.x = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPointX"));
            }
        }
        public float StartPointY
        {
            get { return data.startPoint.y; }
            set
            {
                data.startPoint.y = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPointY"));
            }
        }
        public int Width
        {
            get { return data.width; }
            set
            {
                data.width = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Width"));
            }
        }
        public int Height
        {
            get { return data.height; }
            set
            {
                data.height = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Height"));
            }
        }
        public int Frames
        {
            get { return data.frames; }
            set
            {
                data.frames = value >= 1 ? value : 1;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Frames"));
            }
        }
        public int Delay
        {
            get { return data.delay; }
            set
            {
                data.delay = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Delay"));
            }
        }
        #endregion

        #region Constructor
        public MaskType()
        {
            name = string.Empty;
            data.frames = 1;
        }
        public MaskType(int id) : this()
        {
            data.id = id;
        }
        public MaskType(int id, string name) : this(id)
        {
            this.name = name;
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            return Name;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "MaskType";
            var maskTypeNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName) maskTypeNode = node;
            XmlHelper.BuildFromFields(this, maskTypeNode);
            XmlHelper.BuildFromStruct(ref data, maskTypeNode);
            return maskTypeNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var maskTypeNode = doc.CreateElement("MaskType");
            XmlHelper.StoreFields(this, doc, maskTypeNode);
            XmlHelper.StoreStruct(data, doc, maskTypeNode);
            node.AppendChild(maskTypeNode);
            return maskTypeNode;
        }
        public List<byte> GeneratePlayData(File file)
        {
            var maskTypeBytes = new List<byte>();
            PlayDataHelper.GenerateStringDataFields(this, maskTypeBytes);
            PlayDataHelper.GenerateStruct(data, maskTypeBytes);
            return PlayDataHelper.CreateBlock(maskTypeBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader maskTypeReader = PlayDataHelper.GetBlockReader(reader))
            {
                PlayDataHelper.ReadStringDataFields(this, maskTypeReader);
                data = PlayDataHelper.ReadStruct<MaskTypeData>(maskTypeReader);
            }
        }
        #endregion
    }
}
