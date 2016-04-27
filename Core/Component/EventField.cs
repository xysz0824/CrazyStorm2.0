/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public enum Shape
    {
        Rectangle,
        Ellipse
    }
    public enum Reach
    {
        All,
        Layer,
        Name
    }
    public struct EventFieldData : IFieldData
    {
        public float halfWidth;
        public float halfHeight;
        public Shape shape;
        public Reach reach;
        public string targetName;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
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
        [EnumProperty(typeof(Shape))]
        public Shape Shape
        {
            get { return eventFieldData.shape; }
            set { eventFieldData.shape = value; }
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
            : base()
        {
            eventFieldData.targetName = "";
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
        #endregion
    }
}
