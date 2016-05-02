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
        public CurveEmitter()
        {
            particle = new CurveParticle();
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var curveEmitterNode = (XmlElement)node.SelectSingleNode("CurveEmitter");
            return curveEmitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var curveEmitterNode = doc.CreateElement("CurveEmitter");
            node.AppendChild(curveEmitterNode);
            return curveEmitterNode;
        }
    }
}
