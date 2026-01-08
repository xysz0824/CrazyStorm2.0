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
            var update = Parameter[2] as Action<Emitter, ParticleType>;
            History[0] = emitter.Particle.Type;
            update(emitter, newType);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var emitter = Parameter[0] as Emitter;
            var oldType = History[0] as ParticleType;
            var update = Parameter[2] as Action<Emitter, ParticleType>;
            update(emitter, oldType);
        }
    }
}
