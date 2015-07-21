/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class Emitter : Component
    {
        #region Private Members
        Vector2 emitPosition;
        int emitCount;
        int emitCycle;
        float emitAngle;
        float emitRange;
        #endregion

        #region Public Members
        [Vector2Property]
        public Vector2 EmitPosition
        {
            get { return emitPosition; }
            set { emitPosition = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int EmitCount
        {
            get { return emitCount; }
            set { emitCount = value; }
        }
        [IntProperty(1, int.MaxValue)]
        public int EmitCycle
        {
            get { return emitCycle; }
            set { emitCycle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitAngle
        {
            get { return emitAngle; }
            set { emitAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float EmitRange
        {
            get { return emitRange; }
            set { emitRange = value; }
        }
        #endregion

        #region Constructor
        public Emitter()
            : base()
        {
            emitCount = 1;
            EmitCycle = 1;
        }
        #endregion
    }
}
