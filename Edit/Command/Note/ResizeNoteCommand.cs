/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System.Windows;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ResizeNoteCommand : Command
    {
        Rect oldRect;
        Rect newRect;
        public Rect NewRect
        {
            get { return newRect; }
            set { newRect = value; }
        }
        public ResizeNoteCommand(Rect oldRect, Rect newRect)
        {
            this.oldRect = oldRect;
            this.newRect = newRect;
        }
        public bool IsSameTarget(ResizeNoteCommand command)
        {
            if (command == null) return false;

            var target1 = Parameter[0] as Note;
            var target2 = command.Parameter[0] as Note;
            return target1 != null && target1 == target2;
        }
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var note = Parameter[0] as Note;
            if (note == null) return;

            note.X = (float)newRect.X;
            note.Y = (float)newRect.Y;
            note.Width = (float)newRect.Width;
            note.Height = (float)newRect.Height;
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var note = Parameter[0] as Note;
            if (note == null) return;

            note.X = (float)oldRect.X;
            note.Y = (float)oldRect.Y;
            note.Width = (float)oldRect.Width;
            note.Height = (float)oldRect.Height;
        }
    }
}
