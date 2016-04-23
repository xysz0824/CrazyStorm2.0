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
    class UnbindComponentCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedComponents = Parameter[0] as IList<Component>;
            var boundComponents = new List<Component>();
            foreach (var component in selectedComponents)
            {
                boundComponents.Add(component.BindingTarget);
                component.BindingTarget = null;
            }
            History[0] = boundComponents;
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedComponents = Parameter[0] as IList<Component>;
            var boundComponents = History[0] as IList<Component>;
            foreach (var component in selectedComponents)
            {
                component.BindingTarget = boundComponents.First();
                boundComponents.RemoveAt(0);
            }
        }
    }
}
