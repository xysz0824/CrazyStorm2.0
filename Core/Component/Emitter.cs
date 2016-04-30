/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct EmitterData : IFieldData
    {
        public Vector2 emitPosition;
        public int emitCount;
        public int emitCycle;
        public float emitAngle;
        public float emitRange;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public class Emitter : Component
    {
        #region Private Members
        EmitterData emitterData;
        IList<EventGroup> particleEventGroups;
        #endregion

        #region Public Members
        [Vector2Property]
        public Vector2 EmitPosition
        {
            get { return emitterData.emitPosition; }
            set { emitterData.emitPosition = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int EmitCount
        {
            get { return emitterData.emitCount; }
            set { emitterData.emitCount = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int EmitCycle
        {
            get { return emitterData.emitCycle; }
            set { emitterData.emitCycle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitAngle
        {
            get { return emitterData.emitAngle; }
            set { emitterData.emitAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitRange
        {
            get { return emitterData.emitRange; }
            set { emitterData.emitRange = value; }
        }
        public IList<EventGroup> ParticleEventGroups { get { return particleEventGroups; } }
        #endregion

        #region Constructor
        public Emitter()
        {
            Properties["EmitPosition"] = new PropertyValue
            {
                Expression = true,
                Value = "[Position.x,Position.y]"
            };
            emitterData.emitCount = 1;
            emitterData.emitCycle = 10;
            particleEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Emitter;
            clone.particleEventGroups = new ObservableCollection<EventGroup>();
            foreach (var item in particleEventGroups)
                clone.particleEventGroups.Add(item.Clone() as EventGroup);

            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var emitterNode = (XmlElement)node.SelectSingleNode("Emitter");
            //emitterData
            XmlHelper.BuildStruct(ref emitterData, emitterNode, "EmitterData");
            //particleEventGroups
            XmlHelper.BuildObjectList(particleEventGroups, new EventGroup(), emitterNode, "ParticleEventGroups");
            return emitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var emitterNode = doc.CreateElement("Emitter");
            //emitterData
            XmlHelper.StoreStruct(emitterData, doc, emitterNode, "EmitterData");
            //particleEventGroups
            XmlHelper.StoreObjectList(particleEventGroups, doc, emitterNode, "ParticleEventGroups");
            node.AppendChild(emitterNode);
            return emitterNode;
        }
        #endregion
    }
}
