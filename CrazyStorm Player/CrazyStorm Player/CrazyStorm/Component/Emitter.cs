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
        public ParticleBase Template { get; protected set; }
        public IList<ParticleBase> Particles { get; private set; }
        public IList<EventGroup> EmitterEventGroups { get; private set; }
        public Emitter()
        {
            Particles = new List<ParticleBase>();
        }
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
                Template.LoadPlayData(emitterReader);
                //emitterEventGroups
                PlayDataHelper.LoadObjectList(EmitterEventGroups, emitterReader);
                Template.ParticleEventGroups = EmitterEventGroups;
            }
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            if (currentFrame % EmitCycle == 0)
            {
                base.ExecuteExpression("EmitCycle");
                base.ExecuteExpression("EmitRange");
                base.ExecuteExpression("EmitCount");
                base.ExecuteExpression("EmitAngle");
                base.ExecuteExpression("EmitPosition");
                Template.PPosition = EmitPosition;
                float increment = EmitRange / EmitCount;
                float angle = EmitAngle;
                for (int i = 0;i < EmitCount;++i)
                {
                    angle += increment;
                    Template.PSpeedAngle = angle;
                    ParticleBase newParticle = ParticleManager.GetParticle(Template);
                    newParticle.Emitter = this;
                    Particles.Add(newParticle);
                }
            }
            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as Emitter;
            EmitPosition = initialState.EmitPosition;
            EmitCount = initialState.EmitCount;
            EmitCycle = initialState.EmitCycle;
            EmitAngle = initialState.EmitAngle;
            EmitRange = initialState.EmitRange;
        }
    }
}
