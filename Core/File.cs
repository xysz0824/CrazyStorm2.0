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
        ObservableCollection<Resource> images;
        ObservableCollection<Resource> sounds;
        ObservableCollection<Resource> globals;
        Collection<ParticleSystem> particles;
        #endregion

        #region Public Members
        public string FileName { get { return name + ".bgp"; } }
        public string AbsolutePath { get { return absolutePath; } }
        public ObservableCollection<Resource> Images { get { return images; } }
        public ObservableCollection<Resource> Sounds { get { return sounds; } }
        public ObservableCollection<Resource> Globals { get { return globals; } }
        public Collection<ParticleSystem> Particles { get { return particles; } }
        #endregion

        #region Constructor
        public File(string name)
        {
            this.name = name;
            absolutePath = String.Empty;
            images = new ObservableCollection<Resource>();
            sounds = new ObservableCollection<Resource>();
            globals = new ObservableCollection<Resource>();
            particles = new Collection<ParticleSystem>();
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
