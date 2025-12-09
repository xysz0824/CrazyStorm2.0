/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

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
            if (num is float)
                return -(float)num;
            else
                throw new ExpressionException("TypeError");
        }

        public override void Compile(List<byte> codeStream)
        {
            SyntaxTree expression = GetExpression();
            if (expression.ContainType<Expression.Name>() || expression.ContainType<Expression.Call>())
            {
                byte[] code1 = VM.CreateInstruction(VMCode.NUMBER, 0);
                codeStream.AddRange(code1);
                expression.Compile(codeStream);
                byte[] code2 = VM.CreateInstruction(VMCode.SUB);
                codeStream.AddRange(code2);
            }
            else
            {
                byte[] code = VM.CreateInstruction(VMCode.NUMBER, (float)Eval(null));
                codeStream.AddRange(code);
            }
        }
    }
}
