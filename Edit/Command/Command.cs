/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CrazyStorm
{
    public abstract class Command
    {
        protected object[] parameter;
        protected object[] history;

        public void Do(CommandStack stack, params object[] parameter)
        {
            this.parameter = parameter;
            history = new object[5];
            Redo(stack);
        }
        public virtual void Redo(CommandStack stack)
        {
            if (history == null)
                throw new ParameterEmptyException("Command history is empty.");
            stack.RedoPush(this);
        }
        public virtual void Undo(CommandStack stack)
        {
            if (history == null)
                throw new ParameterEmptyException("Command history is empty.");
            stack.UndoPush(this);
        }
    }
}
