/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetNoteColorCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var note = Parameter[0] as Note;
            if (note == null) return;

            if (History[0] == null) History[0] = note.Color;
            note.Color = (LayerColor)Parameter[1];
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var note = Parameter[0] as Note;
            if (note == null) return;

            note.Color = (LayerColor)History[0];
        }
    }
}
