using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class ParticleType : IPlayData, IRebuildReference<FileResource>
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
        public void RebuildReferenceFromCollection(IList<FileResource> collection)
        {
            if (ImageID != -1)
            {
                foreach (var target in collection)
                {
                    if (ImageID == target.Id)
                    {
                        Image = target;
                        break;
                    }
                }
            }
        }
        public static void LoadDefaultTypes(StreamReader reader, IList<ParticleType> typeset)
        {
            typeset.Clear();
            int i = 0;
            while (!reader.EndOfStream)
            {
                string[] splits = reader.ReadLine().Split('_');
                var particleType = new ParticleType();
                particleType.Id = i + 1000;
                particleType.StartPoint = new Vector2(float.Parse(splits[1]), float.Parse(splits[2]));
                particleType.Width = int.Parse(splits[3]);
                particleType.Height = int.Parse(splits[4]);
                particleType.CenterPoint = new Vector2(float.Parse(splits[5]), float.Parse(splits[6]));
                particleType.Radius = int.Parse(splits[7]);
                typeset.Add(particleType);
                i++;
            }
        }
    }
}
