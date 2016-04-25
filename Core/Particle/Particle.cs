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
        int currentFrame;
        Vector2 position;
        ParticleType type;
        float widthScale;
        float heightScale;
        RGB rgb;
        float opacity;
        float rotation;
        bool stickToSpeedAngle;
        bool retainScale;
        float pspeed;
        float pspeedAngle;
        float pacspeed;
        float pacspeedAngle;
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
        public int CurrentFrame
        {
            get { return currentFrame; }
            set { currentFrame = value >= 0 && value < maxLife ? value : currentFrame; }
        }
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public float X
        {
            get { return position.x; }
            set { position.x = value; }
        }
        public float Y
        {
            get { return position.y; }
            set { position.y = value; }
        }
        public ParticleType Type
        {
            get { return type; }
            set { type = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float WidthScale
        {
            get { return widthScale; }
            set { widthScale = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float HeightScale
        {
            get { return heightScale; }
            set { heightScale = value; }
        }
        [RGBProperty]
        public RGB RGB
        {
            get { return rgb; }
            set { rgb = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
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
    }
}
