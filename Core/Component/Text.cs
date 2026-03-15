/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class TextPool : PoolObject<TextPool, NullData>
    {
        public Text Instance { get; private set; }

        public TextPool()
        {
            Instance = new Text();
            Instance.PoolObject = this;
        }
    }

    public class Text : Component
    {
        #region Private Members
        [StringData]
        [XmlAttribute]
        string font;
        [StringData]
        [XmlAttribute]
        string textValue;
        [XmlAttribute]
        int charsetPixelSize;
        int emitCursor;
        List<ParticleType> runtimeCharacterTypes;
        #endregion

        #region Public Members
        public TextPool PoolObject { get; set; }
        [FontProperty(25, 1, 128)]
        public string Font
        {
            get { return font; }
            set { font = value; }
        }
        [StringProperty(26, 0, 1024, true, true, true, true, true)]
        public string TextValue
        {
            get { return textValue; }
            set { textValue = value; }
        }
        [IntProperty(27, 1, 1024)]
        public int CharsetPixelSize
        {
            get { return charsetPixelSize; }
            set { charsetPixelSize = value; }
        }
        public bool HasRuntimeText => runtimeCharacterTypes != null && runtimeCharacterTypes.Count > 0;
        #endregion

        #region Constructor
        public Text()
        {
            font = string.Empty;
            textValue = string.Empty;
            charsetPixelSize = 48;
        }
        #endregion

        #region Public Methods
        public override Component Instantiate()
        {
            return TextPool.Rent(NullData.Empty).Instance;
        }

        public override void CopyTo(PropertyContainer propertyContainer)
        {
            base.CopyTo(propertyContainer);
            var text = propertyContainer as Text;
            text.font = font;
            text.textValue = textValue;
            text.charsetPixelSize = charsetPixelSize;
            text.runtimeCharacterTypes = null;
            text.emitCursor = 0;
        }

        public override XmlElement BuildFromXml(XmlElement node)
        {
            node = base.BuildFromXml(node);
            XmlHelper.BuildFromFields(this, node);
            return node;
        }

        public override XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            node = base.StoreAsXml(doc, node);
            XmlHelper.StoreFields(this, doc, node);
            return node;
        }

        public override List<byte> GeneratePlayData(File file)
        {
            var bytes = base.GeneratePlayData(file);
            var textBytes = new List<byte>();
            textBytes.AddRange(PlayDataHelper.GetStringBytes(font ?? string.Empty));
            textBytes.AddRange(PlayDataHelper.GetStringBytes(textValue ?? string.Empty));
            textBytes.AddRange(BitConverter.GetBytes(charsetPixelSize));
            bytes.AddRange(PlayDataHelper.CreateBlock(textBytes));
            return bytes;
        }

        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (var textReader = PlayDataHelper.GetBlockReader(reader))
            {
                font = PlayDataHelper.ReadString(textReader);
                textValue = PlayDataHelper.ReadString(textReader);
                charsetPixelSize = textReader.ReadInt32();
            }
        }

        public override void Destroy()
        {
            runtimeCharacterTypes = null;
            TextPool.Return(PoolObject);
        }

        public override void Reset()
        {
            base.Reset();
            emitCursor = 0;
        }

        public void ApplyRuntimeResources(List<ParticleType> characterTypes)
        {
            runtimeCharacterTypes = characterTypes;
            emitCursor = 0;
        }

        public ParticleType GetNextParticleType()
        {
            if (!HasRuntimeText) return null;
            var type = runtimeCharacterTypes[emitCursor];
            emitCursor = (emitCursor + 1) % runtimeCharacterTypes.Count;
            return type;
        }

        public void ClearRuntimeResources()
        {
            runtimeCharacterTypes = null;
            emitCursor = 0;
        }

        public override bool PushProperty(int propertyID)
        {
            if (base.PushProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 25:
                    VM.PushString(Font);
                    return true;
                case 26:
                    VM.PushString(TextValue);
                    return true;
                case 27:
                    VM.PushInt(CharsetPixelSize);
                    return true;
            }
            return false;
        }

        public override bool SetProperty(int propertyID)
        {
            return base.SetProperty(propertyID);
        }
        #endregion
    }
}
