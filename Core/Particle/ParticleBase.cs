/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.IO;
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
    public struct ParticleBaseData
    {
        public int maxLife;
        public int pcurrentFrame;
        public Vector2 pposition;
        public float widthScale;
        public RGB rgb;
        public float mass;
        public float opacity;
        public float pspeed;
        public float pspeedAngle;
        public float pacspeed;
        public float pacspeedAngle;
        public float protation;
        public BlendType blendType;
        public bool killOutside;
        public bool collision;
        public bool ignoreMask;
        public bool ignoreRebound;
        public bool ignoreForce;
        public bool fogEffect;
        public bool fadeEffect;
    }
    public abstract class ParticleBase : PropertyContainer, IXmlData, IRebuildReference<ParticleType>, IGeneratePlayData, ILoadPlayData, 
        IComparable<ParticleBase>
    {
        #region Private Members
        Vector2 pspeedVector;
        Vector2 pacspeedVector;
        ParticleType type;
        int typeID = -1;
        ParticleBaseData particleBaseData;
        #endregion

        #region Public Members
        public int ID { get; set; }
        public int RenderOrder;
        public bool Alive;
        public int FogFrame;
        public Emitter Emitter { get; set; }
        //public ParticleQuadTree QuadTree { get; set; }
        [IntProperty(0, int.MaxValue)]
        public int MaxLife
        {
            get { return particleBaseData.maxLife; }
            set { particleBaseData.maxLife = value; }
        }
        [RuntimeProperty]
        public int PCurrentFrame
        {
            get { return particleBaseData.pcurrentFrame; }
            set { particleBaseData.pcurrentFrame = value; }
        }
        [RuntimeProperty]
        public Vector2 PPosition
        {
            get { return particleBaseData.pposition; }
            set { particleBaseData.pposition = value; }
        }
        public ParticleType Type
        {
            get { return type; }
            set { type = value; }
        }
        [RGBProperty]
        public RGB RGB
        {
            get { return particleBaseData.rgb; }
            set { particleBaseData.rgb = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float Mass
        {
            get { return particleBaseData.mass; }
            set { particleBaseData.mass = value; }
        }
        [FloatProperty(0, float.MaxValue)]
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
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PSpeed
        {
            get { return particleBaseData.pspeed; }
            set { particleBaseData.pspeed = value; }
        }
        [RuntimeProperty]
        public float PSpeedAngle
        {
            get { return particleBaseData.pspeedAngle; }
            set { particleBaseData.pspeedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PAcspeed
        {
            get { return particleBaseData.pacspeed; }
            set { particleBaseData.pacspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PAcspeedAngle
        {
            get { return particleBaseData.pacspeedAngle; }
            set { particleBaseData.pacspeedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float PRotation
        {
            get { return particleBaseData.protation; }
            set { particleBaseData.protation = value; }
        }
        [EnumProperty(typeof(BlendType))]
        public BlendType BlendType
        {
            get { return particleBaseData.blendType; }
            set { particleBaseData.blendType = value; }
        }
        [BoolProperty]
        public bool KillOutside
        {
            get { return particleBaseData.killOutside; }
            set { particleBaseData.killOutside = value; }
        }
        [BoolProperty]
        public bool Collision
        {
            get { return particleBaseData.collision; }
            set { particleBaseData.collision = value; }
        }
        [BoolProperty]
        public bool IgnoreMask
        {
            get { return particleBaseData.ignoreMask; }
            set { particleBaseData.ignoreMask = value; }
        }
        [BoolProperty]
        public bool IgnoreRebound
        {
            get { return particleBaseData.ignoreRebound; }
            set { particleBaseData.ignoreRebound = value; }
        }
        [BoolProperty]
        public bool IgnoreForce
        {
            get { return particleBaseData.ignoreForce; }
            set { particleBaseData.ignoreForce = value; }
        }
        [FloatProperty(0, float.MaxValue)]
        public float WidthScale
        {
            get { return particleBaseData.widthScale; }
            set { particleBaseData.widthScale = value; }
        }
        [BoolProperty]
        public bool FogEffect
        {
            get { return particleBaseData.fogEffect; }
            set { particleBaseData.fogEffect = value; }
        }
        [BoolProperty]
        public bool FadeEffect
        {
            get { return particleBaseData.fadeEffect; }
            set { particleBaseData.fadeEffect = value; }
        }
        public IList<EventGroup> ParticleEventGroups { get; set; }
        #endregion

        #region Constructor
        public ParticleBase()
        {
            RenderOrder = int.MaxValue;
            particleBaseData.maxLife = 200;
            particleBaseData.widthScale = 1;
            particleBaseData.rgb = new RGB(255, 255, 255);
            particleBaseData.mass = 1;
            particleBaseData.opacity = 100;
            particleBaseData.pspeed = 5;
            particleBaseData.killOutside = true;
            particleBaseData.collision = true;
            particleBaseData.fogEffect = true;
            particleBaseData.fadeEffect = true;
            ParticleEventGroups = new List<EventGroup>();
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
            XmlHelper.BuildFromStruct(ref particleBaseData, particleBaseNode, "ParticleBaseData");
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
            XmlHelper.StoreStruct(particleBaseData, doc, particleBaseNode, "ParticleBaseData");
            node.AppendChild(particleBaseNode);
            return particleBaseNode;
        }
        public void RebuildReferenceFromCollection(IList<ParticleType> collection)
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
        public virtual List<byte> GeneratePlayData()
        {
            var particleBaseBytes = new List<byte>();
            //properties
            base.GeneratePlayData(particleBaseBytes);
            //type
            if (type != null)
                particleBaseBytes.AddRange(PlayDataHelper.GetBytes(type.ID));
            else
                particleBaseBytes.AddRange(PlayDataHelper.GetBytes(-1));

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
            }
            for (int i = 0; i < Emitter.Locals.Count; ++i)
            {
                if (Emitter.Locals[i].Label == propertyName)
                {
                    VM.PushFloat(Emitter.Locals[i].Value);
                    return true;
                }
            }
            for (int i = 0; i < Emitter.Globals.Count; ++i)
            {
                if (Emitter.Globals[i].Label == propertyName)
                {
                    VM.PushFloat(Emitter.Globals[i].Value);
                    return true;
                }
            }
            return false;
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
            }
            for (int i = 0; i < Emitter.Locals.Count; ++i)
            {
                if (Emitter.Locals[i].Label == propertyName)
                {
                    Emitter.Locals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            for (int i = 0; i < Emitter.Globals.Count; ++i)
            {
                if (Emitter.Globals[i].Label == propertyName)
                {
                    Emitter.Globals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            return false;
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
            //QuadTree.Update(this);
            PSpeedVector += PAcspeedVector;
            PPosition += PSpeedVector;
            for (int i = 0; i < ParticleEventGroups.Count; ++i)
                ParticleEventGroups[i].Execute(this, null);

            ++PCurrentFrame;
            if (PCurrentFrame < MaxLife - 10)
            {
                ++FogFrame;
                if (!FogEffect || FogFrame >= 10)
                    FogFrame = 10;
            }
            else if (FadeEffect)
            {
                --FogFrame;
                if (FogFrame <= 0)
                    FogFrame = 0;
            }
        }
        public ParticleBase Copy()
        {
            return MemberwiseClone() as ParticleBase;
        }
        public int CompareTo(ParticleBase other)
        {
            return RenderOrder - other.RenderOrder;
        }
        #endregion
    }
}
