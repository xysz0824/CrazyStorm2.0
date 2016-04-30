﻿/*
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
    public enum RebounderShape
    {
        Line,
        Circle
    }
    public struct RebounderData : IFieldData
    {
        public int size;
        public RebounderShape shape;
        public float rotation;
        public int collisionTime;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public class Rebounder : Component
    {
        #region Private Members
        RebounderData rebounderData;
        IList<EventGroup> rebounderEventGroups;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int Size
        {
            get { return rebounderData.size; }
            set { rebounderData.size = value; }
        }
        [EnumProperty(typeof(RebounderShape))]
        public RebounderShape Shape
        {
            get { return rebounderData.shape; }
            set { rebounderData.shape = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Rotation
        {
            get { return rebounderData.rotation; }
            set { rebounderData.rotation = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int CollisionTime
        {
            get { return rebounderData.collisionTime; }
            set { rebounderData.collisionTime = value; }
        }
        public IList<EventGroup> RebounderEventGroups { get { return rebounderEventGroups; } }
        #endregion

        #region Constructor
        public Rebounder()
        {
            rebounderData.size = 50;
            rebounderEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Rebounder;
            clone.rebounderEventGroups = new ObservableCollection<EventGroup>();
            foreach (var item in rebounderEventGroups)
                clone.rebounderEventGroups.Add(item.Clone() as EventGroup);

            return clone;
        }
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var rebounderNode = (XmlElement)node.SelectSingleNode("Rebounder");
            //rebounderData
            XmlHelper.BuildStruct(ref rebounderData, rebounderNode, "RebounderData");
            //rebounderEventGroups
            XmlHelper.BuildObjectList(rebounderEventGroups, new EventGroup(), rebounderNode, "RebounderEventGroups");
            return rebounderNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var rebounderNode = doc.CreateElement("Rebounder");
            //rebounderData
            XmlHelper.StoreStruct(rebounderData, doc, rebounderNode, "RebounderData");
            //rebounderEventGroups
            XmlHelper.StoreObjectList(rebounderEventGroups, doc, rebounderNode, "RebounderEventGroups");
            node.AppendChild(rebounderNode);
            return rebounderNode;
        }
        #endregion
    }
}
