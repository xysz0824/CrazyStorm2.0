/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
        public List<VMEventInfo> VMEvents { get; set; }
        public GenericContainer<string> Events { get; private set; }
        public List<byte[]> CompiledEvents { get; private set; }
        #endregion

        #region Constructor
        public EventGroup()
        {
            name = string.Empty;
            condition = string.Empty;
            Events = new GenericContainer<string>();
            CompiledEvents = new List<byte[]>();
            VMEvents = new List<VMEventInfo>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as EventGroup;
            clone.Events = new GenericContainer<string>();
            foreach (var item in Events) clone.Events.Add(item);
            clone.compiledCondition = null;
            clone.CompiledEvents = new List<byte[]>();
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
            XmlHelper.BuildFromList(Events, eventGroupNode, "Events");
            return eventGroupNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var eventGroupNode = doc.CreateElement("EventGroup");
            XmlHelper.StoreFields(this, doc, eventGroupNode);
            //events
            XmlHelper.StoreList(Events, doc, eventGroupNode, "Events");
            node.AppendChild(eventGroupNode);
            return eventGroupNode;
        }
        public List<byte> GeneratePlayData(File file)
        {
            var eventGroupBytes = new List<byte>();
            //compiledCondition
            if (compiledCondition != null)
            {
                eventGroupBytes.AddRange(BitConverter.GetBytes(compiledCondition.Length));
                eventGroupBytes.AddRange(compiledCondition);
            }
            else
                eventGroupBytes.AddRange(BitConverter.GetBytes(0));
            //compiledEvents
            for (int i = 0; i < CompiledEvents.Count; ++i)
            {
                eventGroupBytes.AddRange(BitConverter.GetBytes(CompiledEvents[i].Length));
                eventGroupBytes.AddRange(CompiledEvents[i]);
            }
            return PlayDataHelper.CreateBlock(eventGroupBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader eventGroupReader = PlayDataHelper.GetBlockReader(reader))
            {
                //compiledCondition
                int length = eventGroupReader.ReadInt32();
                if (length > 0) VMCondition = VM.Decode(eventGroupReader.ReadBytes(length));
                //compiledEvents
                while (!PlayDataHelper.EndOfReader(eventGroupReader))
                {
                    length = eventGroupReader.ReadInt32();
                    VMEvents.Add(EventHelper.BuildFromPlayData(eventGroupReader.ReadBytes(length)));
                }
            }
        }
        public void Execute(PropertyContainer propertyContainer, PropertyContainer bindingContainer, float frameScale)
        {
            if (VMCondition != null) VM.Execute(propertyContainer, VMCondition, frameScale);
            if (VMCondition == null || VM.PopBool())
            {
                for (int i = 0; i < VMEvents.Count; ++i)
                {
                    if (EventHelper.Execute(propertyContainer, bindingContainer, VMEvents[i], frameScale))
                        i = -1;
                }
            }
        }
        #endregion
    }
}