/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    class NegativeExpression : SyntaxTree
    {
        public NegativeExpression(Token negative, SyntaxTree expression)
        {
            Token = negative;
            AddChild(expression);
        }

        public SyntaxTree GetExpression() { return GetChildren()[0]; }

        public override object Eval(Environment e)
        {
            var num = GetExpression().Eval(e);
            if (num is int)
                return -(int)num;
            else if (num is float)
                return -(float)num;
            else
                throw new ExpressionException("Type error.");
        }
    }
}
