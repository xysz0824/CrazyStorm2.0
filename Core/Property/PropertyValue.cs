using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class PropertyValue
    {
        bool expression;
        string value;
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
    }
}
