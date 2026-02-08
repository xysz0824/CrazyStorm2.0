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
        EventFieldData eventFieldData;
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
        public GenericContainer<EventGroup> EventFieldEventGroups { get; private set; }
        #endregion

        #region Constructor
        public EventField()
        {
            targetName = string.Empty;
            eventFieldData.halfWidth = 50;
            eventFieldData.halfHeight = 50;
            EventFieldEventGroups = new GenericContainer<EventGroup>();
        }
        #endregion

        #region Private Methods
        void Update(float frameScale)
        {
            int count = 0;
            var results = FieldShape == FieldShape.Rectangle ? 
                ParticleManager.SearchByRect(Position, HalfWidth, HalfHeight, Rotation, out count) :
                ParticleManager.SearchByEllipse(Position, HalfWidth, HalfHeight, Rotation, out count);
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
            eventField.eventFieldData = eventFieldData;
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
            }
            return false;
        }
        public override void BindingUpdate(int id, float frameScale)
        {
            if (id == 0) Update(frameScale);
            else if (id == 1) UpdateMutexMask();
            else if (id == 2) UpdateLayerMask();
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
        }
        public void UpdateMutexMask()
        {
            var maskCount = ParticleManager.MaskCount;
            if (maskCount >= ParticleManager.MAX_MASK_COUNT || !LayerMask || !Visibility || !LayerMaskMutex) return;
            ParticleManager.MaskPositionArray[maskCount] = Position;
            ParticleManager.MaskSizeArray[maskCount] = new Vector2(HalfWidth, HalfHeight);
            ParticleManager.MaskShapeArray[maskCount] = FieldShape == FieldShape.Circle ? 1 : 0;
            ParticleManager.MaskTypeArray[maskCount] = LayerMaskType == LayerMaskType.OutsideMask ? 1 : 2;
            ParticleManager.MaskRotateArray[maskCount] = (float)MathHelper.DegToRad(Rotation);
            maskCount++;
        }
        public void UpdateLayerMask()
        {
            var maskCount = ParticleManager.MaskCount;
            if (maskCount >= ParticleManager.MAX_MASK_COUNT || !LayerMask || !Visibility) return;
            ParticleManager.MaskPositionArray[maskCount] = Position;
            ParticleManager.MaskSizeArray[maskCount] = new Vector2(HalfWidth, HalfHeight);
            ParticleManager.MaskShapeArray[maskCount] = FieldShape == FieldShape.Circle ? 1 : 0;
            ParticleManager.MaskTypeArray[maskCount] = (int)LayerMaskType + 1;
            ParticleManager.MaskRotateArray[maskCount] = (float)MathHelper.DegToRad(Rotation);
            maskCount++;
        }
        #endregion
    }
}
