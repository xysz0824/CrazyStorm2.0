/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ParticleData
    {
        public bool stickToSpeedAngle;
        public float heightScale;
        public bool retainScale;
        public bool afterimageEffect;
    }
    public struct AfterImage
    {
        public float alpha;
        public float x, y;
        public float rot;
    }
    public class Particle : ParticleBase
    {
        public const int AFTERIMAGE_COUNT = 25;
        #region Private Members
        ParticleData particleData;
        int afterimageTime;
        AfterImage[] afterimageData = new AfterImage[AFTERIMAGE_COUNT];
        #endregion

        #region Public Members
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float HeightScale
        {
            get { return particleData.heightScale; }
            set { particleData.heightScale = value; }
        }
        [BoolProperty]
        public bool RetainScale
        {
            get { return particleData.retainScale; }
            set { particleData.retainScale = value; }
        }
        [BoolProperty]
        public bool StickToSpeedAngle
        {
            get { return particleData.stickToSpeedAngle; }
            set { particleData.stickToSpeedAngle = value; }
        }
        [BoolProperty]
        public bool AfterimageEffect
        {
            get { return particleData.afterimageEffect; }
            set { particleData.afterimageEffect = value; }
        }
        public AfterImage[] AfterImageData => afterimageData;
        public ParticlePool PoolObject { get; set; }
        #endregion

        #region Constructor
        public Particle()
        {
            particleData.heightScale = 1;
            particleData.retainScale = true;
            particleData.stickToSpeedAngle = true;
        }
        #endregion

        #region Public Methods
        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            var particleNode = (XmlElement)node.SelectSingleNode("Particle");
            //particleData
            XmlHelper.BuildFromStruct(ref particleData, particleNode);
            return particleNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var particleNode = doc.CreateElement("Particle");
            //particleData
            XmlHelper.StoreStruct(particleData, doc, particleNode);
            node.AppendChild(particleNode);
            return particleNode;
        }
        public override List<byte> GeneratePlayData()
        {
            var bytes = base.GeneratePlayData();
            var particleBytes = new List<byte>();
            //particleData
            PlayDataHelper.GenerateStruct(particleData, particleBytes);
            bytes.AddRange(PlayDataHelper.CreateBlock(particleBytes));
            return bytes;
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader particleReader = PlayDataHelper.GetBlockReader(reader))
            {
                //particleData
                particleData = PlayDataHelper.ReadStruct<ParticleData>(particleReader);
            }
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "StickToSpeedAngle":
                    VM.PushBool(StickToSpeedAngle);
                    return true;
                case "HeightScale":
                    VM.PushFloat(HeightScale);
                    return true;
                case "RetainScale":
                    VM.PushBool(RetainScale);
                    return true;
                case "AfterimageEffect":
                    VM.PushBool(AfterimageEffect);
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
                case "StickToSpeedAngle":
                    StickToSpeedAngle = VM.PopBool();
                    return true;
                case "HeightScale":
                    HeightScale = VM.PopFloat();
                    return true;
                case "RetainScale":
                    RetainScale = VM.PopBool();
                    return true;
                case "AfterimageEffect":
                    AfterimageEffect = VM.PopBool();
                    if (!AfterimageEffect) afterimageTime = 0;
                    return true;
            }
            return false;
        }
        public override bool CheckCollision(Vector2 playerLast, Vector2 player, float r)
        {
            return FogFrame >= FOG_TIME &&
                MathHelper.Judge(PPositionLast, PPosition, playerLast, player,
                new Vector2(Math.Abs(WidthScale), Math.Abs(HeightScale)), r, PRotation);
        }
        public override bool CheckVolume(bool playerDead, Vector2 playerLast, Vector2 player, out Vector2 newPlayerPos)
        {
            newPlayerPos = player;
            return FogFrame >= FOG_TIME && 
                MathHelper.VolumeJudge(playerDead, playerLast, player, PPositionLast, PPosition, PRotation, Type.CenterPoint, 
                new Vector2(WidthScale, HeightScale), Type.VolumeStart, new Vector2(Type.VolumeWidth, Type.VolumeHeight), 
                Type.VolumeJudgeArea, out newPlayerPos);
        }
        public override bool Update(float frameScale, float currentFrame = 1)
        {
            if (!base.Update(frameScale, currentFrame)) return false;
            if (StickToSpeedAngle) PRotation = PSpeedAngle + 90;
            if (RetainScale && WidthScale != HeightScale) HeightScale = WidthScale;
            if (AfterimageEffect)
            {
                for (int i = 0; i < AFTERIMAGE_COUNT; ++i)
                {
                    afterimageData[i].alpha = Math.Max(0, afterimageData[i].alpha - 1.0f / AFTERIMAGE_COUNT);
                }
                afterimageData[afterimageTime].alpha = 0.4f;
                afterimageData[afterimageTime].x = PPosition.x;
                afterimageData[afterimageTime].y = PPosition.y;
                afterimageData[afterimageTime].rot = PRotation;
                afterimageTime = (afterimageTime + 1) % AFTERIMAGE_COUNT;
            }
            return true;
        }
        public override void CopyTo(PropertyContainer target)
        {
            base.CopyTo(target);
            var particle = target as Particle;
            if (particle == null) return;
            particle.particleData = particleData;
            particle.afterimageTime = 0;
            for (int i = 0; i < AFTERIMAGE_COUNT; ++i)
            {
                particle.AfterImageData[i] = default;
            }
        }
        public override void Reset()
        {
            base.Reset();
            afterimageData = new AfterImage[AFTERIMAGE_COUNT];
        }
        #endregion
    }
}
