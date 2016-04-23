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
    public class EventField : Component
    {
        #region Private Members
        float halfWidth;
        float halfHeight;
        Shape shape;
        Reach reach;
        string targetName;
        ObservableCollection<EventGroup> eventFieldEventGroups;
        #endregion

        #region Public Members
        [FloatProperty(0, float.MaxValue)]
        public float HalfWidth
        {
            get { return halfWidth; }
            set { halfWidth = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float HalfHeight
        {
            get { return halfHeight; }
            set { halfHeight = value; }
        }
        [EnumProperty(typeof(Shape))]
        public Shape Shape
        {
            get { return shape; }
            set { shape = value; }
        }
        [EnumProperty(typeof(Reach))]
        public Reach Reach
        {
            get { return reach; }
            set { reach = value; }
        }
        [StringProperty(1, 15, true, true, false, false)]
        public string TargetName
        {
            get { return targetName; }
            set { targetName = value; }
        }
        public ObservableCollection<EventGroup> EventFieldEventGroups { get { return eventFieldEventGroups; } }
        #endregion

        #region Constructor
        public EventField()
            : base()
        {
            targetName = "";
            eventFieldEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion
    }
}
