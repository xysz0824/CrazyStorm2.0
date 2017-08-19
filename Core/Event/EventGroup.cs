/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CrazyStorm.Core
{
    public class EventGroup : IXmlData, IGeneratePlayData, ILoadPlayData
    {
        #region Private Members
        [XmlAttribute]
        string name;
        [XmlAttribute]
        string condition;
        byte[] compiledCondition;
        IList<string> originalEvents;
        IList<string> translatedEvents;
        IList<byte[]> compiledEvents;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public string Condition
        {
            get { return condition; }
            set { condition = value; }
        }
        public byte[] CompiledCondition
        {
            get { return compiledCondition; }
            set { compiledCondition = value; }
        }
        public VMInstruction[] VMCondition { get; set; }
        public IList<VMEventInfo> VMEvents { get; set; }
        public IList<string> OriginalEvents { get { return originalEvents; } }
        public IList<string> TranslatedEvents { get { return translatedEvents; } }
        public IList<byte[]> CompiledEvents { get { return compiledEvents; } }
        #endregion

        #region Constructor
        public EventGroup()
        {
            name = "NewEventGroup";
            condition = string.Empty;
            originalEvents = new GenericContainer<string>();
            translatedEvents = new GenericContainer<string>();
            compiledEvents = new List<byte[]>();
            VMEvents = new List<VMEventInfo>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as EventGroup;
            clone.originalEvents = new GenericContainer<string>();
            foreach (var item in originalEvents)
                clone.originalEvents.Add(item);

            clone.translatedEvents = new GenericContainer<string>();
            foreach (var item in translatedEvents)
                clone.translatedEvents.Add(item);

            clone.compiledCondition = null;
            clone.compiledEvents = new List<byte[]>();
            return clone;
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "EventGroup";
            var eventGroupNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                eventGroupNode = node;

            XmlHelper.BuildFromFields(this, eventGroupNode);
            //events
            XmlHelper.BuildFromList(originalEvents, eventGroupNode, "Events");
            return eventGroupNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var eventGroupNode = doc.CreateElement("EventGroup");
            XmlHelper.StoreFields(this, doc, eventGroupNode);
            //events
            XmlHelper.StoreList(originalEvents, doc, eventGroupNode, "Events");
            node.AppendChild(eventGroupNode);
            return eventGroupNode;
        }
        public List<byte> GeneratePlayData()
        {
            var eventGroupBytes = new List<byte>();
            //compiledCondition
            if (compiledCondition != null)
            {
                eventGroupBytes.AddRange(PlayDataHelper.GetBytes(compiledCondition.Length));
                eventGroupBytes.AddRange(compiledCondition);
            }
            else
                eventGroupBytes.AddRange(PlayDataHelper.GetBytes(0));
            //compiledEvents
            for (int i = 0; i < compiledEvents.Count; ++i)
            {
                eventGroupBytes.AddRange(PlayDataHelper.GetBytes(CompiledEvents[i].Length));
                eventGroupBytes.AddRange(compiledEvents[i]);
            }
            return PlayDataHelper.CreateBlock(eventGroupBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader eventGroupReader = PlayDataHelper.GetBlockReader(reader))
            {
                //compiledCondition
                int length = eventGroupReader.ReadInt32();
                if (length > 0)
                    VMCondition = VM.Decode(eventGroupReader.ReadBytes(length));

                //compiledEvents
                while (!PlayDataHelper.EndOfReader(eventGroupReader))
                {
                    length = eventGroupReader.ReadInt32();
                    VMEvents.Add(EventHelper.BuildFromPlayData(eventGroupReader.ReadBytes(length)));
                }
            }
        }
        public void Execute(PropertyContainer propertyContainer)
        {
            if (VMCondition != null)
                VM.Execute(propertyContainer, VMCondition);

            if (VMCondition == null || VM.PopBool())
            {
                for (int i = 0; i < VMEvents.Count; ++i)
                {
                    if (EventHelper.Execute(propertyContainer, VMEvents[i]))
                        i = -1;
                }
            }
        }
        #endregion
    }
}