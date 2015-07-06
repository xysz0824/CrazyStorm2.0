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
        private List<object> parameter;
        protected IList<object> Parameter { get { return parameter; } }
        private List<object> history;
        protected IList<object> History { get { return history; } }

        public void Do(CommandStack stack, params object[] parameter)
        {
            this.parameter = new List<object>(parameter);
            history = new List<object>(5);
            for (int i = 0; i < 5; ++i)
                history.Add(null);

            Redo(stack);
            stack.UndoClear();
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
