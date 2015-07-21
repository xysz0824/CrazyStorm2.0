using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class Vector3PropertyAttribute : PropertyAttribute
    {
        public override bool IsLegal(string newValue, out object value)
        {
            value = null;
            Vector3 typeValue;
            bool result = Vector3.TryParse(newValue, out typeValue);
            if (result)
            {
                value = typeValue;
                return true;
            }
            return false;
        }
    }
}
