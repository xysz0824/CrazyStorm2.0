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
    public class MultiEmitter : Emitter
    {
        #region Private Members
        Particle particle;
        #endregion

        #region Public Members
        public Particle Particle { get { return particle; } }
        #endregion

        #region Constructor
        public MultiEmitter()
        {
            particle = new Particle();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as MultiEmitter;
            clone.particle = particle.Clone() as Particle;
            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var multiEmitterNode = (XmlElement)node.SelectSingleNode("MultiEmitter");
            particle.BuildFromXml(multiEmitterNode);
            return multiEmitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var multiEmitterNode = doc.CreateElement("MultiEmitter");
            particle.StoreAsXml(doc, multiEmitterNode);
            node.AppendChild(multiEmitterNode);
            return multiEmitterNode;
        }
        #endregion
    }
}
