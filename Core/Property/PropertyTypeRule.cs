/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CrazyStorm.Core
{
    public class PropertyTypeRule
    {
        public static PropertyType GetValueType(Type type, Type subType, string name)
        {
            var split = name.Split('.');
            var nameWithoutDot = split[0];
            var memberName = split.Length >= 2 ? split[1] : null;
            var properties = type.GetProperties();
            if (subType != null)
            {
                var subProperties = subType.GetProperties();
                var originalLength = properties.Length;
                Array.Resize(ref properties, originalLength + subProperties.Length);
                Array.Copy(subProperties, 0, properties, originalLength, subProperties.Length);
            }
            foreach (var property in properties)
            {
                if (property.Name != nameWithoutDot) continue;
                var attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute is StringPropertyAttribute) return PropertyType.String;
                    if (attribute is BoolPropertyAttribute) return PropertyType.Boolean;
                    if (attribute is IntPropertyAttribute) return PropertyType.Int32;
                    if (attribute is FloatPropertyAttribute) return PropertyType.Single;
                    if (attribute is Vector2PropertyAttribute) return memberName != null ? PropertyType.Single : PropertyType.Vector2;
                    if (attribute is RGBPropertyAttribute) return memberName != null ? PropertyType.Single : PropertyType.RGB;
                    if (attribute is EnumPropertyAttribute) return PropertyType.Enum;
                    if (attribute is RuntimePropertyAttribute)
                    {
                        var obj = Activator.CreateInstance(property.PropertyType);
                        return GetValueType(obj);
                    }
                }
            }
            return PropertyType.IllegalType;
        }
        public static PropertyType GetValueType(object value)
        {
            if (value is bool)
                return PropertyType.Boolean;
            else if (value is int)
                return PropertyType.Int32;
            else if (value is float)
                return PropertyType.Single;
            else if (value is Enum)
                return PropertyType.Enum;
            else if (value is Vector2)
                return PropertyType.Vector2;
            else if (value is RGB)
                return PropertyType.RGB;
            else if (value is string)
                return PropertyType.String;
            else
                return PropertyType.IllegalType;
        }
        public static bool IsMatchWith(Type typeA, Type typeB)
        {
            if (typeA.Equals(typeB))
                return true;

            Type intType = typeof(int);
            Type floatType = typeof(float);
            if ((typeA.Equals(intType) && typeB.Equals(floatType)) ||
                (typeA.Equals(floatType) && typeB.Equals(intType)))
                return true;

            return false;
        }
        public static object Parse(PropertyType type, string name, string text)
        {
            switch (type)
            {
                case PropertyType.Boolean:
                    return bool.Parse(text);
                case PropertyType.Int32:
                    return int.Parse(text);
                case PropertyType.Single:
                    return float.Parse(text);
                case PropertyType.Enum:
                    var namespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
                    return (int)Enum.Parse(Type.GetType(namespaceName + "." + name), text);
                case PropertyType.Vector2:
                    Vector2 vector2Value;
                    Vector2.TryParse(text, out vector2Value);
                    return vector2Value;
                case PropertyType.RGB:
                    RGB rgbValue;
                    RGB.TryParse(text, out rgbValue);
                    return rgbValue;
                case PropertyType.String:
                    return text;
            }
            return null;
        }
        public static bool TryParse(object target, string text, out object output)
        {
            bool result = false;
            if (target is bool)
            {
                bool value;
                result = bool.TryParse(text, out value);
                output = value;
            }
            else if (target is int)
            {
                int value;
                result = int.TryParse(text, out value);
                output = value;
            }
            else if (target is float)
            {
                float value;
                result = float.TryParse(text, out value);
                output = value;
            }
            else if (target is Enum)
            {
                result = Enum.IsDefined(target.GetType(), text);
                output = result ? Enum.Parse(target.GetType(), text) : null;
            }
            else if (target is Vector2)
            {
                Vector2 value;
                result = Vector2.TryParse(text, out value);
                output = value;
            }
            else if (target is RGB)
            {
                RGB value;
                result = RGB.TryParse(text, out value);
                output = value;
            }
            else if (target is string)
            {
                output = text;
                result = true;
            }
            else
                output = null;

            return result;
        }
    }
}
