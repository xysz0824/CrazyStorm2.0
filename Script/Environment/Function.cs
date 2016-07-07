/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    public class Function
    {
        int argumentCount;
        Type[] argumentTypes;
        public Function(params Type[] argumentTypes)
        {
            this.argumentCount = argumentTypes.Length;
            this.argumentTypes = argumentTypes;
        }

        public int ArgumentCount { get { return argumentCount; } }
        public Type[] ArgumentTypes { get { return argumentTypes; } }
    }
}
