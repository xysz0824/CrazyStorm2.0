/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CrazyStorm.Core
{
    public class File
    {
        #region Private Members
        string name;
        string absolutePath;
        IList<Resource> images;
        IList<Resource> sounds;
        IList<Resource> globals;
        IList<ParticleSystem> particles;
        int particleIndex;
        #endregion

        #region Public Members
        public string FileName { get { return name + ".bgp"; } }
        public string AbsolutePath { get { return absolutePath; } }
        public IList<Resource> Images { get { return images; } }
        public IList<Resource> Sounds { get { return sounds; } }
        public IList<Resource> Globals { get { return globals; } }
        public IList<ParticleSystem> Particles { get { return particles; } }
        public int ParticleIndex { get { return particleIndex++; } }
        #endregion

        #region Constructor
        public File(string name)
        {
            this.name = name;
            absolutePath = String.Empty;
            images = new ObservableCollection<Resource>();
            sounds = new ObservableCollection<Resource>();
            globals = new ObservableCollection<Resource>();
            particles = new List<ParticleSystem>();
            particles.Add(new ParticleSystem("Untitled"));
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
        #endregion
    }
}
