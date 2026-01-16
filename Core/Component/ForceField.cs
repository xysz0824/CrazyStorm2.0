/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public enum ForceType
    {
        OneDirection,
        InnerForce,
        OuterForce
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ForceFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public FieldShape fieldShape;
        public Reach reach;
        public float force;
        public float direction;
        public ForceType forceType;
        public float rotation;
        public float impactSpeed;
    }
    public class ForceField : Component
    {
        public delegate void ForceImpactHandler(Vector2 impactSpeed);
        public static event ForceImpactHandler OnForceImpactBody;

        #region Private Members
        [StringData]
        [XmlAttribute]
        public string targetName;
        ForceFieldData forceFieldData;
        #endregion

        #region Public Members
        [FloatProperty(1, float.MaxValue)]
        public float HalfWidth
        {
            get { return forceFieldData.halfWidth; }
            set { forceFieldData.halfWidth = value; }
        }
        [FloatProperty(1, float.MaxValue)]
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
            get { return targetName; }
            set { targetName = value; }
        }
        [FloatProperty(int.MinValue, int.MaxValue)]
        public float Rotation
        {
            get { return forceFieldData.rotation; }
            set { forceFieldData.rotation = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float ForceImpactSpeed
        {
            get { return forceFieldData.impactSpeed; }
            set { forceFieldData.impactSpeed = value; }
        }
        #endregion

        #region Constructor
        public ForceField()
        {
            targetName = string.Empty;
            forceFieldData.halfWidth = 50;
            forceFieldData.halfHeight = 50;
            forceFieldData.force = 0.1f;
        }
        #endregion

        #region Private Methods
        void Update()
        {
            int count = 0;
            Vector2 v = default;
            var results = FieldShape == FieldShape.Rectangle ?
                ParticleManager.SearchByRect(Position, HalfWidth, HalfHeight, Rotation, out count) :
                ParticleManager.SearchByEllipse(Position, HalfWidth, HalfHeight, Rotation, out count);
            for (int i = 0; i < count;++i)
            {
                if (results[i].IgnoreForce) continue;
                switch (Reach)
                {
                    case Reach.Layer:
                        if (results[i].Emitter.LayerName != TargetName) continue;
                        break;
                    case Reach.Name:
                        if (results[i].Emitter.Name != TargetName && results[i].Emitter.LayerName != LayerName) continue;
                        break;
                }
                switch (ForceType)
                {
                    case ForceType.OneDirection:
                        v = MathHelper.GetVector2(Force / results[i].Mass, Direction);
                        results[i].PSpeedVector += v;
                        break;
                    case ForceType.InnerForce:
                        v = Position == results[i].PPosition ? new Vector2(0, 0) : Vector2.Normalize(Position - results[i].PPosition);
                        results[i].PSpeedVector += v * (Force / results[i].Mass);
                        break;
                    case ForceType.OuterForce:
                        v = Position == results[i].PPosition ? new Vector2(0, 0) : Vector2.Normalize(results[i].PPosition - Position);
                        results[i].PSpeedVector += v * (Force / results[i].Mass);
                        break;
                }
            }
            var bodyImpacted = false;
            v = MathHelper.Rotate(BodyPosition - Position, -Rotation);
            if (FieldShape == FieldShape.Rectangle)
            {
                bodyImpacted = v.x >= -HalfWidth && v.x <= HalfWidth && v.y >= -HalfHeight && v.y <= HalfHeight;
            }
            else if (FieldShape == FieldShape.Circle)
            {
                bodyImpacted = (v.x * v.x) / (HalfWidth * HalfWidth) + (v.y * v.y) / (HalfHeight * HalfHeight) <= 1f;
            }
            if (bodyImpacted)
            {
                switch (ForceType)
                {
                    case ForceType.OneDirection:
                        OnForceImpactBody?.Invoke(MathHelper.GetVector2(ForceImpactSpeed, Direction));
                        break;
                    case ForceType.InnerForce:
                        OnForceImpactBody?.Invoke((Position == BodyPosition ? new Vector2(0, 0) : Vector2.Normalize(Position - BodyPosition)) * 
                            ForceImpactSpeed);
                        break;
                    case ForceType.OuterForce:
                        OnForceImpactBody?.Invoke((Position == BodyPosition ? new Vector2(0, 0) : Vector2.Normalize(BodyPosition - Position)) * 
                            ForceImpactSpeed);
                        break;
                }
            }
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var forceFieldNode = (XmlElement)node.SelectSingleNode("ForceField");
            XmlHelper.BuildFromFields(this, forceFieldNode);
            //forceFieldData
            XmlHelper.BuildFromStruct(ref forceFieldData, forceFieldNode);
            return forceFieldNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var forceFieldNode = doc.CreateElement("ForceField");
            XmlHelper.StoreFields(this, doc, forceFieldNode);
            //forceFieldData
            XmlHelper.StoreStruct(forceFieldData, doc, forceFieldNode);
            node.AppendChild(forceFieldNode);
            return forceFieldNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var forceFieldBytes = new List<byte>();
            //stringDataFields
            PlayDataHelper.GenerateStringDataFields(this, forceFieldBytes);
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
                //stringDataFields
                PlayDataHelper.ReadStringDataFields(this, forceFieldReader);
                //forceFieldData
                forceFieldData = PlayDataHelper.ReadStruct<ForceFieldData>(forceFieldReader);
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
                    VM.PushInt((int)FieldShape);
                    return true;
                case "Reach":
                    VM.PushInt((int)Reach);
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
                    VM.PushInt((int)ForceType);
                    return true;
                case "Rotation":
                    VM.PushFloat(Rotation);
                    return true;
                case "ForceImpactSpeed":
                    VM.PushFloat(ForceImpactSpeed);
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
                    FieldShape = (FieldShape)VM.PopInt();
                    return true;
                case "Reach":
                    Reach = (Reach)VM.PopInt();
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
                    ForceType = (ForceType)VM.PopInt();
                    return true;
                case "Rotation":
                    Rotation = VM.PopFloat();
                    return true;
                case "ForceImpactSpeed":
                    ForceImpactSpeed = VM.PopFloat();
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
                BindingUpdate(Update, true);

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
            Rotation = initialState.Rotation;
            ForceImpactSpeed = initialState.ForceImpactSpeed;
        }
        #endregion
    }
}
