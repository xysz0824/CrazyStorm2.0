/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetParticleTypeCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var emitter = Parameter[0] as Emitter;
            var newType = Parameter[1] as ParticleType;
            var newColor = Parameter[2] as ParticleColor?;
            var update = Parameter[3] as Action<Emitter, ParticleType, ParticleColor>;
            History[0] = emitter.Particle.Type;
            History[1] = emitter.Particle.Type.Color;
            update(emitter, newType, newColor.Value);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var emitter = Parameter[0] as Emitter;
            var oldType = History[0] as ParticleType;
            var oldColor = History[1] as ParticleColor?;
            var update = Parameter[3] as Action<Emitter, ParticleType, ParticleColor>;
            update(emitter, oldType, oldColor.Value);
        }
    }
}
