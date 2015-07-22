using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrazyStorm.Script
{
    class NumberToken : Token
    {
        float value;
        public NumberToken(int lineNumber, int index, float value)
            : base(lineNumber, index)
        {
            this.value = value;
        }

        public override object GetValue()
        {
            return value;
        }
    }
}
