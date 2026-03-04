/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetNoteCommentCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var note = Parameter[0] as Note;
            if (note == null) return;

            if (History[0] == null) History[0] = note.Comment;
            note.Comment = (string)Parameter[1];
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var note = Parameter[0] as Note;
            if (note == null) return;

            note.Comment = (string)History[0];
        }
    }
}
