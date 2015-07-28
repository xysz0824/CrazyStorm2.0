using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IntPropertyAttribute : PropertyAttribute
    {
        int minValue, maxValue;
        public IntPropertyAttribute(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        public override bool IsLegal(string newValue, out object value)
        {
            value = null;
            int typeValue;
            bool result = int.TryParse(newValue, out typeValue);
            if (result && (typeValue >= minValue && typeValue <= maxValue))
            {
                value = typeValue;
                return true;
            }
            float tempValue;
            result = float.TryParse(newValue, out tempValue);
            if (result && ((int)tempValue >= minValue && (int)typeValue <= maxValue))
            {
                value = (int)tempValue;
                return true;
            }
            return false;
        }
    }
}
