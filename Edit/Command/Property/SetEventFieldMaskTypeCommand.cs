/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetEventFieldMaskTypeCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var eventField = Parameter[0] as EventField;
            var newMaskType = Parameter[1] as MaskType;
            var update = Parameter[2] as Action<EventField, MaskType>;
            History[0] = eventField != null ? eventField.MaskType : null;
            update(eventField, newMaskType);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var eventField = Parameter[0] as EventField;
            var oldMaskType = History[0] as MaskType;
            var update = Parameter[2] as Action<EventField, MaskType>;
            update(eventField, oldMaskType);
        }
    }
}
