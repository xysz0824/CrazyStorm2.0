/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public enum RebounderShape
    {
        Line,
        Circle
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RebounderData
    {
        public int size;
        public RebounderShape shape;
        public float rotation;
        public int limit;
        public bool oneSide;
    }
    public class Rebounder : Component
    {
        #region Private Members
        float lastRotation;
        RebounderData rebounderData;
        IList<EventGroup> rebounderEventGroups;
        #endregion

        #region Public Members
        [IntProperty(14, 1, int.MaxValue)]
        public int Size
        {
            get { return rebounderData.size; }
            set { rebounderData.size = value; }
        }
        [EnumProperty(15, typeof(RebounderShape))]
        public RebounderShape RebounderShape
        {
            get { return rebounderData.shape; }
            set { rebounderData.shape = value; }
        }
        [FloatProperty(16, float.MinValue, float.MaxValue)]
        public float Rotation
        {
            get { return rebounderData.rotation; }
            set { rebounderData.rotation = value; }
        }
        [IntProperty(17, 1, int.MaxValue)]
        public int ReboundLimit
        {
            get { return rebounderData.limit; }
            set { rebounderData.limit = value; }
        }
        [BoolProperty(18)]
        public bool ReboundOneSide
        {
            get { return rebounderData.oneSide; }
            set { rebounderData.oneSide= value; }
        }
        public IList<EventGroup> RebounderEventGroups { get { return rebounderEventGroups; } }
        #endregion

        #region Constructor
        public Rebounder()
        {
            rebounderData.size = 50;
            rebounderData.limit = 1;
            rebounderEventGroups = new GenericContainer<EventGroup>();
        }
        #endregion

        #region Private Methods
        void Update(float frameScale)
        {
            ParticleManager.SearchByRect(Position, Size, Size, 0);
            foreach (var particle in ParticleManager.ActiveParticles)
            {
                if (!particle.SearchFlag) continue;
                particle.SearchFlag = false;
                if (particle.IgnoreRebound || 
                    particle.Type == null || particle.PSpeedVector == Vector2.Zero || 
                    particle.ReboundTime >= ReboundLimit) continue;
                float width = particle.Type.Width;
                float widthScale = particle.WidthScale;
                float height = (particle is Particle) ? particle.Type.Height : particle.Type.Width;
                float heightScale = (particle is Particle) ? (particle as Particle).HeightScale : particle.WidthScale;
                var center = MathHelper.GetActualCenter(particle.PPosition, particle.PRotation, particle.Type.CenterPoint,
                    new Vector2(width, height), new Vector2(widthScale, heightScale));
                float radius = new Vector2(width * widthScale, height * heightScale).Length() * 0.5f;
                float rotation = Rotation;
                switch (RebounderShape)
                {
                    case RebounderShape.Line:
                        var p1 = Position;
                        var p2 = Position + MathHelper.GetVector2(Size, Rotation);
                        if (!MathHelper.LineIntersectWithCircle(p1, p2, center, radius)) continue;
                        if (SpeedVector != Vector2.Zero && Vector2.Dot(particle.PSpeedVector, SpeedVector) >= 0) continue;
                        var normal = Vector2.Normalize(MathHelper.Rotate(p2 - p1, -90));
                        if (ReboundOneSide && Vector2.Dot(normal, particle.PSpeedVector) >= 0) continue;
                        float dr = Rotation - lastRotation;
                        if (dr != 0f)
                        {
                            Vector2 rotationVector = MathHelper.GetVector2(1, Rotation + dr > 0 ? 90 : -90);
                            if (Vector2.Dot(particle.PSpeedVector, rotationVector) >= 0) continue;
                        }
                        break;
                    case RebounderShape.Circle:
                        if (!MathHelper.TwoCirclesIntersect(Position, Size, center, radius)) continue;
                        if (MathHelper.PointInsideCircle(Position, Size, center))
                        {
                            if (Vector2.Dot(particle.PSpeedVector, Position - center) >= 0) continue;
                        }
                        else if (Vector2.Dot(particle.PSpeedVector, center - Position) >= 0) continue;
                        rotation = MathHelper.GetDegree(center - Position) + 90;
                        break;
                }
                particle.PSpeedVector = MathHelper.GetVector2(particle.PSpeed, 2 * rotation - particle.PSpeedAngle);
                for (int i = 0; i < RebounderEventGroups.Count; ++i)
                {
                    RebounderEventGroups[i].Execute(particle, null, frameScale);
                }
                particle.ReboundTime++;
            }
            lastRotation = Rotation;
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Rebounder;
            clone.rebounderEventGroups = new GenericContainer<EventGroup>();
            foreach (var item in rebounderEventGroups) clone.rebounderEventGroups.Add(item.Clone() as EventGroup);
            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var rebounderNode = (XmlElement)node.SelectSingleNode("Rebounder");
            //rebounderData
            XmlHelper.BuildFromStruct(ref rebounderData, rebounderNode);
            //rebounderEventGroups
            XmlHelper.BuildFromObjectList(rebounderEventGroups, new EventGroup(), rebounderNode, "RebounderEventGroups");
            return rebounderNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var rebounderNode = doc.CreateElement("Rebounder");
            //rebounderData
            XmlHelper.StoreStruct(rebounderData, doc, rebounderNode);
            //rebounderEventGroups
            XmlHelper.StoreObjectList(rebounderEventGroups, doc, rebounderNode, "RebounderEventGroups");
            node.AppendChild(rebounderNode);
            return rebounderNode;
        }
        public override List<byte> GeneratePlayData(File file)
        {
            var bytes = base.GeneratePlayData(file);
            var rebounderBytes = new List<byte>();
            //rebounderData
            PlayDataHelper.GenerateStruct(rebounderData, rebounderBytes);
            //rebounderEventGroups
            PlayDataHelper.GenerateObjectList(file, rebounderEventGroups, rebounderBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(rebounderBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader rebounderReader = PlayDataHelper.GetBlockReader(reader))
            {
                //rebounderData
                rebounderData = PlayDataHelper.ReadStruct<RebounderData>(rebounderReader);
                lastRotation = Rotation;
                //rebounderEventGroups
                PlayDataHelper.ReadObjectList(RebounderEventGroups, rebounderReader, version);
            }
        }
        public override bool PushProperty(int propertyID)
        {
            if (base.PushProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 14:
                    VM.PushInt(Size);
                    return true;
                case 15:
                    VM.PushInt((int)RebounderShape);
                    return true;
                case 16:
                    VM.PushFloat(Rotation);
                    return true;
                case 17:
                    VM.PushInt(ReboundLimit);
                    return true;
                case 18:
                    VM.PushBool(ReboundOneSide);
                    return true;
            }
            return false;
        }
        public override bool SetProperty(int propertyID)
        {
            if (base.SetProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 14:
                    Size = VM.PopInt();
                    return true;
                case 15:
                    RebounderShape = (RebounderShape)VM.PopInt();
                    return true;
                case 16:
                    Rotation = VM.PopFloat();
                    return true;
                case 17:
                    ReboundLimit = VM.PopInt();
                    return true;
                case 18:
                    ReboundOneSide = VM.PopBool();
                    return true;
            }
            return false;
        }
        public override bool Update(float frameScale, float currentFrame)
        {
            if (!base.Update(frameScale, currentFrame))
                return false;

            if (BindingTarget == null)
                Update(frameScale);
            else
                BindingUpdate(Update, true, frameScale);

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as Rebounder;
            Size = initialState.Size;
            RebounderShape = initialState.RebounderShape;
            Rotation = initialState.Rotation;
            ReboundLimit = initialState.ReboundLimit;
            ReboundOneSide = initialState.ReboundOneSide;
        }
        #endregion
    }
}
