/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ComponentFactory
    {
        private ComponentFactory()
        { }
        public static Component Create(string name)
        {
            switch (name)
            {
                case "MultiEmitter":
                    return new MultiEmitter();
                case "CurveEmitter":
                    return new CurveEmitter();
                case "Mask":
                    return new Mask();
                case "Rebound":
                    return new Rebound();
                case "Force":
                    return new Force();
                default:
                    return null;
            }
        }
    }
}
