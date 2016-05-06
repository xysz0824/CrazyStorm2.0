using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class ParticleType : IPlayData
    {
        public FileResource Image { get; set; }
        public int ImageID { get; private set; }
        public int Id { get; set; }
        public Vector2 StartPoint { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 CenterPoint { get; set; }
        public int Frames { get; set; }
        public int Delay { get; set; }
        public int Radius { get; set; }
        public ParticleType()
        {
            ImageID = -1;
        }
        public void LoadPlayData(BinaryReader reader)
        {
            using (BinaryReader particleTypeReader = PlayDataHelper.GetBlockReader(reader))
            {
                ImageID = particleTypeReader.ReadInt32();
                Id = particleTypeReader.ReadInt32();
                StartPoint = PlayDataHelper.ReadVector2(particleTypeReader);
                Width = particleTypeReader.ReadInt32();
                Height = particleTypeReader.ReadInt32();
                CenterPoint = PlayDataHelper.ReadVector2(particleTypeReader);
                Frames = particleTypeReader.ReadInt32();
                Delay = particleTypeReader.ReadInt32();
                Radius = particleTypeReader.ReadInt32();
            }
        }
    }
}
