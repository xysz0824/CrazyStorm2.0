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
    public class ForceFieldPool : PoolObject<ForceFieldPool, NullData>
    {
        public ForceField Instance { get; private set; }
        public ForceFieldPool()
        {
            Instance = new ForceField();
            Instance.PoolObject = this;
        }
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
        Dictionary<long, ForceFieldData> bindingForceFieldData;
        #endregion

        #region Public Members
        public ForceFieldPool PoolObject { get; set; }
        [FloatProperty(14, 1, float.MaxValue)]
        public float HalfWidth
        {
            get { return forceFieldData.halfWidth; }
            set { forceFieldData.halfWidth = value; }
        }
        [FloatProperty(15, 1, float.MaxValue)]
        public float HalfHeight
        {
            get { return forceFieldData.halfHeight; }
            set { forceFieldData.halfHeight = value; }
        }
        [FloatProperty(16, float.MinValue, float.MaxValue)]
        public float Force
        {
            get { return forceFieldData.force; }
            set { forceFieldData.force = value; }
        }
        [FloatProperty(17, float.MinValue, float.MaxValue)]
        public float Direction
        {
            get { return forceFieldData.direction; }
            set { forceFieldData.direction = value; }
        }
        [EnumProperty(18, typeof(ForceType))]
        public ForceType ForceType
        {
            get { return forceFieldData.forceType; }
            set { forceFieldData.forceType = value; }
        }
        [EnumProperty(19, typeof(FieldShape))]
        public FieldShape FieldShape
        {
            get { return forceFieldData.fieldShape; }
            set { forceFieldData.fieldShape = value; }
        }
        [EnumProperty(20, typeof(Reach))]
        public Reach Reach
        {
            get { return forceFieldData.reach; }
            set { forceFieldData.reach = value; }
        }
        [StringProperty(21, 1, 15, true, true, false, false)]
        public string TargetName
        {
            get { return targetName; }
            set { targetName = value; }
        }
        [FloatProperty(22, int.MinValue, int.MaxValue)]
        public float Rotation
        {
            get { return forceFieldData.rotation; }
            set { forceFieldData.rotation = value; }
        }
        [FloatProperty(23, float.MinValue, float.MaxValue)]
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
            bindingForceFieldData = new Dictionary<long, ForceFieldData>();
        }
        #endregion

        #region Private Methods
        void Update(float frameScale)
        {
            int count = 0;
            Vector2 v = default;
            var results = FieldShape == FieldShape.Rectangle ?
                ParticleManager.SearchByRect(System, Position, HalfWidth, HalfHeight, Rotation, out count) :
                ParticleManager.SearchByEllipse(System, Position, HalfWidth, HalfHeight, Rotation, out count);
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
                        results[i].PSpeedVector += v * frameScale;
                        break;
                    case ForceType.InnerForce:
                        v = Position == results[i].PPosition ? new Vector2(0, 0) : Vector2.Normalize(Position - results[i].PPosition);
                        results[i].PSpeedVector += v * (Force / results[i].Mass) * frameScale;
                        break;
                    case ForceType.OuterForce:
                        v = Position == results[i].PPosition ? new Vector2(0, 0) : Vector2.Normalize(results[i].PPosition - Position);
                        results[i].PSpeedVector += v * (Force / results[i].Mass) * frameScale;
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
        public override void CopyTo(PropertyContainer propertyContainer)
        {
            base.CopyTo(propertyContainer);
            var forceField = propertyContainer as ForceField;
            forceField.targetName = targetName;
            forceField.forceFieldData = forceFieldData;
        }
        public override Component Instantiate()
        {
            return ForceFieldPool.Rent(NullData.Empty).Instance;
        }
        public override void Destroy()
        {
            ForceFieldPool.Return(PoolObject);
        }
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
        public override List<byte> GeneratePlayData(File file)
        {
            var bytes = base.GeneratePlayData(file);
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
        public override bool PushProperty(int propertyID)
        {
            if (base.PushProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 14:
                    VM.PushFloat(HalfWidth);
                    return true;
                case 15:
                    VM.PushFloat(HalfHeight);
                    return true;
                case 16:
                    VM.PushInt((int)FieldShape);
                    return true;
                case 17:
                    VM.PushInt((int)Reach);
                    return true;
                case 18:
                    VM.PushString(TargetName);
                    return true;
                case 19:
                    VM.PushFloat(Force);
                    return true;
                case 20:
                    VM.PushFloat(Direction);
                    return true;
                case 21:
                    VM.PushInt((int)ForceType);
                    return true;
                case 22:
                    VM.PushFloat(Rotation);
                    return true;
                case 23:
                    VM.PushFloat(ForceImpactSpeed);
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
                    HalfWidth = VM.PopFloat();
                    return true;
                case 15:
                    HalfHeight = VM.PopFloat();
                    return true;
                case 16:
                    FieldShape = (FieldShape)VM.PopInt();
                    return true;
                case 17:
                    Reach = (Reach)VM.PopInt();
                    return true;
                case 18:
                    TargetName = VM.PopString();
                    return true;
                case 19:
                    Force = VM.PopFloat();
                    return true;
                case 20:
                    Direction = VM.PopFloat();
                    return true;
                case 21:
                    ForceType = (ForceType)VM.PopInt();
                    return true;
                case 22:
                    Rotation = VM.PopFloat();
                    return true;
                case 23:
                    ForceImpactSpeed = VM.PopFloat();
                    return true;
            }
            return false;
        }
        protected override int BindingClear(PropertyContainer propertyContainer, long[] resultArray)
        {
            var count = base.BindingClear(propertyContainer, resultArray);
            for (int i = 0; i < count; ++i) bindingForceFieldData.Remove(resultArray[i]);
            return count;
        }
        protected override void BindingUpdate(ParticleBase particle, long uniqueId, int updateId, bool executeEvents, float frameScale)
        {
            if (bindingForceFieldData.ContainsKey(uniqueId)) forceFieldData = bindingForceFieldData[uniqueId];
            base.BindingUpdate(particle, uniqueId, updateId, executeEvents, frameScale);
            bindingForceFieldData[uniqueId] = forceFieldData;
        }
        public override void BindingUpdate(int updateId, float frameScale)
        {
            Update(frameScale);
        }
        public override bool Update(float frameScale, float currentFrame)
        {
            if (!base.Update(frameScale, currentFrame))
                return false;

            if (BindingTarget == null)
                Update(frameScale);
            else
                BindingUpdate(0, true, frameScale);

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as ForceField;
            TargetName = initialState.TargetName;
            forceFieldData = initialState.forceFieldData;
        }
        #endregion
    }
}
