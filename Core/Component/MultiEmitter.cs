/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class MultiEmitterPool : PoolObject<MultiEmitterPool, NullData>
    {
        public MultiEmitter Instance { get; private set; }
        public MultiEmitterPool()
        {
            Instance = new MultiEmitter();
            Instance.PoolObject = this;
        }
    }
    public class MultiEmitter : Emitter
    {
        public MultiEmitterPool PoolObject { get; set; }
        public MultiEmitter()
        {
            InitialTemplate = new Particle();
            Template = new Particle();
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
        public override Component Instantiate()
        {
            return MultiEmitterPool.Rent(NullData.Empty).Instance;
        }
        public override void Destroy()
        {
            MultiEmitterPool.Return(PoolObject);
        }
    }
}
