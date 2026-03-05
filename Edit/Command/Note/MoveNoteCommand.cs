/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System.Collections.Generic;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class MoveNoteCommand : Command
    {
        Vector2 move;
        public Vector2 Move
        {
            get { return move; }
            set { move = value; }
        }
        public MoveNoteCommand(Vector2 move)
        {
            this.move = move;
        }
        public MoveNoteCommand(MoveStatus status, Vector2 gridSize, bool gridAlignment)
        {
            move = Vector2.Zero;
            switch (status)
            {
                case MoveStatus.Up:
                    move = new Vector2(0, -1);
                    break;
                case MoveStatus.Down:
                    move = new Vector2(0, 1);
                    break;
                case MoveStatus.Left:
                    move = new Vector2(-1, 0);
                    break;
                case MoveStatus.Right:
                    move = new Vector2(1, 0);
                    break;
            }
            if (gridAlignment)
            {
                move.x *= gridSize.x / 2;
                move.y *= gridSize.y / 2;
            }
        }
        public bool IsSameTarget(MoveNoteCommand command)
        {
            if (command == null) 
                return false;

            var target1 = History[0] as List<Note>;
            var target2 = command.History[0] as List<Note>;
            if (target2 == null || target1.Count != target2.Count) 
                return false;

            foreach (var item in target1)
                if (!target2.Contains(item))
                    return false;

            return true;
        }
        public override void Redo(CommandStack stack)
        {
            List<Note> notes;
            if (History[0] == null)
            {
                notes = new List<Note>();
                var source = Parameter[0] as List<Note>;
                if (source == null) return;
                foreach (var note in source) notes.Add(note);
                History[0] = notes;
            }
            else
            {
                notes = History[0] as List<Note>;
            }

            if (notes == null) return;
            foreach (var note in notes)
            {
                note.X += move.x;
                note.Y += move.y;
            }
            base.Redo(stack);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var notes = History[0] as List<Note>;
            if (notes == null) return;

            foreach (var note in notes)
            {
                note.X -= move.x;
                note.Y -= move.y;
            }
        }
    }
}
