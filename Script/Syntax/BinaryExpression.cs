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
            if ((!(left is int) && !(left is float)) || (!(right is int) && !(right is float)))
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
                return new ExpressionException("Type error.");
            }
            switch (op)
            {
                case "+":
                    if (left is int && right is int)
                        return (int)left + (int)right;
                    else
                        return (float)left + (float)right;
                case "-":
                    if (left is int && right is int)
                        return (int)left - (int)right;
                    else
                        return (float)left - (float)right;
                case "*":
                    if (left is int && right is int)
                        return (int)left * (int)right;
                    else
                        return (float)left * (float)right;
                case "/":
                    if (Convert.ToSingle(right) == 0)
                        throw new ExpressionException("Divided by zero.");

                    if (left is int && right is int)
                        return (int)left / (int)right;
                    else
                        return (float)left / (float)right;
                case "%":
                    if (Convert.ToSingle(right) == 0)
                        throw new ExpressionException("Divided by zero.");

                    if (left is int && right is int)
                        return (int)left % (int)right;
                    else
                        return (float)left % (float)right;
                case ">":
                    if (left is int && right is int)
                        return (int)left > (int)right;
                    else
                        return (float)left > (float)right;
                case "<":
                    if (left is int && right is int)
                        return (int)left < (int)right;
                    else
                        return (float)left < (float)right;
                case "=":
                    if (left is int && right is int)
                        return (int)left == (int)right;
                    else
                        return (float)left == (float)right;
            }
            return new ExpressionException("Type error.");
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
                    scode = VM.CreateCode(VMCode.BOOL, (bool)result);
                else
                    scode = VM.CreateCode(VMCode.NUMBER, (float)result);

                codeStream.AddRange(scode);
            }
            if (leftCanEval)
            {
                object result = left.Eval(null);
                byte[] scode = null;
                if (result is bool)
                    scode = VM.CreateCode(VMCode.BOOL, (bool)result);
                else
                    scode = VM.CreateCode(VMCode.NUMBER, (float)result);

                codeStream.AddRange(scode);
            }
            else
                left.Compile(codeStream);

            if (rightCanEval)
            {
                object result = right.Eval(null);
                byte[] scode = null;
                if (result is bool)
                    scode = VM.CreateCode(VMCode.BOOL, (bool)result);
                else
                    scode = VM.CreateCode(VMCode.NUMBER, (float)result);

                codeStream.AddRange(scode);
            }
            else
                right.Compile(codeStream);

            var op = (string)Token.GetValue();
            byte[] code = null;
            switch (op)
            {
                case "&":
                    code = VM.CreateCode(VMCode.AND);
                    break;
                case "|":
                    code = VM.CreateCode(VMCode.OR);
                    break;
                case "=":
                    code = VM.CreateCode(VMCode.EQUAL);
                    break;
                case "+":
                    code = VM.CreateCode(VMCode.ADD);
                    break;
                case "-":
                    code = VM.CreateCode(VMCode.SUB);
                    break;
                case "*":
                    code = VM.CreateCode(VMCode.MUL);
                    break;
                case "/":
                    code = VM.CreateCode(VMCode.DIV);
                    break;
                case "%":
                    code = VM.CreateCode(VMCode.MOD);
                    break;
                case ">":
                    code = VM.CreateCode(VMCode.MORE);
                    break;
                case "<":
                    code = VM.CreateCode(VMCode.LESS);
                    break;
            }
            codeStream.AddRange(code);
        }
    }
}
