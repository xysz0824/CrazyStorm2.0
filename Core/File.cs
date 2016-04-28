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

namespace CrazyStorm.Core
{
    public class File : IXmlData
    {
        #region Private Members
        IList<Resource> images;
        IList<Resource> sounds;
        IList<Resource> globals;
        IList<ParticleSystem> particleSystems;
        [XmlAttribute]
        int fileResourceIndex;
        [XmlAttribute]
        int particleIndex;
        #endregion

        #region Public Members
        public IList<Resource> Images { get { return images; } }
        public IList<Resource> Sounds { get { return sounds; } }
        public IList<Resource> Globals { get { return globals; } }
        public IList<ParticleSystem> ParticleSystems { get { return particleSystems; } }
        public int FileResourceIndex { get { return fileResourceIndex++; } }
        public int ParticleIndex { get { return particleIndex++; } }
        #endregion

        #region Constructor
        public File()
        {
            images = new ObservableCollection<Resource>();
            sounds = new ObservableCollection<Resource>();
            globals = new ObservableCollection<Resource>();
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
        public XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
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
        #endregion
    }
}
