using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class CurveParticle : PropertyContainer
    {
        #region Private Members
        int maxLife;
        ParticleType type;
        float widthScale;
        int length;
        RGB rgb;
        float opacity;
        float pspeed;
        float pspeedAngle;
        float pacspeed;
        float pacspeedAngle;
        BlendType blendType;
        bool killOutside;
        bool collision;
        bool ignoreMask;
        bool ignoreRebound;
        bool ignoreForce;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int MaxLife
        {
            get { return maxLife; }
            set { maxLife = value; }
        }
        public ParticleType Type
        {
            get { return type; }
            set { type = value; }
        }
        [FloatProperty(0, 1)]
        public float WidthScale
        {
            get { return widthScale; }
            set { widthScale = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        [Vector3Property]
        public RGB RGB
        {
            get { return rgb; }
            set { rgb = value; }
        }
        [FloatProperty(0, 255)]
        public float Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PSpeed
        {
            get { return pspeed; }
            set { pspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PSpeedAngle
        {
            get { return pspeedAngle; }
            set { pspeedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PAcspeed
        {
            get { return pacspeed; }
            set { pacspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PAcspeedAngle
        {
            get { return pacspeedAngle; }
            set { pacspeedAngle = value; }
        }
        [EnumProperty(typeof(BlendType))]
        public BlendType BlendType
        {
            get { return blendType; }
            set { blendType = value; }
        }
        [BoolProperty]
        public bool KillOutside
        {
            get { return killOutside; }
            set { killOutside = value; }
        }
        [BoolProperty]
        public bool Collision
        {
            get { return collision; }
            set { collision = value; }
        }
        [BoolProperty]
        public bool IgnoreMask
        {
            get { return ignoreMask; }
            set { ignoreMask = value; }
        }
        [BoolProperty]
        public bool IgnoreRebound
        {
            get { return ignoreRebound; }
            set { ignoreRebound = value; }
        }
        [BoolProperty]
        public bool IgnoreForce
        {
            get { return ignoreForce; }
            set { ignoreForce = value; }
        }
        #endregion

        #region Constructor
        public CurveParticle()
        {
        }
        #endregion

        #region Public Methods
        #endregion
    }
}
