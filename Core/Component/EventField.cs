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
    public enum FieldShape
    {
        Rectangle,
        Circle
    }
    public enum Reach
    {
        All,
        Layer,
        Name
    }
    public enum LayerMaskType
    {
        InsideMask,
        OutsideMask,
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EventFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public FieldShape fieldShape;
        public Reach reach;
        public bool layerMask;
        public LayerMaskType layerMaskType;
        public bool layerMaskMutex;
        public float rotation;
        public float dissolveStrength;
        public float dissolveEdgeWidth;
        public float dissolveUSpeed;
        public float dissolveVSpeed;
    }
    public class EventFieldPool : PoolObject<EventFieldPool, NullData>
    {
        public EventField Instance { get; private set; }
        public EventFieldPool()
        {
            Instance = new EventField();
            Instance.PoolObject = this;
        }
    }
    public class EventField : Component
    {
        #region Private Members
        [StringData]
        [XmlAttribute]
        string targetName;
        int maskTypeID = -1;
        EventFieldData eventFieldData;
        Dictionary<long, EventFieldData> bindingEventFieldData;
        MaskType maskType;
        #endregion

        #region Public Members
        public EventFieldPool PoolObject { get; set; }
        [FloatProperty(14, 1, float.MaxValue)]
        public float HalfWidth
        {
            get { return eventFieldData.halfWidth; }
            set { eventFieldData.halfWidth = value; }
        }
        [FloatProperty(15, 1, float.MaxValue)]
        public float HalfHeight
        {
            get { return eventFieldData.halfHeight; }
            set { eventFieldData.halfHeight = value; }
        }
        [EnumProperty(16, typeof(FieldShape))]
        public FieldShape FieldShape
        {
            get { return eventFieldData.fieldShape; }
            set { eventFieldData.fieldShape = value; }
        }
        [EnumProperty(17, typeof(Reach))]
        public Reach Reach
        {
            get { return eventFieldData.reach; }
            set { eventFieldData.reach = value; }
        }
        [StringProperty(18, 1, 15, true, true, false, false)]
        public string TargetName
        {
            get { return targetName; }
            set { targetName = value; }
        }
        [BoolProperty(19)]
        public bool LayerMask
        {
            get { return eventFieldData.layerMask; }
            set { eventFieldData.layerMask = value; }
        }
        [EnumProperty(20, typeof(LayerMaskType))]
        public LayerMaskType LayerMaskType
        {
            get { return eventFieldData.layerMaskType; }
            set { eventFieldData.layerMaskType = value; }
        }
        [BoolProperty(21)]
        public bool LayerMaskMutex
        {
            get { return eventFieldData.layerMaskMutex; }
            set { eventFieldData.layerMaskMutex = value; }
        }
        [FloatProperty(22, int.MinValue, int.MaxValue)]
        public float Rotation
        {
            get { return eventFieldData.rotation; }
            set { eventFieldData.rotation = value; }
        }
        [ReadOnlyProperty(23)]
        public MaskType MaskType
        {
            get { return maskType; }
            set
            {
                maskType = value;
                maskTypeID = value != null ? value.ID : -1;
            }
        }
        [FloatProperty(24, 0, 1)]
        public float DissolveStrength
        {
            get { return eventFieldData.dissolveStrength; }
            set { eventFieldData.dissolveStrength = Math.Max(0, Math.Min(1, value)); }
        }
        [FloatProperty(25, 0, float.MaxValue)]
        public float DissolveEdgeWidth
        {
            get { return eventFieldData.dissolveEdgeWidth; }
            set { eventFieldData.dissolveEdgeWidth = value >= 0 ? value : 0; }
        }
        [FloatProperty(26, float.MinValue, float.MaxValue)]
        public float DissolveUSpeed
        {
            get { return eventFieldData.dissolveUSpeed; }
            set { eventFieldData.dissolveUSpeed = value; }
        }
        [FloatProperty(27, float.MinValue, float.MaxValue)]
        public float DissolveVSpeed
        {
            get { return eventFieldData.dissolveVSpeed; }
            set { eventFieldData.dissolveVSpeed = value; }
        }
        public GenericContainer<EventGroup> EventFieldEventGroups { get; private set; }
        #endregion

        #region Constructor
        public EventField()
        {
            targetName = string.Empty;
            eventFieldData.halfWidth = 50;
            eventFieldData.halfHeight = 50;
            bindingEventFieldData = new Dictionary<long, EventFieldData>();
            EventFieldEventGroups = new GenericContainer<EventGroup>();
        }
        #endregion

        #region Private Methods
        int GetMaskFrameIndex(MaskType type)
        {
            if (type == null || type.Frames <= 1) return 0;
            return (int)Math.Max(CurrentFrame - 1, 0) / (type.Delay + 1) % type.Frames;
        }
        void UpdateMaskTextureData(int maskIndex)
        {
            var selectedMaskType = MaskType;
            var hasTexture = 0f;
            var startPoint = Vector2.Zero;
            var size = Vector2.Zero;
            var frame = 0f;
            if (selectedMaskType != null)
            {
                hasTexture = 1f;
                startPoint = selectedMaskType.StartPoint;
                size = new Vector2(selectedMaskType.Width, selectedMaskType.Height);
                frame = GetMaskFrameIndex(selectedMaskType);
            }
            ParticleManager.MaskTextureStartPointArray[maskIndex] = startPoint;
            ParticleManager.MaskTextureSizeArray[maskIndex] = size;
            ParticleManager.MaskTextureFrameArray[maskIndex] = frame;
            ParticleManager.MaskDissolveStrengthArray[maskIndex] = DissolveStrength;
            ParticleManager.MaskDissolveEdgeWidthArray[maskIndex] = DissolveEdgeWidth;
            ParticleManager.MaskDissolveUSpeedArray[maskIndex] = selectedMaskType != null ? DissolveUSpeed : 0;
            ParticleManager.MaskDissolveVSpeedArray[maskIndex] = selectedMaskType != null ? DissolveVSpeed : 0;
            ParticleManager.MaskAnimateFrameArray[maskIndex] = selectedMaskType != null ? CurrentFrame : 0;
            ParticleManager.MaskTextureEnabledArray[maskIndex] = hasTexture;
        }
        void Update(float frameScale)
        {
            int count = 0;
            var results = FieldShape == FieldShape.Rectangle ? 
                ParticleManager.SearchByRect(System, Position, HalfWidth, HalfHeight, Rotation, out count) :
                ParticleManager.SearchByEllipse(System, Position, HalfWidth, HalfHeight, Rotation, out count);
            for (int i = 0;i < count; ++i)
            {
                if (results[i].IgnoreMask) continue;
                switch (Reach)
                {
                    case Reach.Layer:
                        if (results[i].Emitter.LayerName != TargetName) continue;
                        break;
                    case Reach.Name:
                        if (results[i].Emitter.Name != TargetName || results[i].Emitter.LayerName != LayerName) continue;
                        break;
                }
                for (int k = 0; k < EventFieldEventGroups.Count; ++k)
                {
                    EventFieldEventGroups[k].Execute(results[i], null, frameScale);
                }
            }
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as EventField;
            clone.EventFieldEventGroups = new GenericContainer<EventGroup>();
            foreach (var item in EventFieldEventGroups) clone.EventFieldEventGroups.Add(item.Clone() as EventGroup);
            return clone;
        }
        public override void CopyTo(PropertyContainer propertyContainer)
        {
            base.CopyTo(propertyContainer);
            var eventField = propertyContainer as EventField;
            eventField.targetName = targetName;
            eventField.maskTypeID = maskTypeID;
            eventField.eventFieldData = eventFieldData;
            eventField.maskType = maskType;
            eventField.EventFieldEventGroups = EventFieldEventGroups;
        }
        public override Component Instantiate()
        {
            return EventFieldPool.Rent(NullData.Empty).Instance;
        }
        public override void Destroy()
        {
            EventFieldPool.Return(PoolObject);
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var eventFieldNode = (XmlElement)node.SelectSingleNode("EventField");
            XmlHelper.BuildFromFields(this, eventFieldNode);
            if (eventFieldNode.HasAttribute("maskType"))
            {
                int parsedID;
                if (int.TryParse(eventFieldNode.GetAttribute("maskType"), out parsedID)) maskTypeID = parsedID;
                else throw new FileLoadException("FileDataError");
            }
            else maskTypeID = -1;
            maskType = null;
            //eventFieldData
            XmlHelper.BuildFromStruct(ref eventFieldData, eventFieldNode);
            //eventFieldEventGroups
            XmlHelper.BuildFromObjectList(EventFieldEventGroups, new EventGroup(), eventFieldNode, "EventFieldEventGroups");
            return eventFieldNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var eventFieldNode = doc.CreateElement("EventField");
            XmlHelper.StoreFields(this, doc, eventFieldNode);
            if (maskType != null)
            {
                var maskTypeAttribute = doc.CreateAttribute("maskType");
                maskTypeAttribute.Value = maskType.ID.ToString();
                eventFieldNode.Attributes.Append(maskTypeAttribute);
            }
            //eventFieldData
            XmlHelper.StoreStruct(eventFieldData, doc, eventFieldNode);
            //eventFieldEventGroups
            XmlHelper.StoreObjectList(EventFieldEventGroups, doc, eventFieldNode, "EventFieldEventGroups");
            node.AppendChild(eventFieldNode);
            return eventFieldNode;
        }
        public override List<byte> GeneratePlayData(File file)
        {
            var bytes = base.GeneratePlayData(file);
            var eventFieldBytes = new List<byte>();
            //stringDataFields
            PlayDataHelper.GenerateStringDataFields(this, eventFieldBytes);
            //eventFieldData
            PlayDataHelper.GenerateStruct(eventFieldData, eventFieldBytes);
            eventFieldBytes.AddRange(BitConverter.GetBytes(maskTypeID));
            //eventFieldEventGroups
            PlayDataHelper.GenerateObjectList(file, EventFieldEventGroups, eventFieldBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(eventFieldBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader eventFieldReader = PlayDataHelper.GetBlockReader(reader))
            {
                //stringDataFields
                PlayDataHelper.ReadStringDataFields(this, eventFieldReader);
                //eventFieldData
                eventFieldData = PlayDataHelper.ReadStruct<EventFieldData>(eventFieldReader);
                maskTypeID = eventFieldReader.ReadInt32();
                maskType = null;
                //eventFieldEventGroups
                PlayDataHelper.ReadObjectList(EventFieldEventGroups, eventFieldReader, version);
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
                    VM.PushBool(LayerMask);
                    return true;
                case 20:
                    VM.PushInt((int)LayerMaskType);
                    return true;
                case 21:
                    VM.PushBool(LayerMaskMutex);
                    return true;
                case 22:
                    VM.PushFloat(Rotation);
                    return true;
                case 24:
                    VM.PushFloat(DissolveStrength);
                    return true;
                case 25:
                    VM.PushFloat(DissolveEdgeWidth);
                    return true;
                case 26:
                    VM.PushFloat(DissolveUSpeed);
                    return true;
                case 27:
                    VM.PushFloat(DissolveVSpeed);
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
                    LayerMask = VM.PopBool();
                    return true;
                case 20:
                    LayerMaskType = (LayerMaskType)VM.PopInt();
                    return true;
                case 21:
                    LayerMaskMutex = VM.PopBool();
                    return true;
                case 22:
                    Rotation = VM.PopFloat();
                    return true;
                case 24:
                    DissolveStrength = VM.PopFloat();
                    return true;
                case 25:
                    DissolveEdgeWidth = VM.PopFloat();
                    return true;
                case 26:
                    DissolveUSpeed = VM.PopFloat();
                    return true;
                case 27:
                    DissolveVSpeed = VM.PopFloat();
                    return true;
            }
            return false;
        }
        protected override int BindingClear(PropertyContainer propertyContainer, long[] resultArray)
        {
            var count = base.BindingClear(propertyContainer, resultArray);
            for (int i = 0; i < count; ++i)
            {
                bindingEventFieldData.Remove(resultArray[i]);
            }
            return count;
        }
        protected override void BindingUpdate(ParticleBase particle, long uniqueId, int updateId, bool executeEvents, float frameScale)
        {
            if (bindingEventFieldData.ContainsKey(uniqueId)) eventFieldData = bindingEventFieldData[uniqueId];
            base.BindingUpdate(particle, uniqueId, updateId, executeEvents, frameScale);
            bindingEventFieldData[uniqueId] = eventFieldData;
        }
        public override void BindingUpdate(int updateId, float frameScale)
        {
            if (updateId == 0) Update(frameScale);
            else if (updateId == 1) UpdateMutexMask();
            else if (updateId == 2) UpdateLayerMask();
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
            var initialState = base.initialState as EventField;
            TargetName = initialState.TargetName;
            eventFieldData = initialState.eventFieldData;
            maskTypeID = initialState.maskTypeID;
            MaskType = initialState.MaskType;
        }
        public void RebuildMaskTypeReference(IList<MaskType> maskTypes)
        {
            maskType = null;
            if (maskTypeID < 0 || maskTypes == null) return;
            for (int i = 0; i < maskTypes.Count; ++i)
            {
                if (maskTypes[i].ID == maskTypeID)
                {
                    maskType = maskTypes[i];
                    return;
                }
            }
        }
        public void UpdateMutexMask()
        {
            var maskCount = ParticleManager.MaskCount;
            if (maskCount >= ParticleManager.MAX_MASK_COUNT || !LayerMask || !Visibility || !LayerMaskMutex || !IsValid()) return;
            var rotation = (float)MathHelper.DegToRad(Rotation);
            var halfWidthSq = HalfWidth * HalfWidth;
            var halfHeightSq = HalfHeight * HalfHeight;
            ParticleManager.MaskPositionArray[maskCount] = Position;
            ParticleManager.MaskSizeArray[maskCount] = new Vector2(HalfWidth, HalfHeight);
            ParticleManager.MaskShapeArray[maskCount] = FieldShape == FieldShape.Circle ? 1 : 0;
            ParticleManager.MaskLayerArray[maskCount] = LayerMaskType == LayerMaskType.OutsideMask ? 1 : 2;
            ParticleManager.MaskRotateArray[maskCount] = rotation;
            ParticleManager.MaskRotateTrigArray[maskCount] = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            ParticleManager.MaskEllipseInvSizeSqArray[maskCount] = new Vector2(1f / halfWidthSq, 1f / halfHeightSq);
            UpdateMaskTextureData(maskCount);
            maskCount++;
            ParticleManager.MaskCount = maskCount;
        }
        public void UpdateLayerMask()
        {
            var maskCount = ParticleManager.MaskCount;
            if (maskCount >= ParticleManager.MAX_MASK_COUNT || !LayerMask || !Visibility || !IsValid()) return;
            var rotation = (float)MathHelper.DegToRad(Rotation);
            var halfWidthSq = HalfWidth * HalfWidth;
            var halfHeightSq = HalfHeight * HalfHeight;
            ParticleManager.MaskPositionArray[maskCount] = Position;
            ParticleManager.MaskSizeArray[maskCount] = new Vector2(HalfWidth, HalfHeight);
            ParticleManager.MaskShapeArray[maskCount] = FieldShape == FieldShape.Circle ? 1 : 0;
            ParticleManager.MaskLayerArray[maskCount] = (int)LayerMaskType + 1;
            ParticleManager.MaskRotateArray[maskCount] = rotation;
            ParticleManager.MaskRotateTrigArray[maskCount] = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
            ParticleManager.MaskEllipseInvSizeSqArray[maskCount] = new Vector2(1f / halfWidthSq, 1f / halfHeightSq);
            UpdateMaskTextureData(maskCount);
            maskCount++;
            ParticleManager.MaskCount = maskCount;
        }
        #endregion
    }
}
