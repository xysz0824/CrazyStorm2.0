/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public enum ForceType
    {
        Direction,
        Inner,
        Outer
    }
    public struct ForceFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public FieldShape fieldShape;
        public Reach reach;
        public string targetName;
        public float force;
        public float direction;
        public ForceType forceType;
    }
    public class ForceField : Component
    {
        #region Private Members
        ForceFieldData forceFieldData;
        #endregion

        #region Public Members
        [FloatProperty(0, float.MaxValue)]
        public float HalfWidth
        {
            get { return forceFieldData.halfWidth; }
            set { forceFieldData.halfWidth = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float HalfHeight
        {
            get { return forceFieldData.halfHeight; }
            set { forceFieldData.halfHeight = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Force
        {
            get { return forceFieldData.force; }
            set { forceFieldData.force = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Direction
        {
            get { return forceFieldData.direction; }
            set { forceFieldData.direction = value; }
        }
        [EnumProperty(typeof(ForceType))]
        public ForceType ForceType
        {
            get { return forceFieldData.forceType; }
            set { forceFieldData.forceType = value; }
        }
        [EnumProperty(typeof(FieldShape))]
        public FieldShape FieldShape
        {
            get { return forceFieldData.fieldShape; }
            set { forceFieldData.fieldShape = value; }
        }
        [EnumProperty(typeof(Reach))]
        public Reach Reach
        {
            get { return forceFieldData.reach; }
            set { forceFieldData.reach = value; }
        }
        [StringProperty(1, 15, true, true, false, false)]
        public string TargetName
        {
            get { return forceFieldData.targetName; }
            set { forceFieldData.targetName = value; }
        }
        #endregion

        #region Constructor
        public ForceField()
        {
            forceFieldData.targetName = string.Empty;
            forceFieldData.halfWidth = 50;
            forceFieldData.halfHeight = 50;
            forceFieldData.force = 0.1f;
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var forceFieldNode = (XmlElement)node.SelectSingleNode("ForceField");
            //forceFieldData
            XmlHelper.BuildFromStruct(ref forceFieldData, forceFieldNode, "ForceFieldData");
            return forceFieldNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var forceFieldNode = doc.CreateElement("ForceField");
            //forceFieldData
            XmlHelper.StoreStruct(forceFieldData, doc, forceFieldNode, "ForceFieldData");
            node.AppendChild(forceFieldNode);
            return forceFieldNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var forceFieldBytes = new List<byte>();
            //forceFieldData
            PlayDataHelper.GenerateStruct(forceFieldData, forceFieldBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(forceFieldBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader forceFieldReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(forceFieldReader))
                {
                    HalfWidth = dataReader.ReadSingle();
                    HalfHeight = dataReader.ReadSingle();
                    FieldShape = PlayDataHelper.ReadEnum<FieldShape>(dataReader);
                    Reach = PlayDataHelper.ReadEnum<Reach>(dataReader);
                    TargetName = PlayDataHelper.ReadString(dataReader);
                    Force = dataReader.ReadSingle();
                    Direction = dataReader.ReadSingle();
                    ForceType = PlayDataHelper.ReadEnum<ForceType>(dataReader);
                }
            }
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "HalfWidth":
                    VM.PushFloat(HalfWidth);
                    return true;
                case "HalfHeight":
                    VM.PushFloat(HalfHeight);
                    return true;
                case "FieldShape":
                    VM.PushEnum((int)FieldShape);
                    return true;
                case "Reach":
                    VM.PushEnum((int)Reach);
                    return true;
                case "TargetName":
                    VM.PushString(TargetName);
                    return true;
                case "Force":
                    VM.PushFloat(Force);
                    return true;
                case "Direction":
                    VM.PushFloat(Direction);
                    return true;
                case "ForceType":
                    VM.PushEnum((int)ForceType);
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
                case "HalfWidth":
                    HalfWidth = VM.PopFloat();
                    return true;
                case "HalfHeight":
                    HalfHeight = VM.PopFloat();
                    return true;
                case "FieldShape":
                    FieldShape = (FieldShape)VM.PopEnum();
                    return true;
                case "Reach":
                    Reach = (Reach)VM.PopEnum();
                    return true;
                case "TargetName":
                    TargetName = VM.PopString();
                    return true;
                case "Force":
                    Force = VM.PopFloat();
                    return true;
                case "Direction":
                    Direction = VM.PopFloat();
                    return true;
                case "ForceType":
                    ForceType = (ForceType)VM.PopEnum();
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
            var initialState = base.initialState as ForceField;
            HalfWidth = initialState.HalfWidth;
            HalfHeight = initialState.HalfHeight;
            FieldShape = initialState.FieldShape;
            Reach = initialState.Reach;
            TargetName = initialState.TargetName;
            Force = initialState.Force;
            Direction = initialState.Direction;
            ForceType = initialState.ForceType;
        }
        void Update()
        {
            base.ExecuteExpression("HalfWidth");
            base.ExecuteExpression("HalfHeight");
            base.ExecuteExpression("Force");
            base.ExecuteExpression("Direction");
            List<ParticleBase> results = ParticleManager.SearchByRect(Position.x - HalfWidth, Position.x + HalfWidth,
                Position.y - HalfHeight, Position.y + HalfHeight);
            foreach (Particle particle in results)
            {
                if (particle.IgnoreForce)
                    continue;

                switch (Reach)
                {
                    case Reach.Layer:
                        if (particle.Emitter.LayerName != TargetName && particle.Emitter.LayerName != LayerName)
                            continue;

                        break;
                    case Reach.Name:
                        if (particle.Emitter.Name != TargetName)
                            continue;

                        break;
                }
                if (FieldShape == FieldShape.Circle)
                {
                    Vector2 v = Position - particle.PPosition;
                    if (Math.Sqrt(v.x * v.x + v.y * v.y) > HalfWidth)
                        continue;
                }
                switch (ForceType)
                {
                    case ForceType.Direction:
                        Vector2 v = new Vector2();
                        MathHelper.SetVector2(ref v, Force / particle.Mass, Direction);
                        particle.PSpeedVector += v;
                        break;
                    case ForceType.Inner:
                        v = Position - particle.PPosition;
                        float d = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
                        particle.PSpeedVector += v / d * (Force / particle.Mass);
                        break;
                    case ForceType.Outer:
                        v = particle.PPosition - Position;
                        d = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
                        particle.PSpeedVector += v / d * (Force / particle.Mass);
                        break;
                }
            }
        }
        #endregion
    }
}
