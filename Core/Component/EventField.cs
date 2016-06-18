/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            eventFieldEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as EventField;
            clone.eventFieldEventGroups = new ObservableCollection<EventGroup>();
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
        #endregion
    }
}
