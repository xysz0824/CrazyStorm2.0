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
    public struct RebounderData : IFieldData
    {
        public int length;
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
        public int Length
        {
            get { return rebounderData.length; }
            set { rebounderData.length = value; }
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
            : base()
        {
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
        #endregion
    }
}
