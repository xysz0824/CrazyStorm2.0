/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetEmitterDistortTypeCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var emitter = Parameter[0] as Emitter;
            var newDistortType = Parameter[1] as DistortType;
            var update = Parameter[2] as Action<Emitter, DistortType>;
            History[0] = emitter != null && emitter.InitialTemplate != null ? emitter.InitialTemplate.DistortType : null;
            update(emitter, newDistortType);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var emitter = Parameter[0] as Emitter;
            var oldDistortType = History[0] as DistortType;
            var update = Parameter[2] as Action<Emitter, DistortType>;
            update(emitter, oldDistortType);
        }
    }
}
