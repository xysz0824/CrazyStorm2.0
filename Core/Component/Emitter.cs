/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public float emitRadius;
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
        public ParticleBase InitialTemplate { get; protected set; }
        public ParticleBase Template { get; protected set; }
        public LinkedList<ParticleBase> Particles { get; private set; }
        public IList<EventGroup> EmitterEventGroups { get; private set; }
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
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitRadius
        {
            get { return emitterData.emitRadius; }
            set { emitterData.emitRadius = value; }
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
            Particles = new LinkedList<ParticleBase>();
            EmitterEventGroups = new List<EventGroup>();
            particleEventGroups = new GenericContainer<EventGroup>();
        }
        #endregion

        #region Private Methods
        void EmitCyclically()
        {
            if (CurrentFrame % EmitCycle == 0)
                Emit();
        }
        void Emit()
        {
            base.ExecuteExpression("EmitCycle");
            base.ExecuteExpression("EmitRange");
            base.ExecuteExpression("EmitCount");
            base.ExecuteExpression("EmitAngle");
            base.ExecuteExpression("EmitPosition");
            base.ExecuteExpression("EmitRadius");
            Template.ExecuteExpressions();
            float increment = EmitRange / EmitCount;
            float angle = EmitAngle - (EmitRange + increment) / 2;
            for (int i = 0; i < EmitCount; ++i)
            {
                angle += increment;
                Template.PPosition = new Vector2(
                    EmitPosition.x + EmitRadius * (float)Math.Cos(MathHelper.DegToRad(angle)),
                    EmitPosition.y + EmitRadius * (float)Math.Sin(MathHelper.DegToRad(angle)));
                Template.PSpeedAngle = angle;
                ParticleBase newParticle = ParticleManager.GetParticle(Template);
                newParticle.ParticleEventGroups = EmitterEventGroups;
                Particles.AddLast(newParticle);
            }
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Emitter;
            clone.particle = particle.Clone() as ParticleBase;
            clone.particleEventGroups = new GenericContainer<EventGroup>();
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
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader emitterReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(emitterReader))
                {
                    EmitPosition = PlayDataHelper.ReadVector2(dataReader);
                    EmitCount = dataReader.ReadInt32();
                    EmitCycle = dataReader.ReadInt32();
                    EmitAngle = dataReader.ReadSingle();
                    EmitRange = dataReader.ReadSingle();
                    EmitRadius = dataReader.ReadSingle();
                }
                //particle
                InitialTemplate.LoadPlayData(emitterReader, version);
                InitialTemplate.Emitter = this;
                //emitterEventGroups
                PlayDataHelper.LoadObjectList(EmitterEventGroups, emitterReader, version);
                InitialTemplate.ParticleEventGroups = EmitterEventGroups;
            }
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "EmitPosition":
                    VM.PushVector2(EmitPosition);
                    return true;
                case "EmitPosition.x":
                    VM.PushFloat(EmitPosition.x);
                    return true;
                case "EmitPosition.y":
                    VM.PushFloat(EmitPosition.y);
                    return true;
                case "EmitCount":
                    VM.PushInt(EmitCount);
                    return true;
                case "EmitCycle":
                    VM.PushInt(EmitCycle);
                    return true;
                case "EmitAngle":
                    VM.PushFloat(EmitAngle);
                    return true;
                case "EmitRange":
                    VM.PushFloat(EmitRange);
                    return true;
                case "EmitRadius":
                    VM.PushFloat(EmitRadius);
                    return true;
            }
            if (Template == null)
            {
                return false;
            }
            return Template.PushProperty(propertyName);
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "EmitPosition":
                    EmitPosition = VM.PopVector2();
                    return true;
                case "EmitPosition.x":
                    EmitPosition = new Vector2(VM.PopFloat(), EmitPosition.y);
                    return true;
                case "EmitPosition.y":
                    EmitPosition = new Vector2(EmitPosition.x, VM.PopFloat());
                    return true;
                case "EmitCount":
                    EmitCount = VM.PopInt();
                    return true;
                case "EmitCycle":
                    EmitCycle = VM.PopInt();
                    return true;
                case "EmitAngle":
                    EmitAngle = VM.PopFloat();
                    return true;
                case "EmitRange":
                    EmitRange = VM.PopFloat();
                    return true;
                case "EmitRadius":
                    EmitRadius = VM.PopFloat();
                    return true;
            }
            if (Template == null)
            {
                return false;
            }
            return Template.SetProperty(propertyName);
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            if (BindingTarget == null || CheckCircularBinding())
                EmitCyclically();
            else
                BindingUpdate(EmitCyclically);

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as Emitter;
            EmitPosition = initialState.EmitPosition;
            EmitCount = initialState.EmitCount;
            EmitCycle = initialState.EmitCycle;
            EmitAngle = initialState.EmitAngle;
            EmitRange = initialState.EmitRange;
            EmitRadius = initialState.EmitRadius;
            Template = InitialTemplate.Copy();
        }
        public void EmitParticle()
        {
            if (BindingTarget == null || CheckCircularBinding())
                Emit();
            else
                BindingUpdate(Emit);
        }
        #endregion
    }
}
