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

        public override object Test(Environment e)
        {
            var num = GetExpression().Test(e);
            if ((!(num is int) && !(num is float)))
                throw new ScriptException("Type error.");

            //Execute method is just for testing,
            //which means it doesn't need to call real function.
            return 0.0f;
        }
    }
}
