/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class EventGroup : IXmlData
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
        public IList<string> OriginalEvents { get { return originalEvents; } }
        public IList<string> TranslatedEvents { get { return translatedEvents; } }
        public IList<byte[]> CompiledEvents { get { return compiledEvents; } }
        #endregion

        #region Constructor
        public EventGroup()
        {
            name = "NewEventGroup";
            condition = string.Empty;
            originalEvents = new ObservableCollection<string>();
            translatedEvents = new ObservableCollection<string>();
            compiledEvents = new List<byte[]>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as EventGroup;
            clone.originalEvents = new ObservableCollection<string>();
            foreach (var item in originalEvents)
                clone.originalEvents.Add(item);

            clone.translatedEvents = new ObservableCollection<string>();
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
        #endregion
    }
}