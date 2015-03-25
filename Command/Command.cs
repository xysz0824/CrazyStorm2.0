/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm
{
    abstract class Command
    {
        object target;

        public Command(object target)
        {
            this.target = target;
        }
        public abstract void Do();
        public abstract void Undo();
    }
}
