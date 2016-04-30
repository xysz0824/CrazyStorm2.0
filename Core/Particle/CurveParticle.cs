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
    public class CurveParticle : PropertyContainer, IXmlData, IRebuildReference<ParticleType>
    {
        #region Private Members
        ParticleType type;
        int typeID = -1;
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
            particleBaseData.maxLife = 200;
            particleBaseData.widthScale = 1;
            particleBaseData.rgb = new RGB(255, 255, 255);
            particleBaseData.opacity = 100;
            particleBaseData.pspeed = 5;
            particleBaseData.killOutside = true;
            particleBaseData.collision = true;
            particleBaseData.fogEffect = true;
            particleBaseData.fadeEffect = true;
            curveParticleData.length = 10;
        }
        #endregion

        #region Public Methods
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "CurveParticle";
            var curveParticleNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                curveParticleNode = node;

            //properties
            base.BuildFromXmlElement(curveParticleNode);
            //type
            if (curveParticleNode.HasAttribute("type"))
            {
                string typeAttribute = curveParticleNode.GetAttribute("type");
                int parsedID;
                if (int.TryParse(typeAttribute, out parsedID))
                    typeID = parsedID;
                else
                    throw new System.IO.FileLoadException("FileLoadError");
            }
            //particleBaseData
            XmlHelper.BuildStruct(ref particleBaseData, curveParticleNode, "ParticleBaseData");
            //curveParticleData
            XmlHelper.BuildStruct(ref curveParticleData, curveParticleNode, "CurveParticleData");
            return curveParticleNode;
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
            return curveParticleNode;
        }
        public void RebuildReferenceFromCollection(IList<ParticleType> collection)
        {
            //type
            if (typeID != -1)
            {
                foreach (var target in collection)
                {
                    if (typeID == target.ID)
                    {
                        type = target;
                        break;
                    }
                }
                typeID = -1;
            }
        }
        #endregion
    }
}
