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
    public struct ForceFieldData : IFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public Shape shape;
        public Reach reach;
        public string targetName;
        public float intensity;
        public float directionAngle;
        public ForceType forceType;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
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
        [EnumProperty(typeof(Shape))]
        public Shape Shape
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
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Intensity
        {
            get { return forceFieldData.intensity; }
            set { forceFieldData.intensity = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float DirectionAngle
        {
            get { return forceFieldData.directionAngle; }
            set { forceFieldData.directionAngle = value; }
        }
        [EnumProperty(typeof(ForceType))]
        public ForceType ForceType
        {
            get { return forceFieldData.forceType; }
            set { forceFieldData.forceType = value; }
        }
        #endregion

        #region Constructor
        public ForceField()
        {
            forceFieldData.targetName = "";
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var forceFieldNode = doc.CreateElement("ForveField");
            //forceFieldData
            XmlHelper.StoreStruct(forceFieldData, doc, forceFieldNode, "ForceFieldData");
            node.AppendChild(forceFieldNode);
            return forceFieldNode;
        }
        #endregion
    }
}
