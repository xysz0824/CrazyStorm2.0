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
    class DelEventGroupCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var eventGroup = Parameter[0] as EventGroup;
            var groups = Parameter[1] as IList<EventGroup>;
            var delButton = Parameter[2] as Button;
            groups.Remove(eventGroup);
            delButton.IsEnabled = groups.Count > 0;
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var eventGroup = Parameter[0] as EventGroup;
            var groups = Parameter[1] as IList<EventGroup>;
            var delButton = Parameter[2] as Button;
            groups.Add(eventGroup);
            delButton.IsEnabled = true;
        }
    }
}
