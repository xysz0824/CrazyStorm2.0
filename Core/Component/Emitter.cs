/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EmitterData
    {
        public Vector2 emitPosition;
        public int emitCount;
        public int emitCycle;
        public float emitAngle;
        public bool bindToSpeedAngle;
        public float emitRange;
        public float emitRadius;
        public float emitRoundAngle;
        public bool instantMovement;
    }
    public abstract class Emitter : Component
    {
        #region Private Members
        EmitterData emitterData;
        IList<EventGroup> particleEventGroups;
        Vector2[] lastSpawn;
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
        [BoolProperty]
        public bool BindToSpeedAngle
        {
            get { return emitterData.bindToSpeedAngle; }
            set { emitterData.bindToSpeedAngle = value; }
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
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitRoundAngle
        {
            get { return emitterData.emitRoundAngle; }
            set { emitterData.emitRoundAngle = value; }
        }
        [BoolProperty]
        public bool InstantMovement
        {
            get { return emitterData.instantMovement; }
            set { emitterData.instantMovement = value; }
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
            lastSpawn = null;
        }
        #endregion

        #region Private Methods
        void EmitCyclically(float frameScale)
        {
            if (MathHelper.FrameMod(CurrentFrame, frameScale, EmitCycle)) Emit(frameScale);
        }
        void Emit(float frameScale)
        {
            var emitPosition = EmitPosition;
            if (ExecuteRandomExpression("EmitPosition", frameScale)) emitPosition += VM.PopVector2();
            var emitCount = EmitCount;
            if (ExecuteRandomExpression("EmitCount", frameScale)) emitCount += VM.PopInt();
            var emitCycle = EmitCycle;
            if (ExecuteRandomExpression("EmitCycle", frameScale)) emitCycle += VM.PopInt();
            var emitAngle = EmitAngle;
            if (ExecuteRandomExpression("EmitAngle", frameScale)) emitAngle += VM.PopFloat();
            var emitRange = EmitRange;
            if (ExecuteRandomExpression("EmitRange", frameScale)) emitRange += VM.PopFloat();
            var emitRadius = EmitRadius;
            if (ExecuteRandomExpression("EmitRadius", frameScale)) emitRadius += VM.PopFloat();
            var emitRoundAngle = EmitRoundAngle;
            if (ExecuteRandomExpression("EmitRoundAngle", frameScale)) emitRoundAngle += VM.PopFloat();
            Template.ExecuteExpressionsAndSet(frameScale);
            float increment = emitRange / emitCount;
            float angle = emitAngle - (emitRange + increment) / 2;
            if (BindToSpeedAngle) angle = SpeedAngle + angle;
            for (int i = 0; i < emitCount; ++i)
            {
                angle += increment;
                Template.PPosition = new Vector2(
                    emitPosition.x + emitRadius * (float)Math.Cos(MathHelper.DegToRad(emitRoundAngle)),
                    emitPosition.y + emitRadius * (float)Math.Sin(MathHelper.DegToRad(emitRoundAngle)));
                Template.PSpeedAngle = angle;
                ParticleBase newParticle = ParticleManager.GetParticle(System, LayerID, Template);
                if (InstantMovement)
                {
                    newParticle.MaxLife = 1;
                    if (lastSpawn == null) lastSpawn = new Vector2[emitCount];
                    else if (lastSpawn != null && lastSpawn.Length < emitCount)
                    {
                        lastSpawn = new Vector2[emitCount];
                    }
                    newParticle.PPositionLast = lastSpawn[i] == default ? newParticle.PPosition : lastSpawn[i];
                    lastSpawn[i] = newParticle.PPosition;
                }
                newParticle.ParticleEventGroups = EmitterEventGroups;
                Particles.AddLast(newParticle);
            }
            if (EventManager.Sounds != null && EventManager.TypeSoundMap != null && 
                EventManager.TypeSoundMap.ContainsKey(Template.Type.ID))
            {
                var sound = EventManager.Sounds.FirstOrDefault((item) => item.ID == EventManager.TypeSoundMap[Template.Type.ID]);
                if (sound != null) EventManager.PlaySound(sound.AbsolutePath);
            }
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Emitter;
            clone.lastSpawn = new Vector2[EmitCount];
            clone.particle = particle.Clone() as ParticleBase;
            clone.particleEventGroups = new GenericContainer<EventGroup>();
            foreach (var item in particleEventGroups) clone.particleEventGroups.Add(item.Clone() as EventGroup);
            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var emitterNode = (XmlElement)node.SelectSingleNode("Emitter");
            //emitterData
            XmlHelper.BuildFromStruct(ref emitterData, emitterNode);
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
            XmlHelper.StoreStruct(emitterData, doc, emitterNode);
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
                //emitterData
                emitterData = PlayDataHelper.ReadStruct<EmitterData>(emitterReader);
                //particle
                InitialTemplate.LoadPlayData(emitterReader, version);
                InitialTemplate.Emitter = this;
                //emitterEventGroups
                PlayDataHelper.ReadObjectList(EmitterEventGroups, emitterReader, version);
                InitialTemplate.ParticleEventGroups = EmitterEventGroups;
            }
        }
        protected override bool PushSystemProperty(string propertyName)
        {
            if (base.PushSystemProperty(propertyName)) return true;
            switch (propertyName)
            {
                case "SelfAngle":
                    VM.PushFloat(MathHelper.GetDegree(Position - EmitPosition));
                    return true;
            }
            return false;
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName)) return true;
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
                case "BindToSpeedAngle":
                    VM.PushBool(BindToSpeedAngle);
                    return true;
                case "EmitRange":
                    VM.PushFloat(EmitRange);
                    return true;
                case "EmitRadius":
                    VM.PushFloat(EmitRadius);
                    return true;
                case "EmitRoundAngle":
                    VM.PushFloat(EmitRoundAngle);
                    return true;
                case "InstanceMovement":
                    VM.PushBool(InstantMovement);
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
                case "BindToSpeedAngle":
                    BindToSpeedAngle = VM.PopBool();
                    return true;
                case "EmitRange":
                    EmitRange = VM.PopFloat();
                    return true;
                case "EmitRadius":
                    EmitRadius = VM.PopFloat();
                    return true;
                case "EmitRoundAngle":
                    EmitRoundAngle = VM.PopFloat();
                    return true;
                case "InstantMovement":
                    InstantMovement = VM.PopBool();
                    if (!InstantMovement && lastSpawn != null) lastSpawn = null;
                    return true;
            }
            if (Template == null) return false;
            return Template.SetProperty(propertyName);
        }
        public override bool Update(float frameScale, float currentFrame)
        {
            if (!base.Update(frameScale, currentFrame)) return false;
            if (BindingTarget == null || CheckCircularBinding()) EmitCyclically(frameScale);
            else BindingUpdate(EmitCyclically, true, frameScale);
            return true;
        }
        public override void Reset()
        {
            Template = InitialTemplate.Clone() as ParticleBase;
            Template.Reset();
            base.Reset();
            var initialState = base.initialState as Emitter;
            EmitPosition = initialState.EmitPosition;
            EmitCount = initialState.EmitCount;
            EmitCycle = initialState.EmitCycle;
            EmitAngle = initialState.EmitAngle;
            BindToSpeedAngle = initialState.BindToSpeedAngle;
            EmitRange = initialState.EmitRange;
            EmitRadius = initialState.EmitRadius;
            EmitRoundAngle = initialState.EmitRoundAngle;
            InstantMovement = initialState.InstantMovement;
        }
        public void EmitParticle(float frameScale)
        {
            if (BindingTarget == null || CheckCircularBinding())
                Emit(frameScale);
            else
                BindingUpdate(Emit, true, frameScale);
        }
        #endregion
    }
}
