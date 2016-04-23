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
    public enum ForceType
    {
        Direction,
        Inner,
        Outer
    }
    public class ForceField : Component
    {
        #region Private Members
        float halfWidth;
        float halfHeight;
        Shape shape;
        Reach reach;
        string targetName;
        float intensity;
        float directionAngle;
        ForceType forceType;
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
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float DirectionAngle
        {
            get { return directionAngle; }
            set { directionAngle = value; }
        }
        [EnumProperty(typeof(ForceType))]
        public ForceType ForceType
        {
            get { return forceType; }
            set { forceType = value; }
        }
        #endregion

        #region Constructor
        public ForceField()
            : base()
        {
            targetName = "";
        }
        #endregion
    }
}
