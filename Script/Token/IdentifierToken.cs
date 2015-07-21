using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrazyStorm.Script
{
    class IdentifierToken : Token
    {
        string value;
        public IdentifierToken(int lineNumber, int index, string value)
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
