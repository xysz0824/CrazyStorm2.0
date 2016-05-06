using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class File : IPlayData
    {
        public IList<FileResource> Images { get; private set; }
        public IList<FileResource> Sounds { get; private set; }
        public IList<VariableResource> Globals { get; private set; }
        public IList<ParticleSystem> ParticleSystems { get; private set; }
        public File()
        {
            Images = new List<FileResource>();
            Sounds = new List<FileResource>();
            Globals = new List<VariableResource>();
            ParticleSystems = new List<ParticleSystem>();
        }
        public void LoadPlayData(BinaryReader reader)
        {
            //Images
            PlayDataHelper.LoadObjectList(Images, reader);
            //Sounds
            PlayDataHelper.LoadObjectList(Sounds, reader);
            //Globals
            PlayDataHelper.LoadObjectList(Globals, reader);
            //ParticleSystems
            PlayDataHelper.LoadObjectList(ParticleSystems, reader);
        }
    }
}
