/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class RuntimePropertyAttribute : PropertyAttribute
    {
        public override bool IsLegal(string newValue, out object value)
        {
            throw new NotImplementedException();
        }
    }
}
