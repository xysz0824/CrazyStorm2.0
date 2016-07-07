/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    public class BinaryExpression : SyntaxTree
    {
        public BinaryExpression(Token op, SyntaxTree left, SyntaxTree right)
            : base()
        {
            Token = op;
            AddChild(left);
            AddChild(right);
        }

        public SyntaxTree GetLeftChild() { return GetChildren()[0]; }

        public SyntaxTree GetRightChild() { return GetChildren()[1]; }

        public override object Eval(Environment e)
        {
            var left = GetLeftChild().Eval(e);
            var right = GetRightChild().Eval(e);
            var op = (string)Token.GetValue();
            if (!(left is float) && !(right is float))
            {
                if (left is bool && right is bool)
                {
                    switch (op)
                    {
                        case "&":
                            return (bool)left && (bool)right;
                        case "|":
                            return (bool)left || (bool)right;
                        case "=":
                            return (bool)left == (bool)right;
                    }
                }
                return new ExpressionException("TypeError");
            }
            switch (op)
            {
                case "+":
                    return (float)left + (float)right;
                case "-":
                    return (float)left - (float)right;
                case "*":
                    return (float)left * (float)right;
                case "/":
                    if (Convert.ToSingle(right) == 0)
                        throw new ExpressionException("DividedByZero");

                    return (float)left / (float)right;
                case "%":
                    if (Convert.ToSingle(right) == 0)
                        throw new ExpressionException("DividedByZero");

                    return (float)left % (float)right;
                case ">":
                    return (float)left > (float)right;
                case "<":
                    return (float)left < (float)right;
                case "=":
                    return (float)left == (float)right;
            }
            return new ExpressionException("TypeError");
        }

        public override void Compile(List<byte> codeStream)
        {
            SyntaxTree left = GetLeftChild();
            SyntaxTree right = GetRightChild();
            bool leftCanEval = !left.ContainType<Expression.Name>() && !left.ContainType<Expression.Call>();
            bool rightCanEval = !right.ContainType<Expression.Name>() && !right.ContainType<Expression.Call>();
            if (leftCanEval && rightCanEval)
            {
                object result = Eval(null);
                byte[] scode = null;
                if (result is bool)
                    scode = VM.CreateInstruction(VMCode.BOOL, (bool)result);
                else
                    scode = VM.CreateInstruction(VMCode.NUMBER, (float)result);

                codeStream.AddRange(scode);
                return;
            }
            if (leftCanEval)
            {
                object result = left.Eval(null);
                byte[] scode = null;
                if (result is bool)
                    scode = VM.CreateInstruction(VMCode.BOOL, (bool)result);
                else
                    scode = VM.CreateInstruction(VMCode.NUMBER, (float)result);

                codeStream.AddRange(scode);
            }
            else
                left.Compile(codeStream);

            if (rightCanEval)
            {
                object result = right.Eval(null);
                byte[] scode = null;
                if (result is bool)
                    scode = VM.CreateInstruction(VMCode.BOOL, (bool)result);
                else
                    scode = VM.CreateInstruction(VMCode.NUMBER, (float)result);

                codeStream.AddRange(scode);
            }
            else
                right.Compile(codeStream);

            var op = (string)Token.GetValue();
            byte[] code = null;
            switch (op)
            {
                case "&":
                    code = VM.CreateInstruction(VMCode.AND);
                    break;
                case "|":
                    code = VM.CreateInstruction(VMCode.OR);
                    break;
                case "=":
                    code = VM.CreateInstruction(VMCode.EQUAL);
                    break;
                case "+":
                    code = VM.CreateInstruction(VMCode.ADD);
                    break;
                case "-":
                    code = VM.CreateInstruction(VMCode.SUB);
                    break;
                case "*":
                    code = VM.CreateInstruction(VMCode.MUL);
                    break;
                case "/":
                    code = VM.CreateInstruction(VMCode.DIV);
                    break;
                case "%":
                    code = VM.CreateInstruction(VMCode.MOD);
                    break;
                case ">":
                    code = VM.CreateInstruction(VMCode.MORE);
                    break;
                case "<":
                    code = VM.CreateInstruction(VMCode.LESS);
                    break;
            }
            codeStream.AddRange(code);
        }
    }
}
