/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ModifyLocalCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var editing = Parameter[0] as VariableResource;
            var newLabel = Parameter[1] as string;
            var newValue = Parameter[2] as string;
            var environment = Parameter[3] as Expression.Environment;
            History[0] = editing.Label;
            History[1] = editing.Value.ToString();
            environment.RemoveLocal(editing.Label);
            editing.Label = newLabel;
            editing.Value = float.Parse(newValue);
            environment.PutLocal(editing.Label, editing.Value);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var editing = Parameter[0] as VariableResource;
            var oldLabel = History[0] as string;
            var oldValue = History[1] as string;
            var environment = Parameter[3] as Expression.Environment;
            environment.RemoveLocal(editing.Label);
            editing.Label = oldLabel;
            editing.Value = float.Parse(oldValue);
            environment.PutLocal(editing.Label, editing.Value);
        }
    }
}
