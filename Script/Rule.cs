using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    public class Rule
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
        public static bool IsMatchWith(object target, string text)
        {
            //Check if the text is match with target type.
            if (target is bool)
            {
                bool value;
                return bool.TryParse(text, out value);
            }
            else if (target is int)
            {
                int value;
                return int.TryParse(text, out value);
            }
            else if (target is float)
            {
                float value;
                return float.TryParse(text, out value);
            }
            else if (target is Enum)
            {
                return Enum.IsDefined(target.GetType(), text);
            }
            else if (target is Core.Vector2)
            {
                Core.Vector2 value;
                return Core.Vector2.TryParse(text, out value);
            }
            else if (target is Core.RGB)
            {
                Core.RGB value;
                return Core.RGB.TryParse(text, out value);
            }
            else if (target is string)
            {
                return true;
            }
            return false;
        }
    }
}
