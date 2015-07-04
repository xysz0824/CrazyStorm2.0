/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm
{
    enum MoveStatus
    {
        Up,
        Down,
        Left,
        Right
    }
    class MoveComponentCommand : Command
    {
        Vector2 move;

        public Vector2 Move 
        { 
            get { return move; }
            set { move = value; }
        }
        public MoveComponentCommand(MoveStatus status, bool gridAlignment)
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
                move *= 32f;
        }
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            List<Component> selectedComponents;
            if (History[0] == null)
            {
                //Be careful that the reference of list could be modified outside, 
                //so there should copy it.
                selectedComponents = new List<Component>();
                var temp = Parameter[0] as List<Component>;
                foreach (var item in temp)
                    selectedComponents.Add(item);
                History[0] = selectedComponents;
            }
            else
                selectedComponents = History[0] as List<Component>;

            foreach (var item in selectedComponents)
            {
                item.X += move.x;
                item.Y += move.y;
            }
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedComponents = History[0] as List<Component>;
            foreach (var item in selectedComponents)
            {
                item.X -= move.x;
                item.Y -= move.y;
            }
        }
    }
}
