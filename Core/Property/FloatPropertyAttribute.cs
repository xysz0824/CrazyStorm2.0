/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class FloatPropertyAttribute : PropertyAttribute
    {
        float minValue, maxValue;
        public FloatPropertyAttribute(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        public override bool IsLegal(string newValue, out object value)
        {
            value = null;
            float typeValue;
            bool result = float.TryParse(newValue, out typeValue);
            if (result && (typeValue >= minValue && typeValue <= maxValue))
            {
                value = typeValue;
                return true;
            }
            return false;
        }
    }
}
