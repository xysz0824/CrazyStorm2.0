/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetEmitterMaskTypeCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var emitter = Parameter[0] as Emitter;
            var newMaskType = Parameter[1] as MaskType;
            var update = Parameter[2] as Action<Emitter, MaskType>;
            History[0] = emitter != null && emitter.InitialTemplate != null ? emitter.InitialTemplate.MaskType : null;
            update(emitter, newMaskType);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var emitter = Parameter[0] as Emitter;
            var oldMaskType = History[0] as MaskType;
            var update = Parameter[2] as Action<Emitter, MaskType>;
            update(emitter, oldMaskType);
        }
    }
}
