/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
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
    public struct EmitterData
    {
        public Vector2 emitPosition;
        public int emitCount;
        public int emitCycle;
        public float emitAngle;
        public float emitRange;
    }
    public abstract class Emitter : Component
    {
        #region Private Members
        EmitterData emitterData;
        IList<EventGroup> particleEventGroups;
        #endregion

        #region Protected Members
        protected ParticleBase particle;
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
        public ParticleBase Particle { get { return particle; } }
        public IList<EventGroup> ParticleEventGroups { get { return particleEventGroups; } }
        #endregion

        #region Constructor
        public Emitter()
        {
            Properties["EmitPosition"] = new PropertyValue
            {
                Expression = true,
                Value = "Position"
            };
            emitterData.emitCount = 1;
            emitterData.emitCycle = 10;
            emitterData.emitRange = 360;
            particleEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Emitter;
            clone.particle = particle.Clone() as ParticleBase;
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
            XmlHelper.BuildFromStruct(ref emitterData, emitterNode, "EmitterData");
            //particle
            particle.BuildFromXml(emitterNode);
            //particleEventGroups
            XmlHelper.BuildFromObjectList(particleEventGroups, new EventGroup(), emitterNode, "ParticleEventGroups");
            return emitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var emitterNode = doc.CreateElement("Emitter");
            //emitterData
            XmlHelper.StoreStruct(emitterData, doc, emitterNode, "EmitterData");
            //particle
            particle.StoreAsXml(doc, emitterNode);
            //particleEventGroups
            XmlHelper.StoreObjectList(particleEventGroups, doc, emitterNode, "ParticleEventGroups");
            node.AppendChild(emitterNode);
            return emitterNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var emitterBytes = new List<byte>();
            //emitterData
            PlayDataHelper.GenerateStruct(emitterData, emitterBytes);
            //particle
            emitterBytes.AddRange(particle.GeneratePlayData());
            //particleEventGroups
            PlayDataHelper.GenerateObjectList(particleEventGroups, emitterBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(emitterBytes));
            return bytes;
        }
        #endregion
    }
}
