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
        [IntProperty(1, int.MaxValue)]
        public int Size
        {
            get { return rebounderData.size; }
            set { rebounderData.size = value; }
        }
        [EnumProperty(typeof(RebounderShape))]
        public RebounderShape RebounderShape
        {
            get { return rebounderData.shape; }
            set { rebounderData.shape = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Rotation
        {
            get { return rebounderData.rotation; }
            set { rebounderData.rotation = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int ReboundLimit
        {
            get { return rebounderData.limit; }
            set { rebounderData.limit = value; }
        }
        [BoolProperty]
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
        void Update(float frameRate)
        {
            int count = 0;
            var results = ParticleManager.SearchByRect(Position, Size, Size, 0, out count);
            for (int i = 0; i < count; ++i)
            {
                if (results[i].IgnoreRebound || results[i].Type == null || results[i].PSpeedVector == Vector2.Zero ||
                    results[i].ReboundTime >= ReboundLimit) continue;
                float width = results[i].Type.Width;
                float widthScale = results[i].WidthScale;
                float height = (results[i] is Particle) ? results[i].Type.Height : results[i].Type.Width;
                float heightScale = (results[i] is Particle) ? (results[i] as Particle).HeightScale : results[i].WidthScale;
                var center = MathHelper.GetActualCenter(results[i].PPosition, results[i].PRotation, results[i].Type.CenterPoint,
                    new Vector2(width, height), new Vector2(widthScale, heightScale));
                float radius = new Vector2(width * widthScale, height * heightScale).Length() * 0.5f;
                float rotation = Rotation;
                switch (RebounderShape)
                {
                    case RebounderShape.Line:
                        var p1 = Position;
                        var p2 = Position + MathHelper.GetVector2(Size, Rotation);
                        if (!MathHelper.LineIntersectWithCircle(p1, p2, center, radius)) continue;
                        if (SpeedVector != Vector2.Zero && Vector2.Dot(results[i].PSpeedVector, SpeedVector) >= 0) continue;
                        var normal = Vector2.Normalize(MathHelper.Rotate(p2 - p1, -90));
                        if (ReboundOneSide && Vector2.Dot(normal, results[i].PSpeedVector) >= 0) continue;
                        float dr = Rotation - lastRotation;
                        if (dr != 0f)
                        {
                            Vector2 rotationVector = MathHelper.GetVector2(1, Rotation + dr > 0 ? 90 : -90);
                            if (Vector2.Dot(results[i].PSpeedVector, rotationVector) >= 0) continue;
                        }
                        break;
                    case RebounderShape.Circle:
                        if (!MathHelper.TwoCirclesIntersect(Position, Size, center, radius)) continue;
                        if (MathHelper.PointInsideCircle(Position, Size, center))
                        {
                            if (Vector2.Dot(results[i].PSpeedVector, Position - center) >= 0) continue;
                        }
                        else if (Vector2.Dot(results[i].PSpeedVector, center - Position) >= 0) continue;
                        rotation = MathHelper.GetDegree(center - Position) + 90;
                        break;
                }
                results[i].PSpeedVector = MathHelper.GetVector2(results[i].PSpeed, 2 * rotation - results[i].PSpeedAngle);
                for (int k = 0; k < RebounderEventGroups.Count; ++k) RebounderEventGroups[k].Execute(results[i], null, frameRate);
                results[i].ReboundTime++;
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
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var rebounderBytes = new List<byte>();
            //rebounderData
            PlayDataHelper.GenerateStruct(rebounderData, rebounderBytes);
            //rebounderEventGroups
            PlayDataHelper.GenerateObjectList(rebounderEventGroups, rebounderBytes);
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
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "Size":
                    VM.PushInt(Size);
                    return true;
                case "RebounderShape":
                    VM.PushInt((int)RebounderShape);
                    return true;
                case "Rotation":
                    VM.PushFloat(Rotation);
                    return true;
                case "ReboundLimit":
                    VM.PushInt(ReboundLimit);
                    return true;
                case "ReboundOneSide":
                    VM.PushBool(ReboundOneSide);
                    return true;
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "Size":
                    Size = VM.PopInt();
                    return true;
                case "RebounderShape":
                    RebounderShape = (RebounderShape)VM.PopInt();
                    return true;
                case "Rotation":
                    Rotation = VM.PopFloat();
                    return true;
                case "ReboundLimit":
                    ReboundLimit = VM.PopInt();
                    return true;
                case "ReboundOneSide":
                    ReboundOneSide = VM.PopBool();
                    return true;
            }
            return false;
        }
        public override bool Update(float frameRate, float currentFrame)
        {
            if (!base.Update(frameRate, currentFrame))
                return false;

            if (BindingTarget == null)
                Update(frameRate);
            else
                BindingUpdate(Update, true, frameRate);

            return true;
        }
        public override void Reset(float frameRate)
        {
            base.Reset(frameRate);
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
