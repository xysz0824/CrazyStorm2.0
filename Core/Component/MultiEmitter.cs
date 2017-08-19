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
    public class MultiEmitter : Emitter
    {
        public MultiEmitter()
        {
            particle = new Particle();
            InitialTemplate = new Particle();
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var multiEmitterNode = (XmlElement)node.SelectSingleNode("MultiEmitter");
            return multiEmitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var multiEmitterNode = doc.CreateElement("MultiEmitter");
            node.AppendChild(multiEmitterNode);
            return multiEmitterNode;
        }
    }
}
