/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public enum ParticleColor
    {
        None,
        Red,
        Purple,
        Blue,
        Cyan,
        Green,
        Yellow,
        Orange,
        Gray
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ParticleTypeData
    {
        public int id;
        public Vector2 startPoint;
        public int width;
        public int height;
        public Vector2 centerPoint;
        public int frames;
        public int delay;
        public int radius;
        public ParticleColor color;
        public Vector2 volumeStart;
        public int volumeWidth;
        public int volumeHeight;
        public int volumeJudgeArea;
    }
    public class ParticleType : INotifyPropertyChanged, IXmlData, IRebuildReference<FileResource>, IGeneratePlayData, ILoadPlayData
    {
        public static readonly List<ParticleType> DefaultTypes = new List<ParticleType>();
        public const int DefaultTypeIndex = 1000;
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        [XmlAttribute]
        string name;
        FileResource image;
        int imageID = -1;
        ParticleTypeData data;
        #endregion

        #region Public Members
        public int ID
        {
            get { return data.id; }
            set { data.id = value; }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (!StringUtil.IsNullOrWhiteSpace(value)) name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        public FileResource Image
        {
            get { return image; }
            set
            {
                image = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Image"));
            }
        }
        public Vector2 StartPoint
        {
            get { return data.startPoint; }
            set
            {
                data.startPoint = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPoint"));
            }
        }
        public float StartPointX
        {
            get { return data.startPoint.x; }
            set
            {
                data.startPoint.x = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPointX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxX"));
                }
            }
        }
        public float StartPointY
        {
            get { return data.startPoint.y; }
            set
            {
                data.startPoint.y = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StartPointY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxY"));
                }
            }
        }
        public int Width
        {
            get { return data.width; }
            set
            {
                data.width = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Width"));
            }
        }
        public int Height
        {
            get { return data.height; }
            set
            {
                data.height = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Height"));
            }
        }
        public Vector2 CenterPoint
        {
            get { return data.centerPoint; }
            set
            {
                data.centerPoint = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterPoint"));
            }
        }
        public float CenterPointX
        {
            get { return data.centerPoint.x; }
            set
            {
                data.centerPoint.x = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterPointX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxX"));
                }
            }
        }
        public float CircleBoxX
        {
            get { return StartPointX + CenterPointX - Radius; }
        }
        public float CenterPointY
        {
            get { return data.centerPoint.y; }
            set
            {
                data.centerPoint.y = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterPointY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxY"));
                }
            }
        }
        public float CircleBoxY
        {
            get { return StartPointY + CenterPointY - Radius; }
        }
        public int Frames
        {
            get { return data.frames; }
            set
            {
                data.frames = value >= 1 ? value : 1;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Frames"));
            }
        }
        public int Delay
        {
            get { return data.delay; }
            set
            {
                data.delay = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Delay"));
            }
        }
        public int Radius
        {
            get { return data.radius; }
            set
            {
                data.radius = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Radius"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Diameter"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("CircleBoxY"));
                }
            }
        }
        public int Diameter { get { return data.radius * 2; }}
        public ParticleColor Color
        {
            get { return data.color; }
            set
            {
                data.color = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }
        public Vector2 VolumeStart
        {
            get { return data.volumeStart; }
            set
            {
                data.volumeStart = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeStart"));
            }
        }
        public float VolumeStartX
        {
            get { return data.volumeStart.x; }
            set
            {
                data.volumeStart.x = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeStartX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeX"));
                }
            }
        }
        public float VolumeJudgeX
        {
            get { return data.volumeStart.x + data.volumeWidth / 2 * (1f - data.volumeJudgeArea / 100f); }
        }
        public float VolumeStartY
        {
            get { return data.volumeStart.y; }
            set
            {
                data.volumeStart.y = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeStartY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeY"));
                }
            }
        }
        public float VolumeJudgeY
        {
            get { return data.volumeStart.y + data.volumeHeight / 2 * (1f - data.volumeJudgeArea / 100f); }
        }
        public int VolumeWidth
        {
            get { return data.volumeWidth; }
            set
            {
                data.volumeWidth = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeWidth"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeWidth"));
                }
            }
        }
        public int VolumeJudgeWidth
        {
            get { return (int)(data.volumeWidth * (data.volumeJudgeArea / 100f)); }
        }
        public int VolumeHeight
        {
            get { return data.volumeHeight; }
            set
            {
                data.volumeHeight = value >= 0 ? value : 0;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeHeight"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeHeight"));
                }
            }
        }
        public int VolumeJudgeHeight
        {
            get { return (int)(data.volumeHeight * (data.volumeJudgeArea / 100f)); }
        }
        public int VolumeJudgeArea
        {
            get { return data.volumeJudgeArea; }
            set
            {
                data.volumeJudgeArea = Math.Min(Math.Max(value, 0), 100);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeArea"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeX"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeY"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeWidth"));
                    PropertyChanged(this, new PropertyChangedEventArgs("VolumeJudgeHeight"));
                }
            }
        }
        #endregion

        #region Constructor
        public ParticleType()
        {
            data.frames = 1;
        }
        public ParticleType(int id)
        {
            data.id = id;
            name = string.Empty;
            data.frames = 1;
        }
        public ParticleType(int id, string name) : this(id)
        {
            this.name = $"{name}{id + 1}";
        }
        #endregion

        #region Public Methods
        public static void LoadDefaultTypes()
        {
            if (DefaultTypes.Count > 0) return;
            var assembly = Assembly.GetExecutingAssembly();
            Stream defaultParticleTypesStream = assembly.GetManifestResourceStream("CrazyStorm.Core.set.txt");
            using (StreamReader reader = new StreamReader(defaultParticleTypesStream, Encoding.UTF8))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    string[] splits = reader.ReadLine().Split('_');
                    var particleType = new ParticleType(i + DefaultTypeIndex);
                    particleType.Name = splits[0];
                    particleType.StartPoint = new Vector2(float.Parse(splits[1]), float.Parse(splits[2]));
                    particleType.Width = int.Parse(splits[3]);
                    particleType.Height = int.Parse(splits[4]);
                    particleType.CenterPoint = new Vector2(float.Parse(splits[5]), float.Parse(splits[6]));
                    particleType.Radius = int.Parse(splits[7]);
                    if (!StringUtil.IsNullOrWhiteSpace(splits[8]))
                    {
                        particleType.Color = (ParticleColor)(int.Parse(splits[8]) + 1);
                    }
                    DefaultTypes.Add(particleType);
                    i++;
                }
            }
        }
        public override string ToString()
        {
            return Name;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "ParticleType";
            var particleTypeNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName) particleTypeNode = node;
            if (particleTypeNode.HasAttribute("image"))
            {
                string fileResourceAttribute = particleTypeNode.GetAttribute("image");
                int parsedID;
                if (int.TryParse(fileResourceAttribute, out parsedID)) imageID = parsedID;
                else throw new System.IO.FileLoadException("FileDataError");
            }
            XmlHelper.BuildFromFields(this, particleTypeNode);
            //particleTypeData
            XmlHelper.BuildFromStruct(ref data, particleTypeNode);
            return particleTypeNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var particleTypeNode = doc.CreateElement("ParticleType");
            if (image != null)
            {
                var fileResourceAttribute = doc.CreateAttribute("image");
                fileResourceAttribute.Value = image.ID.ToString();
                particleTypeNode.Attributes.Append(fileResourceAttribute);
            }
            XmlHelper.StoreFields(this, doc, particleTypeNode);
            //particleTypeData
            XmlHelper.StoreStruct(data, doc, particleTypeNode);
            node.AppendChild(particleTypeNode);
            return particleTypeNode;
        }
        public void RebuildReferenceFromCollection(IList<FileResource> collection)
        {
            //image
            if (imageID != -1)
            {
                foreach (var target in collection)
                {
                    if (imageID == target.ID)
                    {
                        image = target;
                        break;
                    }
                }
                imageID = -1;
            }
        }
        public List<byte> GeneratePlayData()
        {
            var particleTypeBytes = new List<byte>();
            particleTypeBytes.AddRange(BitConverter.GetBytes(image != null ? image.ID : -1));
            PlayDataHelper.GenerateStringDataFields(this, particleTypeBytes);
            //particleTypeData
            PlayDataHelper.GenerateStruct(data, particleTypeBytes);
            return PlayDataHelper.CreateBlock(particleTypeBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader particleTypeReader = PlayDataHelper.GetBlockReader(reader))
            {
                imageID = particleTypeReader.ReadInt32();
                PlayDataHelper.ReadStringDataFields(this, particleTypeReader);
                //particleTypeData
                data = PlayDataHelper.ReadStruct<ParticleTypeData>(particleTypeReader);
            }
        }
        #endregion
    }
}
