using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct ParticleBaseData : IFieldData
    {
        public int maxLife;
        public int currentFrame;
        public Vector2 position;
        public float widthScale;
        public RGB rgb;
        public float opacity;
        public float pspeed;
        public float pspeedAngle;
        public float pacspeed;
        public float pacspeedAngle;
        public BlendType blendType;
        public bool killOutside;
        public bool collision;
        public bool ignoreMask;
        public bool ignoreRebound;
        public bool ignoreForce;
        public bool fogEffect;
        public bool fadeEffect;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public struct ParticleData : IFieldData
    {
        public float heightScale;
        public float rotation;
        public bool stickToSpeedAngle;
        public bool retainScale;
        public bool afterimageEffect;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public class Particle : PropertyContainer, IXmlData, IRebuildReference<ParticleType>
    {
        #region Private Members
        ParticleType type;
        int typeID = -1;
        ParticleBaseData particleBaseData;
        ParticleData particleData;
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
        public float X
        {
            get { return particleBaseData.position.x; }
            set { particleBaseData.position.x = value; }
        }
        public float Y
        {
            get { return particleBaseData.position.y; }
            set { particleBaseData.position.y = value; }
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
        [FloatProperty(0, float.MaxValue)]
        public float HeightScale
        {
            get { return particleData.heightScale; }
            set { particleData.heightScale = value; }
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
        public float Rotation
        {
            get { return particleData.rotation; }
            set { particleData.rotation = value; }
        }
        [BoolProperty]
        public bool StickToSpeedAngle
        {
            get { return particleData.stickToSpeedAngle; }
            set { particleData.stickToSpeedAngle = value; }
        }
        [BoolProperty]
        public bool RetainScale
        {
            get { return particleData.retainScale; }
            set { particleData.retainScale = value; }
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
        [BoolProperty]
        public bool FogEffect
        {
            get { return particleBaseData.fogEffect; }
            set { particleBaseData.fogEffect = value; }
        }
        [BoolProperty]
        public bool FadeEffect
        {
            get { return particleBaseData.fadeEffect; }
            set { particleBaseData.fadeEffect = value; }
        }
        [BoolProperty]
        public bool AfterimageEffect
        {
            get { return particleData.afterimageEffect; }
            set { particleData.afterimageEffect = value; }
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
        public Particle()
        {
        }
        #endregion

        #region Public Methods
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Particle";
            var particleNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                particleNode = node;

            //properties
            base.BuildFromXmlElement(particleNode);
            //type
            if (particleNode.HasAttribute("type"))
            {
                string typeAttribute = particleNode.GetAttribute("type");
                int parsedID;
                if (int.TryParse(typeAttribute, out parsedID))
                    typeID = parsedID;
                else
                    throw new System.IO.FileLoadException("FileLoadError");
            }
            //particleBaseData
            XmlHelper.BuildStruct(ref particleBaseData, particleNode, "ParticleBaseData");
            //particleData
            XmlHelper.BuildStruct(ref particleData, particleNode, "ParticleData");
            return particleNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var particleNode = doc.CreateElement("Particle");
            //properties
            particleNode.AppendChild(base.GetXmlElement(doc));
            //type
            if (type != null)
            {
                var typeAttribute = doc.CreateAttribute("type");
                typeAttribute.Value = type.ID.ToString();
                particleNode.Attributes.Append(typeAttribute);
            }
            //particleBaseData
            XmlHelper.StoreStruct(particleBaseData, doc, particleNode, "ParticleBaseData");
            //particleData
            XmlHelper.StoreStruct(particleData, doc, particleNode, "ParticleData");
            node.AppendChild(particleNode);
            return particleNode;
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
