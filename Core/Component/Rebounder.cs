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
    public class Rebounder : Component
    {
        #region Private Members
        int length;
        float rotation;
        int collisionTime;
        ObservableCollection<EventGroup> rebounderEventGroups;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int CollisionTime
        {
            get { return collisionTime; }
            set { collisionTime = value; }
        }
        public ObservableCollection<EventGroup> RebounderEventGroups { get { return rebounderEventGroups; } }
        #endregion

        #region Constructor
        public Rebounder()
            : base()
        {
            rebounderEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion
    }
}
