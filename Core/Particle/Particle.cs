using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct ParticleData
    {
        public bool stickToSpeedAngle;
        public float heightScale;
        public bool retainScale;
        public bool afterimageEffect;
    }
    public class Particle : ParticleBase
    {
        #region Private Members
        ParticleData particleData;
        #endregion

        #region Public Members
        [BoolProperty]
        public bool StickToSpeedAngle
        {
            get { return particleData.stickToSpeedAngle; }
            set { particleData.stickToSpeedAngle = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float HeightScale
        {
            get { return particleData.heightScale; }
            set { particleData.heightScale = value; }
        }
        [BoolProperty]
        public bool RetainScale
        {
            get { return particleData.retainScale; }
            set { particleData.retainScale = value; }
        }
        [BoolProperty]
        public bool AfterimageEffect
        {
            get { return particleData.afterimageEffect; }
            set { particleData.afterimageEffect = value; }
        }
        #endregion

        #region Constructor
        public Particle()
        {
            particleData.heightScale = 1;
            particleData.retainScale = true;
            particleData.stickToSpeedAngle = true;
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var particleNode = (XmlElement)node.SelectSingleNode("Particle");
            //particleData
            XmlHelper.BuildFromStruct(ref particleData, particleNode, "ParticleData");
            return particleNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var particleNode = doc.CreateElement("Particle");
            //particleData
            XmlHelper.StoreStruct(particleData, doc, particleNode, "ParticleData");
            node.AppendChild(particleNode);
            return particleNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var particleBytes = new List<byte>();
            //particleData
            PlayDataHelper.GenerateStruct(particleData, particleBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(particleBytes));
            return bytes;
        }
        #endregion
    }
}
