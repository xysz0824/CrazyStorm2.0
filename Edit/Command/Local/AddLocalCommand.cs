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
    class AddLocalCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var name = Parameter[0] as string;
            var component = Parameter[1] as Component;
            var environment = Parameter[2] as Expression.Environment;
            var button = Parameter[3] as Button;
            var local = History[0] == null ? new VariableResource(int.MinValue, name) : History[0] as VariableResource;
            component.Locals.Add(local);
            for (int i = 0; i < component.Locals.Count; ++i)
            {
                component.Locals[i].ID = 1000 + i;
            }
            if (button != null) button.IsEnabled = true;
            environment.PutLocal(local.Label, local.Value);
            History[0] = local;
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var local = History[0] as VariableResource;
            var component = Parameter[1] as Component;
            var environment = Parameter[2] as Expression.Environment;
            var button = Parameter[3] as Button;
            component.Locals.Remove(local);
            for (int i = 0; i < component.Locals.Count; ++i)
            {
                component.Locals[i].ID = 1000 + i;
            }
            if (button != null) button.IsEnabled = component.Locals.Count > 0;
            environment.RemoveLocal(local.Label);
        }
    }
}
