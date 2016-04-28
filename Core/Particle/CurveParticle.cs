using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct CurveParticleData : IFieldData
    {
        public int length;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public class CurveParticle : PropertyContainer, IXmlData
    {
        #region Private Members
        ParticleType type;
        ParticleBaseData particleBaseData;
        CurveParticleData curveParticleData;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int MaxLife
        {
            get { return particleBaseData.maxLife; }
            set { particleBaseData.maxLife = value; }
        }
        public int CurrentFrame
        {
            get { return particleBaseData.currentFrame; }
            set { particleBaseData.currentFrame = value; }
        }
        public Vector2 Position
        {
            get { return particleBaseData.position; }
            set { particleBaseData.position = value; }
        }
        public ParticleType Type
        {
            get { return type; }
            set { type = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float WidthScale
        {
            get { return particleBaseData.widthScale; }
            set { particleBaseData.widthScale = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int Length
        {
            get { return curveParticleData.length; }
            set { curveParticleData.length = value; }
        }
        [RGBProperty]
        public RGB RGB
        {
            get { return particleBaseData.rgb; }
            set { particleBaseData.rgb = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Opacity
        {
            get { return particleBaseData.opacity; }
            set { particleBaseData.opacity = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PSpeed
        {
            get { return particleBaseData.pspeed; }
            set { particleBaseData.pspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PSpeedAngle
        {
            get { return particleBaseData.pspeedAngle; }
            set { particleBaseData.pspeedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PAcspeed
        {
            get { return particleBaseData.pacspeed; }
            set { particleBaseData.pacspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PAcspeedAngle
        {
            get { return particleBaseData.pacspeedAngle; }
            set { particleBaseData.pacspeedAngle = value; }
        }
        [EnumProperty(typeof(BlendType))]
        public BlendType BlendType
        {
            get { return particleBaseData.blendType; }
            set { particleBaseData.blendType = value; }
        }
        [BoolProperty]
        public bool KillOutside
        {
            get { return particleBaseData.killOutside; }
            set { particleBaseData.killOutside = value; }
        }
        [BoolProperty]
        public bool Collision
        {
            get { return particleBaseData.collision; }
            set { particleBaseData.collision = value; }
        }
        [BoolProperty]
        public bool IgnoreMask
        {
            get { return particleBaseData.ignoreMask; }
            set { particleBaseData.ignoreMask = value; }
        }
        [BoolProperty]
        public bool IgnoreRebound
        {
            get { return particleBaseData.ignoreRebound; }
            set { particleBaseData.ignoreRebound = value; }
        }
        [BoolProperty]
        public bool IgnoreForce
        {
            get { return particleBaseData.ignoreForce; }
            set { particleBaseData.ignoreForce = value; }
        }
        #endregion

        #region Constructor
        public CurveParticle()
        {
        }
        #endregion

        #region Public Methods
        public XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var curveParticleNode = doc.CreateElement("CurveParticle");
            //properties
            curveParticleNode.AppendChild(base.GetXmlElement(doc));
            //type
            if (type != null)
            {
                var typeAttribute = doc.CreateAttribute("type");
                typeAttribute.Value = type.ID.ToString();
                curveParticleNode.Attributes.Append(typeAttribute);
            }
            //particleBaseData
            XmlHelper.StoreStruct(particleBaseData, doc, curveParticleNode, "ParticleBaseData");
            //curveParticleData
            XmlHelper.StoreStruct(curveParticleData, doc, curveParticleNode, "CurveParticleData");
            node.AppendChild(curveParticleNode);
            return node;
        }
        #endregion
    }
}
