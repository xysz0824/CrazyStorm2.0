using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    abstract class Emitter : Component
    {
        public Vector2 EmitPosition { get; set; }
        public int EmitCount { get; set; }
        public int EmitCycle { get; set; }
        public float EmitAngle { get; set; }
        public float EmitRange { get; set; }
        public ParticleBase Particle { get; protected set; }
        public IList<EventGroup> emitterEventGroups { get; private set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader emitterReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(emitterReader))
                {
                    EmitPosition = PlayDataHelper.ReadVector2(dataReader);
                    EmitCount = dataReader.ReadInt32();
                    EmitCycle = dataReader.ReadInt32();
                    EmitAngle = dataReader.ReadSingle();
                    EmitRange = dataReader.ReadSingle();
                }
                //particle
                Particle.LoadPlayData(emitterReader);
                //emitterEventGroups
                PlayDataHelper.LoadObjectList(emitterEventGroups, emitterReader);
            }
        }
    }
}
