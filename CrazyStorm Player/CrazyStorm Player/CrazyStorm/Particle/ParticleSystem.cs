using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class ParticleSystem : IPlayData, IPlayable
    {
        public string Name { get; set; }
        public int CurrentFrame { get; set; }
        public IList<ParticleType> CustomTypes { get; private set; }
        public IList<Layer> Layers { get; private set; }
        public ParticleSystem()
        {
            CustomTypes = new List<ParticleType>();
            Layers = new List<Layer>();
        }
        public void LoadPlayData(BinaryReader reader)
        {
            using (BinaryReader particleSystemReader = PlayDataHelper.GetBlockReader(reader))
            {
                Name = PlayDataHelper.ReadString(particleSystemReader);
                //customTypes
                PlayDataHelper.LoadObjectList(CustomTypes, particleSystemReader);
                //layers
                PlayDataHelper.LoadObjectList(Layers, particleSystemReader);
            }
        }
        public bool Update(int currentFrame = 0)
        {
            for (int i = 0; i < Layers.Count; ++i)
                Layers[i].Update(CurrentFrame);

            ++CurrentFrame;
            return true;
        }
        public void Reset()
        {
            CurrentFrame = 0;
            for (int i = 0; i < Layers.Count; ++i)
                Layers[i].Reset();
        }
    }
}
