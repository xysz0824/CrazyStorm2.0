using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class IdentifierToken : Token
    {
        string value;
        bool isOperator;
        public IdentifierToken(int lineNumber, int index, string value, bool isOperator)
            : base(lineNumber, index)
        {
            this.value = value;
            this.isOperator = isOperator;
        }

        public bool IsOperator { get { return isOperator; } }

        public override object GetValue()
        {
            return value;
        }
    }
}
