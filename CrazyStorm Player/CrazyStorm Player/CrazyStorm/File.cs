using System;
using System.Collections.Generic;
using System.Linq;
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
        public void LoadPlayData(BinaryReader reader)
        {
            //ParticleSystems
            PlayDataHelper.LoadObjectList(ParticleSystems, reader);
            //Images
            PlayDataHelper.LoadObjectList(Images, reader);
            //Sounds
            PlayDataHelper.LoadObjectList(Sounds, reader);
            //Globals
            var globals = new List<VariableResource>();
            PlayDataHelper.LoadObjectList(globals, reader);
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
