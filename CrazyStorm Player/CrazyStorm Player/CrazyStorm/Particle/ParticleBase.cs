using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    public enum BlendType
    {
        AlphaBlend,
        Additive,
        Substraction,
        Multiply
    }
    abstract class ParticleBase : PropertyContainer, IPlayData, IRebuildReference<ParticleType>
    {
        public ParticleType Type { get; set; }
        public int TypeID { get; set; }
        public int MaxLife { get; set; }
        public int CurrentFrame { get; set; }
        public Vector2 PPosition { get; set; }
        public float WidthScale { get; set; }
        public RGB RGB { get; set; }
        public float Mass { get; set; }
        public float Opacity { get; set; }
        public float PSpeed { get; set; }
        public float PSpeedAngle { get; set; }
        public float PAcspeed { get; set; }
        public float PAcspeedAngle { get; set; }
        public float PRotation { get; set; }
        public BlendType BlendType { get; set; }
        public bool KillOutside { get; set; }
        public bool Collision { get; set; }
        public bool IgnoreMask { get; set; }
        public bool IgnoreRebound { get; set; }
        public bool IgnoreForce { get; set; }
        public bool FogEffect { get; set; }
        public bool FadeEffect { get; set; }
        public ParticleBase()
        {
            TypeID = -1;
        }
        public virtual void LoadPlayData(BinaryReader reader)
        {
            using (BinaryReader particleBaseReader = PlayDataHelper.GetBlockReader(reader))
            {
                //properties
                base.BuildFromPlayData(particleBaseReader);
                TypeID = particleBaseReader.ReadInt32();
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(particleBaseReader))
                {
                    MaxLife = dataReader.ReadInt32();
                    CurrentFrame = dataReader.ReadInt32();
                    PPosition = PlayDataHelper.ReadVector2(dataReader);
                    WidthScale = dataReader.ReadSingle();
                    RGB = PlayDataHelper.ReadRGB(dataReader);
                    Mass = dataReader.ReadSingle();
                    Opacity = dataReader.ReadSingle();
                    PSpeed = dataReader.ReadSingle();
                    PSpeedAngle = dataReader.ReadSingle();
                    PAcspeed = dataReader.ReadSingle();
                    PAcspeedAngle = dataReader.ReadSingle();
                    PRotation = dataReader.ReadSingle();
                    BlendType = PlayDataHelper.ReadEnum<BlendType>(dataReader);
                    KillOutside = dataReader.ReadBoolean();
                    Collision = dataReader.ReadBoolean();
                    IgnoreMask = dataReader.ReadBoolean();
                    IgnoreRebound = dataReader.ReadBoolean();
                    IgnoreForce = dataReader.ReadBoolean();
                    FogEffect = dataReader.ReadBoolean();
                    FadeEffect = dataReader.ReadBoolean();
                }
            }
        }
        public void RebuildReferenceFromCollection(IList<ParticleType> collection)
        {
            if (TypeID != -1)
            {
                foreach (var target in collection)
                {
                    if (TypeID == target.Id)
                    {
                        Type = target;
                        break;
                    }
                }
            }
        }
    }
}
