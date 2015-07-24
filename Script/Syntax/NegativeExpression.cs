using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class NegativeExpression : SyntaxTree
    {
        public NegativeExpression(Token negative, SyntaxTree expression)
        {
            Token = negative;
            AddChild(expression);
        }

        public SyntaxTree GetExpression() { return GetChildren()[0]; }
    }
}
