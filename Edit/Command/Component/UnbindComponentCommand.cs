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
            var boundEmitters = new List<Component>();
            foreach (var component in selectedComponents)
            {
                boundEmitters.Add(component.BindingTarget);
                component.BindingTarget = null;
            }
            History[0] = boundEmitters;
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedComponents = Parameter[0] as IList<Component>;
            var boundEmitters = History[0] as IList<Component>;
            foreach (var component in selectedComponents)
            {
                component.BindingTarget = boundEmitters.First();
                boundEmitters.RemoveAt(0);
            }
        }
    }
}
