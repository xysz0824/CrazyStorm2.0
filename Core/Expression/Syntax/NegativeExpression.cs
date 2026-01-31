/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
            var v = GetExpression().Eval(e);
            if (v is float) return -(float)v;
            else if (v is Core.Vector2) return -(Core.Vector2)v;
            throw new ExpressionException("TypeError");
        }

        public override void Compile(Type type, Type subType, IList<VariableResource> variables, List<byte> codeStream)
        {
            SyntaxTree expression = GetExpression();
            if (expression is Expression.Number)
            {
                byte[] code = VM.CreateInstruction(VMCode.VECTOR, new Vector3((float)Eval(null)));
                codeStream.AddRange(code);
            }
            else if (expression is Expression.Vector2)
            {
                byte[] code = VM.CreateInstruction(VMCode.VECTOR, new Vector3((Core.Vector2)Eval(null)));
                codeStream.AddRange(code);
            }
            else
            {
                byte[] code1 = VM.CreateInstruction(VMCode.VECTOR, new Vector3(0));
                codeStream.AddRange(code1);
                expression.Compile(type, subType, variables, codeStream);
                byte[] code2 = VM.CreateInstruction(VMCode.SUB);
                codeStream.AddRange(code2);
            }
        }
        public override string ToString()
        {
            return $"-{GetExpression()}";
        }
    }
}
