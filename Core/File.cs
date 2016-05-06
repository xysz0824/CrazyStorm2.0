/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CrazyStorm.Core
{
    public class File : IXmlData, IPlayData
    {
        #region Private Members
        IList<FileResource> images;
        IList<FileResource> sounds;
        IList<VariableResource> globals;
        IList<ParticleSystem> particleSystems;
        [XmlAttribute]
        int fileResourceIndex;
        [XmlAttribute]
        int particleIndex;
        #endregion

        #region Public Members
        public static string CurrentDirectory = string.Empty;
        public IList<FileResource> Images { get { return images; } }
        public IList<FileResource> Sounds { get { return sounds; } }
        public IList<VariableResource> Globals { get { return globals; } }
        public IList<ParticleSystem> ParticleSystems { get { return particleSystems; } }
        public int ParticleIndex { get { return particleIndex++; } }
        public int FileResourceIndex { get { return fileResourceIndex++; } }
        #endregion

        #region Constructor
        public File()
        {
            images = new ObservableCollection<FileResource>();
            sounds = new ObservableCollection<FileResource>();
            globals = new ObservableCollection<VariableResource>();
            particleSystems = new List<ParticleSystem>();
            particleSystems.Add(new ParticleSystem("Untitled"));
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
            //images
            XmlHelper.BuildFromObjectList(images, new FileResource(0, "", ""), node, "Images");
            //sounds
            XmlHelper.BuildFromObjectList(sounds, new FileResource(0, "", ""), node, "Sounds");
            //globals
            XmlHelper.BuildFromObjectList(globals, new VariableResource(""), node, "Globals");
            //particleSystems
            XmlHelper.BuildFromObjectList(particleSystems, new ParticleSystem(""), node, "ParticleSystems");
            return node;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            XmlHelper.StoreFields(this, doc, node);
            //images
            XmlHelper.StoreObjectList(images, doc, node, "Images");
            //sounds
            XmlHelper.StoreObjectList(sounds, doc, node, "Sounds");
            //globals
            XmlHelper.StoreObjectList(globals, doc, node, "Globals");            
            //particleSystems
            XmlHelper.StoreObjectList(particleSystems, doc, node, "ParticleSystems");
            return node;
        }
        public List<byte> GeneratePlayData()
        {
            var fileBytes = new List<byte>();
            //images
            PlayDataHelper.GenerateObjectList(images, fileBytes);
            //sounds
            PlayDataHelper.GenerateObjectList(sounds, fileBytes);
            //globals
            PlayDataHelper.GenerateObjectList(globals, fileBytes);
            //particleSystems
            PlayDataHelper.GenerateObjectList(particleSystems, fileBytes);
            return fileBytes;
        }
        #endregion
    }
}
