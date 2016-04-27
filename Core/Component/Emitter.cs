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
    public struct EmitterData : IFieldData
    {
        public Vector2 emitPosition;
        public int emitCount;
        public int emitCycle;
        public float emitAngle;
        public float emitRange;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public class Emitter : Component
    {
        #region Private Members
        EmitterData emitterData;
        IList<EventGroup> particleEventGroups;
        #endregion

        #region Public Members
        [Vector2Property]
        public Vector2 EmitPosition
        {
            get { return emitterData.emitPosition; }
            set { emitterData.emitPosition = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int EmitCount
        {
            get { return emitterData.emitCount; }
            set { emitterData.emitCount = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int EmitCycle
        {
            get { return emitterData.emitCycle; }
            set { emitterData.emitCycle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitAngle
        {
            get { return emitterData.emitAngle; }
            set { emitterData.emitAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitRange
        {
            get { return emitterData.emitRange; }
            set { emitterData.emitRange = value; }
        }
        public IList<EventGroup> ParticleEventGroups { get { return particleEventGroups; } }
        #endregion

        #region Constructor
        public Emitter()
            : base()
        {
            emitterData.emitCount = 1;
            emitterData.emitCycle = 1;
            particleEventGroups = new ObservableCollection<EventGroup>();
        }
        #endregion

        #region Public Methods
        public override object Clone()
        {
            var clone = base.Clone() as Emitter;
            clone.particleEventGroups = new ObservableCollection<EventGroup>();
            foreach (var item in particleEventGroups)
                clone.particleEventGroups.Add(item.Clone() as EventGroup);

            return clone;
        }
        #endregion
    }
}
