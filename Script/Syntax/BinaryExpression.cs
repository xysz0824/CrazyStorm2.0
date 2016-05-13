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

    class BinaryExpression : SyntaxTree
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
    }
}
