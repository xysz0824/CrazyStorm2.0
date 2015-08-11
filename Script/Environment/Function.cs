/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    public class Function
    {
        int argumentCount;
        public Function(int argumentCount)
        {
            this.argumentCount = argumentCount;
        }

        public int ArgumentCount { get { return argumentCount; } }
    }
}
