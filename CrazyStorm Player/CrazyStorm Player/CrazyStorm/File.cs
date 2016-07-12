/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class File : IPlayData
    {
        public IList<ParticleSystem> ParticleSystems { get; private set; }
        public IList<FileResource> Images { get; private set; }
        public IList<FileResource> Sounds { get; private set; }
        public File()
        {
            ParticleSystems = new List<ParticleSystem>();
            Images = new List<FileResource>();
            Sounds = new List<FileResource>();
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
    }
}
