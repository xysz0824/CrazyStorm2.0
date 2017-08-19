/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CrazyStorm.Core
{
    public class PropertyTypeRule
    {
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
            else if (value is Core.Vector2)
                return PropertyType.Vector2;
            else if (value is Core.RGB)
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
                    Vector2 value;
                    Vector2.TryParse(text, out value);
                    return value;
                case PropertyType.RGB:
                    Vector2.TryParse(text, out value);
                    return value;
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
            else if (target is Core.Vector2)
            {
                Core.Vector2 value;
                result = Core.Vector2.TryParse(text, out value);
                output = value;
            }
            else if (target is Core.RGB)
            {
                Core.RGB value;
                result = Core.RGB.TryParse(text, out value);
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
