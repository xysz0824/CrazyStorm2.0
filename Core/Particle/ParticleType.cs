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
    public enum ParticleColor
    {
        None,
        Red,
        Purple,
        Blue,
        Cyan,
        Green,
        Yellow,
        Orange,
        Gray
    }
    public class ParticleType : INotifyPropertyChanged, IXmlData, IRebuildReference<FileResource>, IPlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        FileResource image;
        int imageID = -1;
        [PlayData]
        [XmlAttribute]
        int id;
        [PlayData]
        [XmlAttribute]
        string name;
        [PlayData]
        [XmlAttribute]
        Vector2 startPoint;
        [PlayData]
        [XmlAttribute]
        int width;
        [PlayData]
        [XmlAttribute]
        int height;
        [PlayData]
        [XmlAttribute]
        Vector2 centerPoint;
        [PlayData]
        [XmlAttribute]
        int frames;
        [PlayData]
        [XmlAttribute]
        int delay;
        [PlayData]
        [XmlAttribute]
        int radius;
        [PlayData]
        [XmlAttribute]
        ParticleColor color;
        #endregion

        #region Public Members
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public FileResource Image
        {
            get { return image; }
            set
            {
                image = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Image"));
            }
        }
        public Vector2 StartPoint
        {
            get { return startPoint; }
            set
            {
                startPoint = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPoint"));
            }
        }
        public float StartPointX
        {
            get { return startPoint.x; }
            set
            {
                startPoint.x = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPointX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxX"));
                }
            }
        }
        public float StartPointY
        {
            get { return startPoint.y; }
            set
            {
                startPoint.y = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPointY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxY"));
                }
            }
        }
        public int Width
        {
            get { return width; }
            set
            {
                width = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Width"));
            }
        }
        public int Height
        {
            get { return height; }
            set
            {
                height = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Height"));
            }
        }
        public Vector2 CenterPoint
        {
            get { return centerPoint; }
            set
            {
                centerPoint = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterPoint"));
            }
        }
        public float CenterPointX
        {
            get { return centerPoint.x; }
            set
            {
                centerPoint.x = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterPointX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxX"));
                }
            }
        }
        public float CircleBoxX
        {
            get { return StartPointX + CenterPointX - Radius; }
        }
        public float CenterPointY
        {
            get { return centerPoint.y; }
            set
            {
                centerPoint.y = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterPointY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxY"));
                }
            }
        }
        public float CircleBoxY
        {
            get { return StartPointY + CenterPointY - Radius; }
        }
        public int Frames
        {
            get { return frames; }
            set
            {
                frames = value >= 1 ? value : 1;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Frames"));
            }
        }
        public int Delay
        {
            get { return delay; }
            set
            {
                delay = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Delay"));
            }
        }
        public int Radius
        {
            get { return radius; }
            set
            {
                radius = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Radius"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Diameter"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxY"));
                }
            }
        }
        public int Diameter { get { return radius * 2; }}
        public ParticleColor Color
        {
            get { return color; }
            set
            {
                color = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }
        #endregion

        #region Constructor
        public ParticleType(int id)
        {
            this.id = id;
            name = "ParticleType" + id;
            startPoint = Vector2.Zero;
            centerPoint = Vector2.Zero;
            frames = 1;
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
            var nodeName = "ParticleType";
            var particleTypeNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                particleTypeNode = node;

            if (particleTypeNode.HasAttribute("image"))
            {
                string fileResourceAttribute = particleTypeNode.GetAttribute("image");
                int parsedID;
                if (int.TryParse(fileResourceAttribute, out parsedID))
                    imageID = parsedID;
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
            XmlHelper.BuildFromFields(this, particleTypeNode);
            return particleTypeNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var particleTypeNode = doc.CreateElement("ParticleType");
            if (image != null)
            {
                var fileResourceAttribute = doc.CreateAttribute("image");
                fileResourceAttribute.Value = image.ID.ToString();
                particleTypeNode.Attributes.Append(fileResourceAttribute);
            }
            XmlHelper.StoreFields(this, doc, particleTypeNode);
            node.AppendChild(particleTypeNode);
            return particleTypeNode;
        }
        public void RebuildReferenceFromCollection(IList<FileResource> collection)
        {
            //image
            if (imageID != -1)
            {
                foreach (var target in collection)
                {
                    if (imageID == target.ID)
                    {
                        image = target;
                        break;
                    }
                }
                imageID = -1;
            }
        }
        public List<byte> GeneratePlayData()
        {
            var particleTypeBytes = new List<byte>();
            if (image != null)
                particleTypeBytes.AddRange(PlayDataHelper.GetBytes(image.ID));
            else
                particleTypeBytes.AddRange(PlayDataHelper.GetBytes(-1));

            PlayDataHelper.GenerateFields(this, particleTypeBytes);
            return PlayDataHelper.CreateTrunk(particleTypeBytes);
        }
        #endregion
    }
}
