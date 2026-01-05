/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class DelLocalCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var local = Parameter[0] as VariableResource;
            var component = Parameter[1] as Component;
            var environment = Parameter[2] as Expression.Environment;
            var button = Parameter[3] as Button;
            component.Locals.Remove(local);
            if (button != null) button.IsEnabled = component.Locals.Count > 0;
            environment.RemoveLocal(local.Label);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var local = Parameter[0] as VariableResource;
            var component = Parameter[1] as Component;
            var environment = Parameter[2] as Expression.Environment;
            var button = Parameter[3] as Button;
            component.Locals.Add(local);
            if (button != null) button.IsEnabled = true;
            environment.PutLocal(local.Label, local.Value);
        }
    }
}
