/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
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
        Vector2 pspeedVector;
        Vector2 pacspeedVector;
        public Emitter Emitter { get; set; }
        public ParticleQuadTree QuadTree { get; set; }
        public bool Alive { get; set; }
        public ParticleType Type { get; set; }
        public int TypeID { get; set; }
        public int MaxLife { get; set; }
        public int PCurrentFrame { get; set; }
        public Vector2 PPosition { get; set; }
        public float WidthScale { get; set; }
        public RGB RGB { get; set; }
        public float Mass { get; set; }
        public float Opacity { get; set; }
        public Vector2 PSpeedVector
        {
            get { return pspeedVector; }
            set 
            { 
                pspeedVector = value;
                if (pspeedVector != Vector2.Zero)
                    PSpeedAngle = MathHelper.GetDegree(pspeedVector);
            }
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
            ParticleEventGroups = new List<EventGroup>();
        }
        public virtual void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader particleBaseReader = PlayDataHelper.GetBlockReader(reader))
            {
                //properties
                base.LoadPropertyExpressions(particleBaseReader);
                TypeID = particleBaseReader.ReadInt32();
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(particleBaseReader))
                {
                    MaxLife = dataReader.ReadInt32();
                    PCurrentFrame = dataReader.ReadInt32();
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
        public override bool PushProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "MaxLife":
                    VM.PushInt(MaxLife);
                    return true;
                case "PCurrentFrame":
                    VM.PushInt(PCurrentFrame);
                    return true;
                case "PPosition":
                    VM.PushVector2(PPosition);
                    return true;
                case "PPosition.x":
                    VM.PushFloat(PPosition.x);
                    return true;
                case "PPosition.y":
                    VM.PushFloat(PPosition.y);
                    return true;
                case "WidthScale":
                    VM.PushFloat(WidthScale);
                    return true;
                case "RGB":
                    VM.PushRGB(RGB);
                    return true;
                case "RGB.r":
                    VM.PushFloat(RGB.r);
                    return true;
                case "RGB.g":
                    VM.PushFloat(RGB.g);
                    return true;
                case "RGB.b":
                    VM.PushFloat(RGB.b);
                    return true;
                case "Mass":
                    VM.PushFloat(Mass);
                    return true;
                case "Opacity":
                    VM.PushFloat(Opacity);
                    return true;
                case "PSpeed":
                    VM.PushFloat(PSpeed);
                    return true;
                case "PSpeedAngle":
                    VM.PushFloat(PSpeedAngle);
                    return true;
                case "PAcspeed":
                    VM.PushFloat(PAcspeed);
                    return true;
                case "PAcspeedAngle":
                    VM.PushFloat(PAcspeedAngle);
                    return true;
                case "PRotation":
                    VM.PushFloat(PRotation);
                    return true;
                case "BlendType":
                    VM.PushEnum((int)BlendType);
                    return true;
                case "KillOutside":
                    VM.PushBool(KillOutside);
                    return true;
                case "Collision":
                    VM.PushBool(Collision);
                    return true;
                case "IgnoreMask":
                    VM.PushBool(IgnoreMask);
                    return true;
                case "IgnoreRebound":
                    VM.PushBool(IgnoreRebound);
                    return true;
                case "IgnoreForce":
                    VM.PushBool(IgnoreForce);
                    return true;
                case "FogEffect":
                    VM.PushBool(FogEffect);
                    return true;
                case "FadeEffect":
                    VM.PushBool(FadeEffect);
                    return true;
                default:
                    for (int i = 0; i < Emitter.Variables.Count; ++i)
                        if (Emitter.Variables[i].Label == propertyName)
                        {
                            VM.PushFloat(Emitter.Variables[i].Value);
                            return true;
                        }

                    for (int i = 0; i < Emitter.Globals.Count; ++i)
                        if (Emitter.Globals[i].Label == propertyName)
                        {
                            VM.PushFloat(Emitter.Globals[i].Value);
                            return true;
                        }

                    return false;
            }
        }
        public override bool SetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "MaxLife":
                    MaxLife = VM.PopInt();
                    return true;
                case "PCurrentFrame":
                    PCurrentFrame = VM.PopInt();
                    return true;
                case "PPosition":
                    PPosition = VM.PopVector2();
                    return true;
                case "PPosition.x":
                    PPosition = new Vector2(VM.PopFloat(), PPosition.y);
                    return true;
                case "PPosition.y":
                    PPosition = new Vector2(PPosition.x, VM.PopFloat());
                    return true;
                case "WidthScale":
                    WidthScale = VM.PopFloat();
                    return true;
                case "RGB":
                    RGB = VM.PopRGB();
                    return true;
                case "RGB.r":
                    RGB = new RGB(VM.PopFloat(), RGB.g, RGB.b);
                    return true;
                case "RGB.g":
                    RGB = new RGB(RGB.r, VM.PopFloat(), RGB.b);
                    return true;
                case "RGB.b":
                    RGB = new RGB(RGB.r, RGB.g, VM.PopFloat());
                    return true;
                case "Mass":
                    Mass = VM.PopFloat();
                    return true;
                case "Opacity":
                    Opacity = VM.PopFloat();
                    return true;
                case "PSpeed":
                    PSpeed = VM.PopFloat();
                    MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle);
                    return true;
                case "PSpeedAngle":
                    PSpeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle);
                    return true;
                case "PAcspeed":
                    PAcspeed = VM.PopFloat();
                    MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle);
                    return true;
                case "PAcspeedAngle":
                    PAcspeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle);
                    return true;
                case "PRotation":
                    PRotation = VM.PopFloat();
                    return true;
                case "BlendType":
                    BlendType = (BlendType)VM.PopEnum();
                    return true;
                case "KillOutside":
                    KillOutside = VM.PopBool();
                    return true;
                case "Collision":
                    Collision = VM.PopBool();
                    return true;
                case "IgnoreMask":
                    IgnoreMask = VM.PopBool();
                    return true;
                case "IgnoreRebound":
                    IgnoreRebound = VM.PopBool();
                    return true;
                case "IgnoreForce":
                    IgnoreForce = VM.PopBool();
                    return true;
                case "FogEffect":
                    FogEffect = VM.PopBool();
                    return true;
                case "FadeEffect":
                    FadeEffect = VM.PopBool();
                    return true;
                default:
                    for (int i = 0; i < Emitter.Variables.Count; ++i)
                        if (Emitter.Variables[i].Label == propertyName)
                        {
                            Emitter.Variables[i].Value = VM.PopFloat();
                            return true;
                        }

                    for (int i = 0; i < Emitter.Globals.Count; ++i)
                        if (Emitter.Globals[i].Label == propertyName)
                        {
                            Emitter.Globals[i].Value = VM.PopFloat();
                            return true;
                        }

                    return false;
            }
        }
        public virtual void Update()
        {
            if (PCurrentFrame >= MaxLife || (KillOutside && ParticleManager.OutOfWindow(PPosition.x, PPosition.y)))
            {
                Alive = false;
                Emitter.Particles.Remove(this);
                return;
            }
            if (PCurrentFrame == 0)
            {
                MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle);
                MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle);
            }
            QuadTree.Update(this);
            PSpeedVector += PAcspeedVector;
            PPosition += PSpeedVector;
            for (int i = 0; i < ParticleEventGroups.Count; ++i)
                ParticleEventGroups[i].Execute(this);

            ++PCurrentFrame;
        }
        public virtual void CopyTo(ParticleBase particleBase)
        {
            particleBase.Emitter = Emitter;
            particleBase.Alive = Alive;
            particleBase.Type = Type;
            particleBase.TypeID = TypeID;
            particleBase.MaxLife = MaxLife;
            particleBase.PCurrentFrame = PCurrentFrame;
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
