/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CrazyStorm.Core
{
    public struct ParticleData
    {
        public bool stickToSpeedAngle;
        public float heightScale;
        public bool retainScale;
        public bool afterimageEffect;
    }
    public class Particle : ParticleBase
    {
        #region Private Members
        ParticleData particleData;
        #endregion

        #region Public Members
        [FloatProperty(0, float.MaxValue)]
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
            XmlHelper.BuildFromStruct(ref particleData, particleNode, "ParticleData");
            return particleNode;
        }
        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            var particleNode = doc.CreateElement("Particle");
            //particleData
            XmlHelper.StoreStruct(particleData, doc, particleNode, "ParticleData");
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
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(particleReader))
                {
                    StickToSpeedAngle = dataReader.ReadBoolean();
                    HeightScale = dataReader.ReadSingle();
                    RetainScale = dataReader.ReadBoolean();
                    AfterimageEffect = dataReader.ReadBoolean();
                }
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
                    return true;
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            if (StickToSpeedAngle)
                PRotation = PSpeedAngle + 90;

            if (RetainScale && WidthScale != HeightScale)
                HeightScale = WidthScale;

            //TODO Particle Effect
        }
        #endregion
    }
}
