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
    public class NegativeExpression : SyntaxTree
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

        public override void Compile(List<byte> codeStream)
        {
            SyntaxTree expression = GetExpression();
            if (expression.ContainType<Expression.Name>() || expression.ContainType<Expression.Call>())
            {
                byte[] code1 = VM.CreateCode(VMCode.NUMBER, 0);
                codeStream.AddRange(code1);
                expression.Compile(codeStream);
                byte[] code2 = VM.CreateCode(VMCode.SUB);
                codeStream.AddRange(code2);
            }
            else
            {
                byte[] code = VM.CreateCode(VMCode.NUMBER, (float)Eval(null));
                codeStream.AddRange(code);
            }
        }
    }
}
