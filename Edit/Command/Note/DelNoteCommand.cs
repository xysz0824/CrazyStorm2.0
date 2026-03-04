/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System.Collections.Generic;
using System.Linq;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class DelNoteCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var system = Parameter[0] as ParticleSystem;
            var selectedNotes = Parameter[1] as List<Note>;
            if (system == null || selectedNotes == null) return;

            List<KeyValuePair<int, Note>> records;
            if (History[0] == null)
            {
                records = new List<KeyValuePair<int, Note>>();
                foreach (var note in selectedNotes)
                {
                    var index = system.Notes.IndexOf(note);
                    if (index < 0) continue;
                    records.Add(new KeyValuePair<int, Note>(index, note));
                }
                records = records.OrderBy(item => item.Key).ToList();
                History[0] = records;
            }
            else
            {
                records = History[0] as List<KeyValuePair<int, Note>>;
            }

            if (records == null) return;
            foreach (var record in records)
            {
                record.Value.Selected = false;
                system.Notes.Remove(record.Value);
            }
            selectedNotes.Clear();
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var system = Parameter[0] as ParticleSystem;
            var selectedNotes = Parameter[1] as List<Note>;
            var records = History[0] as List<KeyValuePair<int, Note>>;
            if (system == null || selectedNotes == null || records == null) return;

            for (int i = 0; i < records.Count; ++i)
            {
                var index = records[i].Key;
                var note = records[i].Value;
                if (index < 0) index = 0;
                if (index > system.Notes.Count) index = system.Notes.Count;
                if (!system.Notes.Contains(note)) system.Notes.Insert(index, note);
            }
            selectedNotes.Clear();
        }
    }
}
