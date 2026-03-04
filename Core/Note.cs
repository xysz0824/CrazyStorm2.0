/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class Note : IXmlData
    {
        public const float DefaultWidth = 80;
        public const float DefaultHeight = 48;

        #region Private Members
        [StringData]
        [XmlAttribute]
        string comment;
        [XmlAttribute]
        float x;
        [XmlAttribute]
        float y;
        [XmlAttribute]
        float width;
        [XmlAttribute]
        float height;
        [XmlAttribute]
        LayerColor color;
        bool selected;
        #endregion

        #region Public Members
        public string Comment
        {
            get { return comment; }
            set { comment = value ?? string.Empty; }
        }
        public float X
        {
            get { return x; }
            set { x = value; }
        }
        public float Y
        {
            get { return y; }
            set { y = value; }
        }
        public float Width
        {
            get { return width; }
            set { width = value < DefaultWidth ? DefaultWidth : value; }
        }
        public float Height
        {
            get { return height; }
            set { height = value < DefaultHeight ? DefaultHeight : value; }
        }
        public LayerColor Color
        {
            get { return color; }
            set { color = value; }
        }
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
        #endregion

        #region Constructor
        public Note()
        {
            comment = string.Empty;
            width = DefaultWidth;
            height = DefaultHeight;
            color = LayerColor.Yellow;
            selected = false;
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            return MemberwiseClone();
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Note";
            var noteNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName) noteNode = node;
            XmlHelper.BuildFromFields(this, noteNode);
            return noteNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var noteNode = doc.CreateElement("Note");
            XmlHelper.StoreFields(this, doc, noteNode);
            node.AppendChild(noteNode);
            return noteNode;
        }
        #endregion
    }
}
