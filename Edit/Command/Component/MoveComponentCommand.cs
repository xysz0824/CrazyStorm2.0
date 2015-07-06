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
        public bool IsSameTarget(MoveComponentCommand command)
        {
            if (command == null)
                return false;

            var target1 = History[0] as List<Component>;
            var target2 = command.History[0] as List<Component>;
            if (target2 == null || target1.Count != target2.Count)
                return false;

            foreach (var item in target1)
                if (!target2.Contains(item))
                    return false;
            
            return true;
        }
        public override void Redo(CommandStack stack)
        {
            List<Component> selectedComponents;
            if (History[0] == null)
            {
                //Be careful that the reference of list could be modified outside, 
                //so they should be copyed.
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
            //Because it needs first to initialize History[0],
            //the base method is put in there instead of the start,
            //otherwise the IsSamgeTarget() can't get right result.
            base.Redo(stack);
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
