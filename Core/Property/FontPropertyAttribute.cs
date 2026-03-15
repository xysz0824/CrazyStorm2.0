/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class FontPropertyAttribute : PropertyAttribute
    {
        readonly int minLength;
        readonly int maxLength;

        public static Func<string, bool> FontValidator { get; set; }

        public FontPropertyAttribute(int id, int minLength, int maxLength) : base(id)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        public override bool IsLegal(string newValue, out object value)
        {
            value = null;
            if (string.IsNullOrEmpty(newValue)) return false;
            if (newValue.Length < minLength || newValue.Length > maxLength) return false;
            if (FontValidator != null && !FontValidator(newValue)) return false;
            value = newValue;
            return true;
        }
    }
}
