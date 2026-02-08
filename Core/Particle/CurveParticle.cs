/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CurveParticleData
    {
        public CurveType type;
        public int length;
        public int segment;
    }
    public class CurveParticle : ParticleBase
    {
        #region Private Members
        CurveParticleData curveParticleData;
        #endregion

        #region Public Members
        [EnumProperty(130,typeof(CurveType))]
        public CurveType CurveType
        {
            get { return  curveParticleData.type; }
            set { curveParticleData.type = value; }
        }
        [IntProperty(131, 0, int.MaxValue)]
        public int Length
        {
            get { return curveParticleData.length; }
            set { curveParticleData.length = value; }
        }
        [IntProperty(132, 16, 256)]
        public int Segment
        {
            get { return curveParticleData.segment; }
            set { curveParticleData.segment = value; }
        }
        public Curve Curve { get; private set; }
        public CurveParticlePool PoolObject { get; set; }
        #endregion

        #region Constructor
        public CurveParticle()
        {
            curveParticleData.type = CurveType.Curve;
            curveParticleData.length = 100;
            curveParticleData.segment = 64;
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var curveParticleNode = (XmlElement)node.SelectSingleNode("CurveParticle");
            //curveParticleData
            XmlHelper.BuildFromStruct(ref curveParticleData, curveParticleNode);
            return curveParticleNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var curveParticleNode = doc.CreateElement("CurveParticle");
            //curveParticleData
            XmlHelper.StoreStruct(curveParticleData, doc, curveParticleNode);
            node.AppendChild(curveParticleNode);
            return curveParticleNode;
        }
        public override List<byte> GeneratePlayData(File file, Emitter e)
        {
            var bytes = base.GeneratePlayData(file, e);
            var curveParticleBytes = new List<byte>();
            //curveParticleData
            PlayDataHelper.GenerateStruct(curveParticleData, curveParticleBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(curveParticleBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader curveParticleReader = PlayDataHelper.GetBlockReader(reader))
            {
                //curveParticleData
                curveParticleData = PlayDataHelper.ReadStruct<CurveParticleData>(curveParticleReader);
            }
        }
        public override bool PushProperty(int propertyID)
        {
            if (base.PushProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 130:
                    VM.PushInt((int)CurveType);
                    return true;
                case 131:
                    VM.PushInt(Length);
                    return true;
                case 132:
                    VM.PushInt(Segment);
                    return true;
            }
            return false;
        }
        public override bool SetProperty(int propertyID)
        {
            if (base.SetProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 130:
                    var newCurveType = (CurveType)VM.PopInt();
                    if (CurveType != newCurveType)
                    {
                        if (Curve != null) Curve.Return(Curve);
                        var head = PSpeedVector;
                        if (head.Length() == 0) MathHelper.SetVector2(ref head, 1, PSpeedAngle, 
                            new Vector2(PSpeedHScale, PSpeedVScale));
                        var initData = new CurveInitData
                        {
                            pos = PPosition,
                            segment = Segment,
                            type = newCurveType,
                            head = head,
                            length = Length
                        };
                        Curve = Curve.Rent(initData);
                    }
                    CurveType = newCurveType;
                    return true;
                case 131:
                    Length = VM.PopInt();
                    return true;
                case 132:
                    Segment = VM.PopInt();
                    return true;
            }
            return false;
        }
        public override Vector2 GetOutPoint() => Curve != null ? Curve.GetCurveEnd() : base.GetOutPoint();
        bool CurveJudge(Vector2 head, Vector2 tail, float scale, Vector2 bp, Vector2 p, Vector2 s, float r, float deg)
        {
            if (scale < 0.3f || scale > 0.7f) return false;
            return MathHelper.Judge(head, tail, bp, p, s, r, deg) & Math.Abs(WidthScale) >= 0.5f;
        }
        public override bool CheckCollision(Vector2 playerLast, Vector2 player, float r)
        {
            return FogFrame >= FOG_TIME && Curve != null &&
                Curve.IterateSegment(CurveJudge, playerLast, player, new Vector2(WidthScale, WidthScale), 2, PRotation + 90, Length);
        }
        public override bool Update(float frameScale, float currentFrame = 1)
        {
            if (!base.Update(frameScale, currentFrame)) return false;
            var head = PSpeedVector;
            if (head.Length() == 0) MathHelper.SetVector2(ref head, 1, PSpeedAngle, 
                new Vector2(PSpeedHScale, PSpeedVScale));
            if (Curve == null)
            {
                var initData = new CurveInitData { pos = PPosition, 
                    segment = Segment, 
                    type = CurveType, 
                    head = head,
                    length = Length };
                Curve = Curve.Rent(initData);
            }
            PRotation = PSpeedAngle;
            Curve.Update(PPosition, head, Type.Width * WidthScale, Length);
            return true;
        }
        public override void CopyTo(PropertyContainer target)
        {
            base.CopyTo(target);
            var particle = target as CurveParticle;
            if (particle == null) return;
            particle.curveParticleData = curveParticleData;
            particle.Curve = null;
        }
        #endregion
    }
}
