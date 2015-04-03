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
        public virtual void Do(CommandStack stack, params object[] parameter)
        {
            stack.DoPush(this);
        }
        public virtual void Undo(CommandStack stack, params object[] parameter)
        {
            stack.UndoPush(this);
        }
    }
}
