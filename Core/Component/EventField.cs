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
    public struct EventFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public FieldShape fieldShape;
        public Reach reach;
        public string targetName;
    }
    public class EventField : Component
    {
        #region Private Members
        EventFieldData eventFieldData;
        IList<EventGroup> eventFieldEventGroups;
        #endregion

        #region Public Members
        [FloatProperty(0, float.MaxValue)]
        public float HalfWidth
        {
            get { return eventFieldData.halfWidth; }
            set { eventFieldData.halfWidth = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float HalfHeight
        {
            get { return eventFieldData.halfHeight; }
            set { eventFieldData.halfHeight = value; }
        }
        [EnumProperty(typeof(FieldShape))]
        public FieldShape FieldShape
        {
            get { return eventFieldData.fieldShape; }
            set { eventFieldData.fieldShape = value; }
        }
        [EnumProperty(typeof(Reach))]
        public Reach Reach
        {
            get { return eventFieldData.reach; }
            set { eventFieldData.reach = value; }
        }
        [StringProperty(1, 15, true, true, false, false)]
        public string TargetName
        {
            get { return eventFieldData.targetName; }
            set { eventFieldData.targetName = value; }
        }
        public IList<EventGroup> EventFieldEventGroups { get { return eventFieldEventGroups; } }
        #endregion

        #region Constructor
        public EventField()
        {
            eventFieldData.targetName = string.Empty;
            eventFieldData.halfWidth = 50;
            eventFieldData.halfHeight = 50;
            eventFieldEventGroups = new GenericContainer<EventGroup>();
        }
        #endregion

        #region Private Methods
        void Update()
        {
            base.ExecuteExpression("HalfWidth");
            base.ExecuteExpression("HalfHeight");
            int count = 0;
            List<ParticleBase> results = ParticleManager.SearchByRect(Position.x - HalfWidth, Position.x + HalfWidth,
                Position.y - HalfHeight, Position.y + HalfHeight, out count);
            for (int i = 0;i < count; ++i)
            {
                if (results[i].IgnoreMask)
                    continue;

                switch (Reach)
                {
                    case Reach.Layer:
                        if (results[i].Emitter.LayerName != TargetName && results[i].Emitter.LayerName != LayerName)
                            continue;

                        break;
                    case Reach.Name:
                        if (results[i].Emitter.Name != TargetName)
                            continue;

                        break;
                }
                if (FieldShape == FieldShape.Circle)
                {
                    Vector2 v = Position - results[i].PPosition;
                    if (Math.Sqrt(v.x * v.x + v.y * v.y) > HalfWidth)
                        continue;
                }
                for (int k = 0; k < EventFieldEventGroups.Count; ++k)
                    EventFieldEventGroups[k].Execute(results[i], null);
            }
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as EventField;
            clone.eventFieldEventGroups = new GenericContainer<EventGroup>();
            foreach (var item in eventFieldEventGroups)
                clone.eventFieldEventGroups.Add(item.Clone() as EventGroup);

            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var eventFieldNode = (XmlElement)node.SelectSingleNode("EventField");
            //eventFieldData
            XmlHelper.BuildFromStruct(ref eventFieldData, eventFieldNode, "EventFieldData");
            //eventFieldEventGroups
            XmlHelper.BuildFromObjectList(eventFieldEventGroups, new EventGroup(), eventFieldNode, "EventFieldEventGroups");
            return eventFieldNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var eventFieldNode = doc.CreateElement("EventField");
            //eventFieldData
            XmlHelper.StoreStruct(eventFieldData, doc, eventFieldNode, "EventFieldData");
            //eventFieldEventGroups
            XmlHelper.StoreObjectList(eventFieldEventGroups, doc, eventFieldNode, "EventFieldEventGroups");
            node.AppendChild(eventFieldNode);
            return eventFieldNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var eventFieldBytes = new List<byte>();
            //eventFieldData
            PlayDataHelper.GenerateStruct(eventFieldData, eventFieldBytes);
            //eventFieldEventGroups
            PlayDataHelper.GenerateObjectList(eventFieldEventGroups, eventFieldBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(eventFieldBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader eventFieldReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(eventFieldReader))
                {
                    HalfWidth = dataReader.ReadSingle();
                    HalfHeight = dataReader.ReadSingle();
                    FieldShape = PlayDataHelper.ReadEnum<FieldShape>(dataReader);
                    Reach = PlayDataHelper.ReadEnum<Reach>(dataReader);
                    TargetName = PlayDataHelper.ReadString(dataReader);
                }
                //eventFieldEventGroups
                PlayDataHelper.LoadObjectList(EventFieldEventGroups, eventFieldReader, version);
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
            var initialState = base.initialState as EventField;
            HalfWidth = initialState.HalfWidth;
            HalfHeight = initialState.HalfHeight;
            FieldShape = initialState.FieldShape;
            Reach = initialState.Reach;
            TargetName = initialState.TargetName;
        }
        #endregion
    }
}
