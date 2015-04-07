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
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            List<Component> selectedComponents;
            if (history[0] == null)
            {
                //Be careful that the reference of list could be modified outside, 
                //so there should copy it.
                selectedComponents = new List<Component>();
                var temp = parameter[0] as List<Component>;
                foreach (var item in temp)
                    selectedComponents.Add(item);
                history[0] = selectedComponents;
            }
            else
                selectedComponents = history[0] as List<Component>;

            var gridAlignment = (bool)parameter[2];
            Vector2 delta = Vector2.Zero;
            switch ((MoveStatus)parameter[1])
            {
                case MoveStatus.Up:
                    delta = new Vector2(0, -1);
                    break;
                case MoveStatus.Down:
                    delta = new Vector2(0, 1);
                    break;
                case MoveStatus.Left:
                    delta = new Vector2(-1, 0);
                    break;
                case MoveStatus.Right:
                    delta = new Vector2(1, 0);
                    break;
            }
            if (gridAlignment)
                delta *= 32f;
            history[1] = delta;
            foreach (var item in selectedComponents)
            {
                item.X += delta.x;
                item.Y += delta.y;
            }
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedComponents = history[0] as List<Component>;
            var delta = (Vector2)history[1];
            foreach (var item in selectedComponents)
            {
                item.X -= delta.x;
                item.Y -= delta.y;
            }
        }
    }
}
