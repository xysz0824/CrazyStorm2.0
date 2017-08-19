/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class StringPropertyAttribute : PropertyAttribute
    {
        int minLength, maxLength;
        bool supportNumber, supportAlpha, supportPunctuation, supportSymbol;
        public StringPropertyAttribute(int minLength, int maxLength, 
            bool supportNumber, bool supportAlpha, bool supportPunctuation, bool supportSymbol)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
            this.supportNumber = supportNumber;
            this.supportAlpha = supportAlpha;
            this.supportPunctuation = supportPunctuation;
            this.supportSymbol = supportSymbol;
        }
        public override bool IsLegal(string newValue, out object value)
        {
            value = null;
            foreach (char item in newValue)
            {
                if (char.IsWhiteSpace(item) || char.IsControl(item))
                    return false;
                if (!supportNumber && char.IsControl(item))
                    return false;
                if (!supportAlpha && char.IsLetter(item))
                    return false;
                if (!supportPunctuation && char.IsPunctuation(item))
                    return false;
                if (!supportSymbol && char.IsSymbol(item))
                    return false;
            }
            if (newValue.Length >= minLength && newValue.Length <= maxLength)
            {
                value = newValue;
                return true;
            }
            return false;
        }
    }
}
