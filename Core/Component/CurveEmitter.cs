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
    public class CurveEmitter : Emitter
    {
        #region Private Members
        CurveParticle curveParticle;
        #endregion

        #region Public Members
        public CurveParticle CurveParticle { get { return curveParticle; } }
        #endregion

        #region Constructor
        public CurveEmitter()
        {
            curveParticle = new CurveParticle();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as CurveEmitter;
            clone.curveParticle = curveParticle.Clone() as CurveParticle;
            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var curveEmitterNode = (XmlElement)node.SelectSingleNode("CurveEmitter");
            curveParticle.BuildFromXml(curveEmitterNode);
            return curveEmitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var curveEmitterNode = doc.CreateElement("CurveEmitter");
            curveParticle.StoreAsXml(doc, curveEmitterNode);
            node.AppendChild(curveEmitterNode);
            return curveEmitterNode;
        }
        #endregion
    }
}
