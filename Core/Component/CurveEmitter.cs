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
    public class CurveEmitterPool : PoolObject<CurveEmitterPool, NullData>
    {
        public CurveEmitter Instance { get; private set; }
        public CurveEmitterPool()
        {
            Instance = new CurveEmitter();
            Instance.PoolObject = this;
        }
    }
    public class CurveEmitter : Emitter
    {
        public CurveEmitterPool PoolObject { get; set; }
        public CurveEmitter()
        {
            InitialTemplate = new CurveParticle();
            Template = new CurveParticle();
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
        public override Component Instantiate()
        {
            return CurveEmitterPool.Rent(NullData.Empty).Instance;
        }
        public override void Destroy()
        {
            CurveEmitterPool.Return(PoolObject);
        }
    }
}
