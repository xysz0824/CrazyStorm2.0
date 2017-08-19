/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct CurveParticleData
    {
        public int length;
    }
    public class CurveParticle : ParticleBase
    {
        #region Private Members
        CurveParticleData curveParticleData;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int Length
        {
            get { return curveParticleData.length; }
            set { curveParticleData.length = value; }
        }
        #endregion

        #region Constructor
        public CurveParticle()
        {
            curveParticleData.length = 10;
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var curveParticleNode = (XmlElement)node.SelectSingleNode("CurveParticle");
            //curveParticleData
            XmlHelper.BuildFromStruct(ref curveParticleData, curveParticleNode, "CurveParticleData");
            return curveParticleNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var curveParticleNode = doc.CreateElement("CurveParticle");
            //curveParticleData
            XmlHelper.StoreStruct(curveParticleData, doc, curveParticleNode, "CurveParticleData");
            node.AppendChild(curveParticleNode);
            return curveParticleNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var curveParticleBytes = new List<byte>();
            //curveParticleData
            PlayDataHelper.GenerateStruct(curveParticleData, curveParticleBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(curveParticleBytes));
            return bytes;
        }
        #endregion
    }
}
