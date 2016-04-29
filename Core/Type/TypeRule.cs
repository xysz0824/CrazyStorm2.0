using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class TypeRule
    {
        public static bool IsMatchWith(System.Type typeA, System.Type typeB)
        {
            if (typeA.Equals(typeB))
                return true;

            System.Type intType = typeof(int);
            System.Type floatType = typeof(float);
            if ((typeA.Equals(intType) && typeB.Equals(floatType)) ||
                (typeA.Equals(floatType) && typeB.Equals(intType)))
                return true;

            return false;
        }
        public static bool IsMatchWith(object target, string text, out object output)
        {
            //Check if the text is match with target type.
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
        public static bool IsMatchWith(object target, string text)
        {
            object value;
            return IsMatchWith(target, text, out value);
        }
    }
}
