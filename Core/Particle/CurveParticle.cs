/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct CurveParticleData
    {
        public int length;
        public int segment;
    }
    public class CurveParticle : ParticleBase
    {
        #region Private Members
        CurveParticleData curveParticleData;
        #endregion

        #region Public Members
        [IntProperty(0, int.MaxValue)]
        public int Length
        {
            get { return curveParticleData.length; }
            set { curveParticleData.length = value; }
        }
        [IntProperty(1, 256)]
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
            XmlHelper.BuildFromStruct(ref curveParticleData, curveParticleNode, "CurveParticleData");
            return curveParticleNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var curveParticleNode = doc.CreateElement("CurveParticle");
            //curveParticleData
            XmlHelper.StoreStruct(curveParticleData, doc, curveParticleNode, "CurveParticleData");
            node.AppendChild(curveParticleNode);
            return curveParticleNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
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
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(curveParticleReader))
                {
                    Length = dataReader.ReadInt32();
                    Segment = dataReader.ReadInt32();
                }
            }
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "Length":
                    VM.PushInt(Length);
                    return true;
                case "Segment":
                    VM.PushInt(Segment);
                    return true;
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "Length":
                    Length = VM.PopInt();
                    return true;
                case "Segment":
                    Segment = VM.PopInt();
                    return true;
            }
            return false;
        }
        public override Vector2 GetOutPoint() => Curve != null ? Curve.GetCurveEnd() : base.GetOutPoint();
        bool CurveJudge(Vector2 head, Vector2 tail, float scale, Vector2 bp, Vector2 p, Vector2 s, float r, float deg)
        {
            if (scale < 0.3f || scale > 0.7f) return false;
            return MathHelper.Judge(head, tail, bp, p, s, r, deg) & WidthScale >= 0.5f;
        }
        public override bool CheckCollision(float bx, float by, float x, float y, float r)
        {
            return FogFrame >= FOG_TIME && Curve != null &&
                Curve.IterateSegment(CurveJudge, new Vector2(bx, by), new Vector2(x, y), new Vector2(WidthScale, WidthScale), 2, PRotation, Length);
        }
        public override bool Update(int currentFrame = 0)
        {
            var initData = new CurveInitData { pos = PPosition, segment = Segment };
            if (Curve == null) Curve = Curve.Rent(initData);
            if (!base.Update()) return false;
            PRotation = PSpeedAngle + 90;
            Curve.Update(PPosition, Type.Width * WidthScale, Length);
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
