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
    abstract class Command
    {
        public virtual void Do(params object[] parameter)
        {
            CommandStack.DoPush(this);
        }
        public virtual void Undo(params object[] parameter)
        {
            CommandStack.UndoPush(this);
        }
    }
}
