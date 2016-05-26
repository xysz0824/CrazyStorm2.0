using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class Particle : ParticleBase
    {
        public bool StickToSpeedAngle { get; set; }
        public float HeightScale { get; set; }
        public bool RetainScale { get; set; }
        public bool AfterimageEffect { get; set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader particleReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(particleReader))
                {
                    StickToSpeedAngle = dataReader.ReadBoolean();
                    HeightScale = dataReader.ReadSingle();
                    RetainScale = dataReader.ReadBoolean();
                    AfterimageEffect = dataReader.ReadBoolean();
                }
            }
        }
        public override void Copy(ParticleBase particleBase)
        {
            base.Copy(particleBase);
            var particle = particleBase as Particle;
            particle.StickToSpeedAngle = StickToSpeedAngle;
            particle.HeightScale = HeightScale;
            particle.RetainScale = RetainScale;
            particle.AfterimageEffect = AfterimageEffect;
        }
    }
}
