/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public abstract class PropertyAttribute : Attribute
    {
        public int ID { get; private set; }
        public PropertyAttribute(int id ) { ID = id; }
        public abstract bool IsLegal(string newValue, out object value);
    }
}
