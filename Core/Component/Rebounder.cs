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
    public enum RebounderShape
    {
        Line,
        Circle
    }
    public struct RebounderData
    {
        public int size;
        public RebounderShape rebounderShape;
        public float rotation;
    }
    public class Rebounder : Component
    {
        #region Private Members
        float lastRotation;
        RebounderData rebounderData;
        IList<EventGroup> rebounderEventGroups;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int Size
        {
            get { return rebounderData.size; }
            set { rebounderData.size = value; }
        }
        [EnumProperty(typeof(RebounderShape))]
        public RebounderShape RebounderShape
        {
            get { return rebounderData.rebounderShape; }
            set { rebounderData.rebounderShape = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Rotation
        {
            get { return rebounderData.rotation; }
            set { rebounderData.rotation = value; }
        }
        public IList<EventGroup> RebounderEventGroups { get { return rebounderEventGroups; } }
        #endregion

        #region Constructor
        public Rebounder()
        {
            rebounderData.size = 50;
            rebounderEventGroups = new GenericContainer<EventGroup>();
        }
        #endregion

        #region Private Methods
        void Update()
        {
            base.ExecuteExpression("Size");
            base.ExecuteExpression("Rotation");
            int count = 0;
            List<ParticleBase> results = ParticleManager.SearchByRect(Position.x - Size, Position.x + Size,
                Position.y - Size, Position.y + Size, out count);
            for (int i = 0; i < count; ++i)
            {
                if (results[i].IgnoreRebound || results[i].Type == null || results[i].PSpeedVector == Vector2.Zero)
                    continue;

                float radius = Math.Max(results[i].Type.Width, results[i].Type.Height) / 2;
                float rotation = Rotation;
                switch (RebounderShape)
                {
                    case RebounderShape.Line:
                        Vector2 p1 = Position + MathHelper.GetVector2(Size, Rotation);
                        Vector2 p2 = Position + MathHelper.GetVector2(Size, Rotation + 180);
                        if (!MathHelper.LineIntersectWithCircle(p1, p2, results[i].PPosition, radius))
                            continue;

                        if (SpeedVector != Vector2.Zero && Vector2.Dot(results[i].PSpeedVector, SpeedVector) >= 0)
                            continue;

                        float dr = Rotation - lastRotation;
                        if (dr > 0)
                        {
                            Vector2 rotationVector = MathHelper.GetVector2(1, Rotation + 90);
                            if (Vector2.Dot(results[i].PSpeedVector, rotationVector) >= 0)
                                continue;
                        }
                        else if (dr < 0)
                        {
                            Vector2 rotationVector = MathHelper.GetVector2(1, Rotation - 90);
                            if (Vector2.Dot(results[i].PSpeedVector, rotationVector) >= 0)
                                continue;
                        }
                        break;
                    case RebounderShape.Circle:
                        if (!MathHelper.TwoCirclesIntersect(Position, Size, results[i].PPosition, radius))
                            continue;

                        if (MathHelper.PointInsideCircle(Position, Size, results[i].PPosition))
                        {
                            if (Vector2.Dot(results[i].PSpeedVector, Position - results[i].PPosition) >= 0)
                                continue;
                        }
                        else if (Vector2.Dot(results[i].PSpeedVector, results[i].PPosition - Position) >= 0)
                            continue;

                        rotation = MathHelper.GetDegree(results[i].PPosition - Position) + 90;
                        break;
                }
                results[i].PSpeedVector = MathHelper.GetVector2(results[i].PSpeed, 2 * rotation - results[i].PSpeedAngle);
                for (int k = 0; k < RebounderEventGroups.Count; ++k)
                    RebounderEventGroups[k].Execute(results[i]);
            }
            lastRotation = Rotation;
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Rebounder;
            clone.rebounderEventGroups = new GenericContainer<EventGroup>();
            foreach (var item in rebounderEventGroups)
                clone.rebounderEventGroups.Add(item.Clone() as EventGroup);

            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var rebounderNode = (XmlElement)node.SelectSingleNode("Rebounder");
            //rebounderData
            XmlHelper.BuildFromStruct(ref rebounderData, rebounderNode, "RebounderData");
            //rebounderEventGroups
            XmlHelper.BuildFromObjectList(rebounderEventGroups, new EventGroup(), rebounderNode, "RebounderEventGroups");
            return rebounderNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var rebounderNode = doc.CreateElement("Rebounder");
            //rebounderData
            XmlHelper.StoreStruct(rebounderData, doc, rebounderNode, "RebounderData");
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
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(rebounderReader))
                {
                    Size = dataReader.ReadInt32();
                    RebounderShape = PlayDataHelper.ReadEnum<RebounderShape>(dataReader);
                    Rotation = dataReader.ReadSingle();
                    lastRotation = Rotation;
                }
                //rebounderEventGroups
                PlayDataHelper.LoadObjectList(RebounderEventGroups, rebounderReader, version);
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
                    VM.PushEnum((int)RebounderShape);
                    return true;
                case "Rotation":
                    VM.PushFloat(Rotation);
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
                    RebounderShape = (RebounderShape)VM.PopEnum();
                    return true;
                case "Rotation":
                    Rotation = VM.PopFloat();
                    return true;
            }
            return false;
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            if (BindingTarget == null)
                Update();
            else
                BindingUpdate(Update);

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as Rebounder;
            Size = initialState.Size;
            RebounderShape = initialState.RebounderShape;
            Rotation = initialState.Rotation;
        }
        #endregion
    }
}
