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
    public sealed class Vector2PropertyAttribute : PropertyAttribute
    {
        public Vector2PropertyAttribute(int id) : base(id) { }
        public override bool IsLegal(string newValue, out object value)
        {
            value = null;
            Vector2 typeValue;
            bool result = Vector2.TryParse(newValue, out typeValue);
            if (result)
            {
                value = typeValue;
                return true;
            }
            return false;
        }
    }
}
