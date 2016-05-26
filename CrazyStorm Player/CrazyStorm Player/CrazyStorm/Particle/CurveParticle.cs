using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class CurveParticle : ParticleBase
    {
        public int Length { get; set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader curveParticleReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(curveParticleReader))
                {
                    Length = dataReader.ReadInt32();
                }
            }
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Copy(ParticleBase particleBase)
        {
            base.Copy(particleBase);
            var curveParticle = particleBase as CurveParticle;
            curveParticle.Length = Length;
        }
    }
}
