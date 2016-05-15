using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class PropertyTypeRule
    {
        public static bool CanConvertToSingle(Type type)
        {
            if (type.Equals(typeof(int)) || type.Equals(typeof(float)) || type.Equals(typeof(bool)) || type.IsEnum)
                return true;
            
            return false;
        }
        public static bool IsMatchWith(Type typeA, Type typeB)
        {
            if (typeA.Equals(typeB))
                return true;

            if (CanConvertToSingle(typeA) && CanConvertToSingle(typeB))
                return true;

            return false;
        }
        public static bool TryMatchWith(object target, string text, out object output)
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
