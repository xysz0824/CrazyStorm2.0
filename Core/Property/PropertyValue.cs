/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public class PropertyValue : ICloneable
    {
        bool expression;
        string value;
        byte[] compiledExpression;
        public bool Expression
        {
            get { return expression; }
            set { expression = value; }
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
        public byte[] CompiledExpression
        {
            get { return compiledExpression; }
            set { compiledExpression = value; }
        }
        public object Clone()
        {
            var clone = MemberwiseClone() as PropertyValue;
            clone.compiledExpression = null;
            return clone;
        }
    }
}
