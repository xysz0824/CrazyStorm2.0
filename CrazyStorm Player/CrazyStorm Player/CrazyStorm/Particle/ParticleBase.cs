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
        private Vector2 pspeedVector;
        private Vector2 pacspeedVector;
        public Emitter Emitter { get; set; }
        public ParticleQuadTree QuadTree { get; set; }
        public bool Alive { get; set; }
        public ParticleType Type { get; set; }
        public int TypeID { get; set; }
        public int MaxLife { get; set; }
        public int CurrentFrame { get; set; }
        public Vector2 PPosition { get; set; }
        public float WidthScale { get; set; }
        public RGB RGB { get; set; }
        public float Mass { get; set; }
        public float Opacity { get; set; }
        public Vector2 PSpeedVector
        {
            get { return pspeedVector; }
            set { pspeedVector = value; }
        }
        public Vector2 PAcspeedVector
        {
            get { return pacspeedVector; }
            set { pacspeedVector = value; }
        }
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
        public IList<EventGroup> ParticleEventGroups { get; set; }
        public ParticleBase()
        {
            TypeID = -1;
        }
        public virtual void LoadPlayData(BinaryReader reader)
        {
            using (BinaryReader particleBaseReader = PlayDataHelper.GetBlockReader(reader))
            {
                //properties
                base.LoadPropertyExpressions(particleBaseReader);
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
        public virtual void Update()
        {
            if (CurrentFrame >= MaxLife)
            {
                Alive = false;
                return;
            }
            if (CurrentFrame == 0)
            {
                MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle);
                MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle);
            }
            QuadTree.Update(this);
            PSpeedVector += PAcspeedVector;
            PPosition += PSpeedVector;
            double vf = 0;
            if (PSpeedVector.y != 0)
            {
                vf = Math.PI / 2 - Math.Atan(PSpeedVector.x / PSpeedVector.y);
                if (PSpeedVector.y < 0)
                    vf += Math.PI;
            }
            else
            {
                vf = PSpeedVector.x >= 0 ? 0 : Math.PI;
            }
            PSpeedAngle = (float)MathHelper.RadToDeg(vf);
            for (int i = 0; i < ParticleEventGroups.Count; ++i)
                ParticleEventGroups[i].Execute();

            ++CurrentFrame;
        }
        public virtual void Copy(ParticleBase particleBase)
        {
            particleBase.Alive = Alive;
            particleBase.Type = Type;
            particleBase.TypeID = TypeID;
            particleBase.MaxLife = MaxLife;
            particleBase.CurrentFrame = CurrentFrame;
            particleBase.PPosition = PPosition;
            particleBase.WidthScale = WidthScale;
            particleBase.RGB = RGB;
            particleBase.Mass = Mass;
            particleBase.Opacity = Opacity;
            particleBase.PSpeed = PSpeed;
            particleBase.PSpeedAngle = PSpeedAngle;
            particleBase.PAcspeed = PAcspeed;
            particleBase.PAcspeedAngle = PAcspeedAngle;
            particleBase.PRotation = PRotation;
            particleBase.BlendType = BlendType;
            particleBase.KillOutside = KillOutside;
            particleBase.Collision = Collision;
            particleBase.IgnoreMask = IgnoreMask;
            particleBase.IgnoreRebound = IgnoreRebound;
            particleBase.IgnoreForce = IgnoreForce;
            particleBase.FogEffect = FogEffect;
            particleBase.FadeEffect = FadeEffect;
        }
    }
}
