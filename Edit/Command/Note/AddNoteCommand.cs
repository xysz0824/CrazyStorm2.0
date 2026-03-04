/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;

namespace CrazyStorm
{
    class AddNoteCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var system = Parameter[0] as ParticleSystem;
            var note = Parameter[1] as Note;
            if (system == null || note == null) return;

            int index;
            if (History[0] == null)
            {
                index = system.Notes.Count;
                History[0] = index;
            }
            else
            {
                index = (int)History[0];
            }

            if (index < 0) index = 0;
            if (index > system.Notes.Count) index = system.Notes.Count;
            if (!system.Notes.Contains(note)) system.Notes.Insert(index, note);
            note.Selected = true;
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var system = Parameter[0] as ParticleSystem;
            var note = Parameter[1] as Note;
            if (system == null || note == null) return;

            note.Selected = false;
            system.Notes.Remove(note);
        }
    }
}
