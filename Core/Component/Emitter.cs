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
        Vector2[] lastSpawn;
        #endregion

        #region Public Members
        public ParticleBase InitialTemplate { get; protected set; }
        public ParticleBase Template { get; protected set; }
        public List<ParticleBase> Particles { get; private set; }
        public List<EventGroup> EmitterEventGroups { get; private set; }
        [Vector2Property(14)]
        public Vector2 EmitPosition
        {
            get { return emitterData.emitPosition; }
            set { emitterData.emitPosition = value; }
        }
        [IntProperty(17, 1, int.MaxValue)]
        public int EmitCount
        {
            get { return emitterData.emitCount; }
            set { emitterData.emitCount = value; }
        }
        [IntProperty(18, 1, int.MaxValue)]
        public int EmitCycle
        {
            get { return emitterData.emitCycle; }
            set { emitterData.emitCycle = value; }
        }
        [FloatProperty(19, float.MinValue, float.MaxValue)]
        public float EmitAngle
        {
            get { return emitterData.emitAngle; }
            set { emitterData.emitAngle = value; }
        }
        [BoolProperty(20)]
        public bool BindToSpeedAngle
        {
            get { return emitterData.bindToSpeedAngle; }
            set { emitterData.bindToSpeedAngle = value; }
        }
        [FloatProperty(21, float.MinValue, float.MaxValue)]
        public float EmitRange
        {
            get { return emitterData.emitRange; }
            set { emitterData.emitRange = value; }
        }
        [FloatProperty(22, float.MinValue, float.MaxValue)]
        public float EmitRadius
        {
            get { return emitterData.emitRadius; }
            set { emitterData.emitRadius = value; }
        }
        [FloatProperty(23, float.MinValue, float.MaxValue)]
        public float EmitRoundAngle
        {
            get { return emitterData.emitRoundAngle; }
            set { emitterData.emitRoundAngle = value; }
        }
        [BoolProperty(24)]
        public bool InstantMovement
        {
            get { return emitterData.instantMovement; }
            set { emitterData.instantMovement = value; }
        }
        public GenericContainer<EventGroup> ParticleEventGroups { get; private set; }
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
            Particles = new List<ParticleBase>();
            EmitterEventGroups = new List<EventGroup>();
            ParticleEventGroups = new GenericContainer<EventGroup>();
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
            if (ExecuteRandomExpression(14, frameScale)) emitPosition += VM.PopVector2();
            var emitCount = EmitCount;
            if (ExecuteRandomExpression(17, frameScale)) emitCount += VM.PopInt();
            var emitCycle = EmitCycle;
            if (ExecuteRandomExpression(18, frameScale)) emitCycle += VM.PopInt();
            var emitAngle = EmitAngle;
            if (ExecuteRandomExpression(19, frameScale)) emitAngle += VM.PopFloat();
            var emitRange = EmitRange;
            if (ExecuteRandomExpression(21, frameScale)) emitRange += VM.PopFloat();
            var emitRadius = EmitRadius;
            if (ExecuteRandomExpression(22, frameScale)) emitRadius += VM.PopFloat();
            var emitRoundAngle = EmitRoundAngle;
            if (ExecuteRandomExpression(23, frameScale)) emitRoundAngle += VM.PopFloat();
            Template.ExecuteExpressionsAndSet(frameScale);
            float increment = emitRange / emitCount;
            float angle = emitAngle - (emitRange + increment) / 2;
            float roundAngle = emitRoundAngle - (emitRange + increment) / 2;
            if (BindToSpeedAngle) angle = SpeedAngle + angle;
            for (int i = 0; i < emitCount; ++i)
            {
                angle += increment;
                roundAngle += increment;
                Template.PPosition = new Vector2(
                    emitPosition.x + emitRadius * (float)Math.Cos(MathHelper.DegToRad(roundAngle)),
                    emitPosition.y + emitRadius * (float)Math.Sin(MathHelper.DegToRad(roundAngle)));
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
                Particles.Add(newParticle);
            }
            if (System.Sounds != null && System.TypeSoundMap != null && System.TypeSoundMap.ContainsKey(Template.Type.ID))
            {
                var sound = System.Sounds.FirstOrDefault((item) => item.ID == System.TypeSoundMap[Template.Type.ID]);
                if (sound != null) EventManager.PlaySound(sound.AbsolutePath);
            }
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Emitter;
            clone.lastSpawn = new Vector2[EmitCount];
            clone.InitialTemplate = InitialTemplate.Clone() as ParticleBase;
            clone.ParticleEventGroups = new GenericContainer<EventGroup>();
            foreach (var item in ParticleEventGroups) clone.ParticleEventGroups.Add(item.Clone() as EventGroup);
            return clone;
        }
        public override void CopyTo(PropertyContainer propertyContainer)
        {
            base.CopyTo(propertyContainer);
            var emitter = propertyContainer as Emitter;
            emitter.InitialTemplate = InitialTemplate;
            emitter.InitialTemplate.Emitter = emitter;
            emitter.Particles.Clear();
            emitter.EmitterEventGroups = EmitterEventGroups;
            emitter.emitterData = emitterData;
            emitter.ParticleEventGroups = ParticleEventGroups;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var emitterNode = (XmlElement)node.SelectSingleNode("Emitter");
            //emitterData
            XmlHelper.BuildFromStruct(ref emitterData, emitterNode);
            //particle
            InitialTemplate.BuildFromXml(emitterNode);
            //particleEventGroups
            XmlHelper.BuildFromObjectList(ParticleEventGroups, new EventGroup(), emitterNode, "ParticleEventGroups");
            return emitterNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var emitterNode = doc.CreateElement("Emitter");
            //emitterData
            XmlHelper.StoreStruct(emitterData, doc, emitterNode);
            //particle
            InitialTemplate.StoreAsXml(doc, emitterNode);
            //particleEventGroups
            XmlHelper.StoreObjectList(ParticleEventGroups, doc, emitterNode, "ParticleEventGroups");
            node.AppendChild(emitterNode);
            return emitterNode;
        }
        public override List<byte> GeneratePlayData(File file)
        {
            var bytes = base.GeneratePlayData(file);
            var emitterBytes = new List<byte>();
            //emitterData
            PlayDataHelper.GenerateStruct(emitterData, emitterBytes);
            //particle
            emitterBytes.AddRange(InitialTemplate.GeneratePlayData(file, this));
            //particleEventGroups
            PlayDataHelper.GenerateObjectList(file, ParticleEventGroups, emitterBytes);
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
        protected override bool PushSystemProperty(int propertyID)
        {
            switch (propertyID)
            {
                case -3:
                    var degree = MathHelper.GetDegree(Position - EmitPosition);
                    VM.PushFloat(degree);
                    return true;
                case -7:
                    degree = MathHelper.GetDegree(BodyPosition - EmitPosition);
                    VM.PushFloat(degree);
                    return true;
                case -11:
                    degree = MathHelper.GetDegree(CenterPosition - EmitPosition);
                    VM.PushFloat(degree);
                    return true;
            }
            if (base.PushSystemProperty(propertyID)) return true;
            return false;
        }
        public override bool PushProperty(int propertyID)
        {
            if (base.PushProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 14:
                    VM.PushVector2(EmitPosition);
                    return true;
                case 15:
                    VM.PushFloat(EmitPosition.x);
                    return true;
                case 16:
                    VM.PushFloat(EmitPosition.y);
                    return true;
                case 17:
                    VM.PushInt(EmitCount);
                    return true;
                case 18:
                    VM.PushInt(EmitCycle);
                    return true;
                case 19:
                    VM.PushFloat(EmitAngle);
                    return true;
                case 20:
                    VM.PushBool(BindToSpeedAngle);
                    return true;
                case 21:
                    VM.PushFloat(EmitRange);
                    return true;
                case 22:
                    VM.PushFloat(EmitRadius);
                    return true;
                case 23:
                    VM.PushFloat(EmitRoundAngle);
                    return true;
                case 24:
                    VM.PushBool(InstantMovement);
                    return true;
            }
            if (Template == null) return false;
            return Template.PushProperty(propertyID);
        }
        public override bool SetProperty(int propertyID)
        {
            if (base.SetProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 14:
                    EmitPosition = VM.PopVector2();
                    return true;
                case 15:
                    EmitPosition = new Vector2(VM.PopFloat(), EmitPosition.y);
                    return true;
                case 16:
                    EmitPosition = new Vector2(EmitPosition.x, VM.PopFloat());
                    return true;
                case 17:
                    EmitCount = VM.PopInt();
                    return true;
                case 18:
                    EmitCycle = VM.PopInt();
                    return true;
                case 19:
                    EmitAngle = VM.PopFloat();
                    return true;
                case 20:
                    BindToSpeedAngle = VM.PopBool();
                    return true;
                case 21:
                    EmitRange = VM.PopFloat();
                    return true;
                case 22:
                    EmitRadius = VM.PopFloat();
                    return true;
                case 23:
                    EmitRoundAngle = VM.PopFloat();
                    return true;
                case 24:
                    InstantMovement = VM.PopBool();
                    if (!InstantMovement && lastSpawn != null) lastSpawn = null;
                    return true;
            }
            if (Template == null) return false;
            return Template.SetProperty(propertyID);
        }
        public override void BindingUpdate(int id, float frameScale)
        {
            if (id == 0) EmitCyclically(frameScale);
            else if (id == 1) Emit(frameScale);
        }
        public override bool Update(float frameScale, float currentFrame)
        {
            if (!base.Update(frameScale, currentFrame)) return false;
            if (BindingTarget == null || CheckCircularBinding()) EmitCyclically(frameScale);
            else BindingUpdate(0, true, frameScale);
            return true;
        }
        public override void Reset()
        {
            InitialTemplate.CopyTo(Template);
            base.Reset();
            var initialState = base.initialState as Emitter;
            emitterData = initialState.emitterData;
        }
        public void EmitParticle(float frameScale)
        {
            if (BindingTarget == null || CheckCircularBinding())
                Emit(frameScale);
            else
                BindingUpdate(1, true, frameScale);
        }
        #endregion
    }
}
