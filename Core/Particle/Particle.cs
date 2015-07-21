using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CrazyStorm.Core
{
    public class Particle : PropertyContainer
    {
        #region Private Members
        int maxLife;
        ParticleType type;
        float widthScale;
        float heightScale;
        Vector3 rgb;
        float opacity;
        float rotation;
        bool stickToSpeedAngle;
        bool retainScale;
        float speed;
        float speedAngle;
        float acspeed;
        float acspeedAngle;
        bool fogEffect;
        bool fadeEffect;
        bool afterimageEffect;
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
        [FloatProperty(0, 1)]
        public float HeightScale
        {
            get { return heightScale; }
            set { heightScale = value; }
        }
        [Vector3Property]
        public Vector3 RGB
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
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        [BoolProperty]
        public bool StickToSpeedAngle
        {
            get { return stickToSpeedAngle; }
            set { stickToSpeedAngle = value; }
        }
        [BoolProperty]
        public bool RetainScale
        {
            get { return retainScale; }
            set { retainScale = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float SpeedAngle
        {
            get { return speedAngle; }
            set { speedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Acspeed
        {
            get { return acspeed; }
            set { acspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float AcspeedAngle
        {
            get { return acspeedAngle; }
            set { acspeedAngle = value; }
        }
        [BoolProperty]
        public bool FogEffect
        {
            get { return fogEffect; }
            set { fogEffect = value; }
        }
        [BoolProperty]
        public bool FadeEffect
        {
            get { return fadeEffect; }
            set { fadeEffect = value; }
        }
        [BoolProperty]
        public bool AfterimageEffect
        {
            get { return afterimageEffect; }
            set { afterimageEffect = value; }
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
        public Particle()
        {
        }
        #endregion

        #region Public Methods
        #endregion
    }
}
