/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public enum ForceType
    {
        Direction,
        Inner,
        Outer
    }
    public struct ForceFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public FieldShape shape;
        public Reach reach;
        public string targetName;
        public float force;
        public float direction;
        public ForceType forceType;
    }
    public class ForceField : Component
    {
        #region Private Members
        ForceFieldData forceFieldData;
        #endregion

        #region Public Members
        [FloatProperty(0, float.MaxValue)]
        public float HalfWidth
        {
            get { return forceFieldData.halfWidth; }
            set { forceFieldData.halfWidth = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float HalfHeight
        {
            get { return forceFieldData.halfHeight; }
            set { forceFieldData.halfHeight = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Force
        {
            get { return forceFieldData.force; }
            set { forceFieldData.force = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Direction
        {
            get { return forceFieldData.direction; }
            set { forceFieldData.direction = value; }
        }
        [EnumProperty(typeof(ForceType))]
        public ForceType ForceType
        {
            get { return forceFieldData.forceType; }
            set { forceFieldData.forceType = value; }
        }
        [EnumProperty(typeof(FieldShape))]
        public FieldShape Shape
        {
            get { return forceFieldData.shape; }
            set { forceFieldData.shape = value; }
        }
        [EnumProperty(typeof(Reach))]
        public Reach Reach
        {
            get { return forceFieldData.reach; }
            set { forceFieldData.reach = value; }
        }
        [StringProperty(1, 15, true, true, false, false)]
        public string TargetName
        {
            get { return forceFieldData.targetName; }
            set { forceFieldData.targetName = value; }
        }
        #endregion

        #region Constructor
        public ForceField()
        {
            forceFieldData.targetName = string.Empty;
            forceFieldData.halfWidth = 50;
            forceFieldData.halfHeight = 50;
            forceFieldData.force = 0.1f;
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var forceFieldNode = (XmlElement)node.SelectSingleNode("ForceField");
            //forceFieldData
            XmlHelper.BuildFromStruct(ref forceFieldData, forceFieldNode, "ForceFieldData");
            return forceFieldNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var forceFieldNode = doc.CreateElement("ForceField");
            //forceFieldData
            XmlHelper.StoreStruct(forceFieldData, doc, forceFieldNode, "ForceFieldData");
            node.AppendChild(forceFieldNode);
            return forceFieldNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var forceFieldBytes = new List<byte>();
            //forceFieldData
            PlayDataHelper.GenerateStruct(forceFieldData, forceFieldBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(forceFieldBytes));
            return bytes;
        }
        #endregion
    }
}
