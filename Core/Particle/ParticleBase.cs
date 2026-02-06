/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml;

namespace CrazyStorm.Core
{
    public enum BlendType
    {
        AlphaBlend,
        Additive,
        Substraction,
        Multiply,
        None
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ParticleBaseData
    {
        public int maxLife;
        public float widthScale;
        public RGB rgb;
        public float mass;
        public float opacity;
        public float pspeed;
        public float pacspeed;
        public float pacspeedAngle;
        public float pspeedHScale;
        public float pspeedVScale;
        public float protation;
        public BlendType blendType;
        public bool killOutside;
        public bool collision;
        public bool ignoreMask;
        public bool ignoreRebound;
        public bool ignoreForce;
        public bool fogEffect;
        public bool fadeEffect;
        public float vspeed;
    }
    public abstract class ParticleBase : PropertyContainer, IXmlData, ILoadPlayData, 
        IComparable<ParticleBase>, IPlayable
    {
        public const float FOG_TIME = 10;
        #region Private Members
        Vector2 pspeedVector;
        Vector2 pacspeedVector;
        ParticleType type;
        int typeID = -1;
        ParticleBaseData particleBaseData;
        #endregion

        #region Public Members
        public long RenderOrder { get; set; }
        public bool Alive { get; set; }
        public float FogFrame { get; private set; }
        public Emitter Emitter { get; set; }
        //public ParticleQuadTree QuadTree { get; set; }
        [IntProperty(100, 1, int.MaxValue)]
        public int MaxLife
        {
            get { return particleBaseData.maxLife; }
            set { particleBaseData.maxLife = value; }
        }
        [RuntimeProperty(101)]
        public float PLayerFrame => Emitter != null ? Emitter.LayerFrame : 1;
        [RuntimeProperty(102)]
        public float PCurrentFrame { get; set; }
        public float PAnimateFrame { get; private set; }
        [RuntimeProperty(103)]
        public bool PMasked { get; set; }
        [RuntimeProperty(104)]
        public Vector2 PPosition { get; set; }
        public Vector2 PPositionLast { get; set; }
        public ParticleType Type
        {
            get { return type; }
            set 
            {
                if (type != value) PAnimateFrame = 1;
                type = value; 
            }
        }
        [RGBProperty(107)]
        public RGB RGB
        {
            get { return particleBaseData.rgb; }
            set { particleBaseData.rgb = value; }
        }
        [FloatProperty(111, 0, float.MaxValue)]
        public float Mass
        {
            get { return particleBaseData.mass; }
            set { particleBaseData.mass = value; }
        }
        [FloatProperty(112, 0, float.MaxValue)]
        public float Opacity
        {
            get { return particleBaseData.opacity; }
            set { particleBaseData.opacity = value; }
        }
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
        [FloatProperty(113, float.MinValue, float.MaxValue)]
        public float PSpeed
        {
            get { return particleBaseData.pspeed; }
            set { particleBaseData.pspeed = value; }
        }
        [RuntimeProperty(114)]
        public float PSpeedAngle { get; set; }
        [FloatProperty(115, float.MinValue, float.MaxValue)]
        public float PAcspeed
        {
            get { return particleBaseData.pacspeed; }
            set { particleBaseData.pacspeed = value; }
        }
        [FloatProperty(116, float.MinValue, float.MaxValue)]
        public float PAcspeedAngle
        {
            get { return particleBaseData.pacspeedAngle; }
            set { particleBaseData.pacspeedAngle = value; }
        }
        [FloatProperty(117, float.MinValue, float.MaxValue)]
        public float PSpeedHScale
        {
            get { return particleBaseData.pspeedHScale; }
            set { particleBaseData.pspeedHScale = value; }
        }
        [FloatProperty(118, float.MinValue, float.MaxValue)]
        public float PSpeedVScale
        {
            get { return particleBaseData.pspeedVScale; }
            set { particleBaseData.pspeedVScale = value; }
        }
        [FloatProperty(119, float.MinValue, float.MaxValue)]
        public float PRotation
        {
            get { return particleBaseData.protation; }
            set { particleBaseData.protation = value; }
        }
        [EnumProperty(120, typeof(BlendType))]
        public BlendType BlendType
        {
            get { return particleBaseData.blendType; }
            set { particleBaseData.blendType = value; }
        }
        [BoolProperty(121)]
        public bool KillOutside
        {
            get { return particleBaseData.killOutside; }
            set { particleBaseData.killOutside = value; }
        }
        [BoolProperty(122)]
        public bool Collision
        {
            get { return particleBaseData.collision; }
            set { particleBaseData.collision = value; }
        }
        [BoolProperty(123)]
        public bool IgnoreMask
        {
            get { return particleBaseData.ignoreMask; }
            set { particleBaseData.ignoreMask = value; }
        }
        [BoolProperty(124)]
        public bool IgnoreRebound
        {
            get { return particleBaseData.ignoreRebound; }
            set { particleBaseData.ignoreRebound = value; }
        }
        [BoolProperty(125)]
        public bool IgnoreForce
        {
            get { return particleBaseData.ignoreForce; }
            set { particleBaseData.ignoreForce = value; }
        }
        [FloatProperty(126, float.MinValue, float.MaxValue)]
        public float WidthScale
        {
            get { return particleBaseData.widthScale; }
            set { particleBaseData.widthScale = value; }
        }
        [BoolProperty(127)]
        public bool FogEffect
        {
            get { return particleBaseData.fogEffect; }
            set { particleBaseData.fogEffect = value; }
        }
        [BoolProperty(128)]
        public bool FadeEffect
        {
            get { return particleBaseData.fadeEffect; }
            set { particleBaseData.fadeEffect = value; }
        }
        [FloatProperty(129, float.MinValue, float.MaxValue)]
        public float VSpeed
        {
            get { return particleBaseData.vspeed; }
            set { particleBaseData.vspeed = value; }
        }
        public int ReboundTime { get; set; }
        public List<EventGroup> ParticleEventGroups { get; set; }
        #endregion

        #region Constructor
        public ParticleBase()
        {
            RenderOrder = int.MaxValue;
            PCurrentFrame = 1;
            PAnimateFrame = 1;
            particleBaseData.maxLife = 200;
            particleBaseData.widthScale = 1;
            particleBaseData.rgb = new RGB(255, 255, 255);
            particleBaseData.mass = 1;
            particleBaseData.opacity = 100;
            particleBaseData.pspeed = 5;
            particleBaseData.pspeedHScale = 1;
            particleBaseData.pspeedVScale = 1;
            particleBaseData.killOutside = true;
            particleBaseData.collision = true;
            particleBaseData.fogEffect = true;
            particleBaseData.fadeEffect = true;
        }
        #endregion

        #region Public Methods
        public virtual XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "ParticleBase";
            var particleBaseNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                particleBaseNode = node;

            //properties
            base.BuildFromXmlElement(particleBaseNode);
            //type
            if (particleBaseNode.HasAttribute("type"))
            {
                string typeAttribute = particleBaseNode.GetAttribute("type");
                int parsedID;
                if (int.TryParse(typeAttribute, out parsedID))
                    typeID = parsedID;
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
            //particleBaseData
            XmlHelper.BuildFromStruct(ref particleBaseData, particleBaseNode);
            return particleBaseNode;
        }
        public virtual XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var particleBaseNode = doc.CreateElement("ParticleBase");
            //properties
            particleBaseNode.AppendChild(base.GetXmlElement(doc));
            //type
            if (type != null)
            {
                var typeAttribute = doc.CreateAttribute("type");
                typeAttribute.Value = type.ID.ToString();
                particleBaseNode.Attributes.Append(typeAttribute);
            }
            //particleBaseData
            XmlHelper.StoreStruct(particleBaseData, doc, particleBaseNode);
            node.AppendChild(particleBaseNode);
            return particleBaseNode;
        }
        public void RebuildReferenceFromCollection(List<ParticleType> collection)
        {
            //type
            if (typeID != -1)
            {
                foreach (var target in collection)
                {
                    if (typeID == target.ID)
                    {
                        type = target;
                        break;
                    }
                }
                typeID = -1;
            }
        }
        public virtual List<byte> GeneratePlayData(File file, Emitter emitter)
        {
            var particleBaseBytes = new List<byte>();
            //properties
            var variables = new List<VariableResource>();
            variables.AddRange(emitter.Locals);
            variables.AddRange(file.Globals);
            base.GeneratePropertyExpressions(variables, particleBaseBytes);
            //type
            particleBaseBytes.AddRange(BitConverter.GetBytes(type != null ? type.ID : -1));
            //particleBaseData
            PlayDataHelper.GenerateStruct(particleBaseData, particleBaseBytes);
            return PlayDataHelper.CreateBlock(particleBaseBytes);
        }
        public virtual void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader particleBaseReader = PlayDataHelper.GetBlockReader(reader))
            {
                //properties
                base.LoadPropertyExpressions(particleBaseReader);
                typeID = particleBaseReader.ReadInt32();
                //particleBaseData
                particleBaseData = PlayDataHelper.ReadStruct<ParticleBaseData>(particleBaseReader);
            }
        }
        protected virtual bool PushSystemProperty(int propertyID)
        {
            switch (propertyID)
            {
                case -1:
                    VM.PushInt(Emitter.Status);
                    return true;
                case -2:
                    VM.PushFloat(Emitter.StatusFrame, true, true);
                    return true;
                case -3:
                    VM.PushFloat(0);
                    return true;
                case -4:
                    VM.PushVector2(Emitter.BodyPosition);
                    return true;
                case -5:
                    VM.PushFloat(Emitter.BodyPosition.x);
                    return true;
                case -6:
                    VM.PushFloat(Emitter.BodyPosition.y);
                    return true;
                case -7:
                    VM.PushFloat(MathHelper.GetDegree(Emitter.BodyPosition - PPosition));
                    return true;
                case -8:
                    VM.PushVector2(Emitter.CenterPosition);
                    return true;
                case -9:
                    VM.PushFloat(Emitter.CenterPosition.x);
                    return true;
                case -10:
                    VM.PushFloat(Emitter.CenterPosition.y);
                    return true;
                case -11:
                    VM.PushFloat(MathHelper.GetDegree(Emitter.CenterPosition - PPosition));
                    return true;
            }
            return false;
        }
        public override bool PushProperty(int propertyID)
        {
            if (PushSystemProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 100:
                    VM.PushInt(MaxLife);
                    return true;
                case 101:
                    VM.PushFloat(PLayerFrame, true, true);
                    return true;
                case 102:
                    VM.PushFloat(PCurrentFrame, true, true);
                    return true;
                case 103:
                    VM.PushBool(PMasked);
                    PMasked = false;
                    return true;
                case 104:
                    VM.PushVector2(PPosition);
                    return true;
                case 105:
                    VM.PushFloat(PPosition.x);
                    return true;
                case 106:
                    VM.PushFloat(PPosition.y);
                    return true;
                case 107:
                    VM.PushRGB(RGB);
                    return true;
                case 108:
                    VM.PushFloat(RGB.r);
                    return true;
                case 109:
                    VM.PushFloat(RGB.g);
                    return true;
                case 110:
                    VM.PushFloat(RGB.b);
                    return true;
                case 111:
                    VM.PushFloat(Mass);
                    return true;
                case 112:
                    VM.PushFloat(Opacity);
                    return true;
                case 113:
                    VM.PushFloat(PSpeed);
                    return true;
                case 114:
                    VM.PushFloat(PSpeedAngle);
                    return true;
                case 115:
                    VM.PushFloat(PAcspeed);
                    return true;
                case 116:
                    VM.PushFloat(PAcspeedAngle);
                    return true;
                case 117:
                    VM.PushFloat(PSpeedHScale);
                    return true;
                case 118:
                    VM.PushFloat(PSpeedVScale);
                    return true;
                case 119:
                    VM.PushFloat(PRotation);
                    return true;
                case 120:
                    VM.PushInt((int)BlendType);
                    return true;
                case 121:
                    VM.PushBool(KillOutside);
                    return true;
                case 122:
                    VM.PushBool(Collision);
                    return true;
                case 123:
                    VM.PushBool(IgnoreMask);
                    return true;
                case 124:
                    VM.PushBool(IgnoreRebound);
                    return true;
                case 125:
                    VM.PushBool(IgnoreForce);
                    return true;
                case 126:
                    VM.PushFloat(WidthScale);
                    return true;
                case 127:
                    VM.PushBool(FogEffect);
                    return true;
                case 128:
                    VM.PushBool(FadeEffect);
                    return true;
                case 129:
                    VM.PushFloat(VSpeed);
                    return true;
            }
            for (int i = 0; i < Emitter.Locals.Count; ++i)
            {
                if (Emitter.Locals[i].ID == propertyID)
                {
                    VM.PushFloat(Emitter.Locals[i].Value);
                    return true;
                }
            }
            for (int i = 0; i < Emitter.Globals.Count; ++i)
            {
                if (Emitter.Globals[i].ID == propertyID)
                {
                    VM.PushFloat(Emitter.Globals[i].Value);
                    return true;
                }
            }
            return false;
        }
        public override bool SetProperty(int propertyID)
        {
            switch (propertyID)
            {
                case 100:
                    MaxLife = VM.PopInt();
                    return true;
                case 104:
                    PPosition = VM.PopVector2();
                    return true;
                case 105:
                    PPosition = new Vector2(VM.PopFloat(), PPosition.y);
                    return true;
                case 106:
                    PPosition = new Vector2(PPosition.x, VM.PopFloat());
                    return true;
                case 107:
                    RGB = VM.PopRGB();
                    return true;
                case 108:
                    RGB = new RGB(VM.PopFloat(), RGB.g, RGB.b);
                    return true;
                case 109:
                    RGB = new RGB(RGB.r, VM.PopFloat(), RGB.b);
                    return true;
                case 110:
                    RGB = new RGB(RGB.r, RGB.g, VM.PopFloat());
                    return true;
                case 111:
                    Mass = VM.PopFloat();
                    return true;
                case 112:
                    Opacity = VM.PopFloat();
                    return true;
                case 113:
                    PSpeed = VM.PopFloat();
                    MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle, 
                        new Vector2(PSpeedHScale, PSpeedVScale));
                    return true;
                case 114:
                    PSpeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle, 
                        new Vector2(PSpeedHScale, PSpeedVScale));
                    return true;
                case 115:
                    PAcspeed = VM.PopFloat();
                    MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle, 
                        new Vector2(PSpeedHScale, PSpeedVScale));
                    return true;
                case 116:
                    PAcspeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle, 
                        new Vector2(PSpeedHScale, PSpeedVScale));
                    return true;
                case 117:
                    PSpeedHScale = VM.PopFloat();
                    return true;
                case 118:
                    PSpeedVScale = VM.PopFloat();
                    return true;
                case 119:
                    PRotation = VM.PopFloat();
                    return true;
                case 120:
                    BlendType = (BlendType)VM.PopInt();
                    RenderOrder = (RenderOrder - RenderOrder % 10) + 9 - (int)BlendType;
                    return true;
                case 121:
                    KillOutside = VM.PopBool();
                    return true;
                case 122:
                    Collision = VM.PopBool();
                    return true;
                case 123:
                    IgnoreMask = VM.PopBool();
                    return true;
                case 124:
                    IgnoreRebound = VM.PopBool();
                    return true;
                case 125:
                    IgnoreForce = VM.PopBool();
                    return true;
                case 126:
                    WidthScale = VM.PopFloat();
                    return true;
                case 127:
                    FogEffect = VM.PopBool();
                    return true;
                case 128:
                    FadeEffect = VM.PopBool();
                    return true;
                case 129:
                    VSpeed = VM.PopFloat();
                    return true;
            }
            for (int i = 0; i < Emitter.Locals.Count; ++i)
            {
                if (Emitter.Locals[i].ID == propertyID)
                {
                    Emitter.Locals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            for (int i = 0; i < Emitter.Globals.Count; ++i)
            {
                if (Emitter.Globals[i].ID == propertyID)
                {
                    Emitter.Globals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            return false;
        }
        public virtual Vector2 GetOutPoint() => PPosition;
        public virtual bool CheckCollision(Vector2 playerLast, Vector2 player, float r) => false;
        public virtual bool CheckVolume(bool playerDead, Vector2 playerLast, Vector2 player, out Vector2 newPlayerPos)
        {
            newPlayerPos = player;
            return false;
        }
        public virtual bool Update(float frameScale, float currentFrame = 1)
        {
            if (PCurrentFrame > MaxLife || (KillOutside && ParticleManager.OutOfWindow(this)))
            {
                Alive = false;
                return false;
            }
            if (MathHelper.FrameEqual(PCurrentFrame, frameScale, 1))
            {
                MathHelper.SetVector2(ref pspeedVector, PSpeed, PSpeedAngle,
                    new Vector2(PSpeedHScale, PSpeedVScale));
                MathHelper.SetVector2(ref pacspeedVector, PAcspeed, PAcspeedAngle,
                    new Vector2(PSpeedHScale, PSpeedVScale));
            }
            //QuadTree.Update(this);
            PPositionLast = PPosition;
            pspeedVector += pacspeedVector * frameScale;
            PPosition += pspeedVector * frameScale;
            for (int i = 0; i < ParticleEventGroups.Count; ++i)
            {
                ParticleEventGroups[i].Execute(this, null, frameScale);
            }
            PCurrentFrame += frameScale;
            PAnimateFrame += frameScale;
            if (MaxLife <= FOG_TIME)
            {
                FogFrame = (int)FOG_TIME;
            }
            else if (PCurrentFrame <= MaxLife - FOG_TIME)
            {
                FogFrame += frameScale;
                if (!FogEffect || FogFrame >= FOG_TIME) FogFrame = (int)FOG_TIME;
            }
            else if (FadeEffect)
            {
                FogFrame -= frameScale;
                if (FogFrame <= 0) FogFrame = 0;
            }
            return true;
        }
        public virtual void Die()
        {
            if (!Alive) return;
            PCurrentFrame = Math.Max(MaxLife - (int)FOG_TIME + 1, 1);
        }
        public override void CopyTo(PropertyContainer target)
        {
            base.CopyTo(target);
            var particle = target as ParticleBase;
            if (particle == null) return;
            particle.type = type;
            particle.typeID = typeID;
            particle.particleBaseData = particleBaseData;
            particle.PCurrentFrame = 1;
            particle.PAnimateFrame = 1;
            particle.PMasked = false;
            particle.PPosition = PPosition;
            particle.PSpeedAngle = PSpeedAngle;
            particle.Emitter = Emitter;
            particle.FogFrame = 0;
            particle.ParticleEventGroups = ParticleEventGroups;
            particle.ReboundTime = 0;
        }
        public virtual void Reset() { }
        public int CompareTo(ParticleBase other)
        {
            return (int)(RenderOrder - other.RenderOrder);
        }
        #endregion
    }
}
