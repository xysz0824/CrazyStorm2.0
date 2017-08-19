/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CrazyStorm.Core
{
    public static class ComponentFactory
    {
        public static Component Create(string name)
        {
            var namespacePrefix = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            return Assembly.GetExecutingAssembly().CreateInstance(namespacePrefix + "." + name, false) as Component;
        }
    }
}
