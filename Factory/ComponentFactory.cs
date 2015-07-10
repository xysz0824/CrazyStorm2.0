/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ComponentFactory
    {
        private ComponentFactory()
        { }
        public static Component Create(string name)
        {
            return Assembly.Load("Core").CreateInstance("CrazyStorm.Core." + name, false) as Component;
        }
    }
}
