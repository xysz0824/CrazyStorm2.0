/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CrazyStorm.Core
{
    public class File : IXmlData, IGeneratePlayData, ILoadPlayData
    {
        #region Private Members
        IList<ParticleSystem> particleSystems;
        IList<FileResource> images;
        IList<FileResource> sounds;
        IList<VariableResource> globals;
        [XmlAttribute]
        int fileResourceIndex;
        [XmlAttribute]
        int particleIndex;
        #endregion

        #region Public Members
        public static string CurrentDirectory = string.Empty;
        public IList<ParticleSystem> ParticleSystems { get { return particleSystems; } }
        public IList<FileResource> Images { get { return images; } }
        public IList<FileResource> Sounds { get { return sounds; } }
        public IList<VariableResource> Globals { get { return globals; } }
        public int ParticleIndex { get { return particleIndex++; } }
        public int FileResourceIndex { get { return fileResourceIndex++; } }
        #endregion

        #region Constructor
        public File(bool newFile)
        {
            CurrentDirectory = string.Empty;
            particleSystems = new List<ParticleSystem>();
            if (newFile)
            {
                particleSystems.Add(new ParticleSystem("Untitled"));
            }
            images = new GenericContainer<FileResource>();
            sounds = new GenericContainer<FileResource>();
            globals = new GenericContainer<VariableResource>();
        }
        #endregion

        #region Public Methods
        public void UpdateResource()
        {
            foreach (var item in images)
                item.CheckValid();

            foreach (var item in sounds)
                item.CheckValid();

            foreach (var item in globals)
                item.CheckValid();
        }
        public object Clone()
        {
            throw new NotImplementedException();
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            XmlHelper.BuildFromFields(this, node);
            //particleSystems
            XmlHelper.BuildFromObjectList(particleSystems, new ParticleSystem(""), node, "ParticleSystems");
            //images
            XmlHelper.BuildFromObjectList(images, new FileResource(0, "", ""), node, "Images");
            //sounds
            XmlHelper.BuildFromObjectList(sounds, new FileResource(0, "", ""), node, "Sounds");
            //globals
            XmlHelper.BuildFromObjectList(globals, new VariableResource(""), node, "Globals");
            return node;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            XmlHelper.StoreFields(this, doc, node);
            //particleSystems
            XmlHelper.StoreObjectList(particleSystems, doc, node, "ParticleSystems");
            //images
            XmlHelper.StoreObjectList(images, doc, node, "Images");
            //sounds
            XmlHelper.StoreObjectList(sounds, doc, node, "Sounds");
            //globals
            XmlHelper.StoreObjectList(globals, doc, node, "Globals");            
            return node;
        }
        public List<byte> GeneratePlayData()
        {
            var fileBytes = new List<byte>();
            //particleSystems
            PlayDataHelper.GenerateObjectList(particleSystems, fileBytes);
            //images
            PlayDataHelper.GenerateObjectList(images, fileBytes);
            //sounds
            PlayDataHelper.GenerateObjectList(sounds, fileBytes);
            //globals
            PlayDataHelper.GenerateObjectList(globals, fileBytes);
            return fileBytes;
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            //ParticleSystems
            PlayDataHelper.LoadObjectList(ParticleSystems, reader, version);
            //Images
            PlayDataHelper.LoadObjectList(Images, reader, version);
            //Sounds
            PlayDataHelper.LoadObjectList(Sounds, reader, version);
            //Globals
            var globals = new List<VariableResource>();
            PlayDataHelper.LoadObjectList(globals, reader, version);
            foreach (var particleSystem in ParticleSystems)
            {
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                        component.Globals = globals;
                }
            }
        }
        #endregion
    }
}
