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

        public override object Test(Environment e)
        {
            var left = GetLeftChild().Test(e);
            var right = GetRightChild().Test(e);
            var op = (string)Token.GetValue();
            //Execute method is just for testing,
            //which means it doesn't need to calculate result.
            if ((!(left is int) && !(left is float)) || (!(right is int) && !(right is float)))
            {
                if (((left is Core.Vector2 && right is Core.Vector2) || (left is Core.RGB && right is Core.RGB))
                    && (op == "+" || op == "-"))
                    return left;

                if (left is bool && right is bool && (op == "&" || op == "|" || op == "="))
                    return left;

                    return new ExpressionException("Type error.");
            }
            switch (op)
            {
                case "/":
                    if (Convert.ToSingle(right) == 0)
                        throw new ExpressionException("Divided by zero.");
                    break;
                case "%":
                    if (Convert.ToSingle(right) == 0)
                        throw new ExpressionException("Divided by zero.");
                    break;
                case ">":
                    return true;
                case "<":
                    return true;
                case "=":
                    return true;
            }
            return left;
        }
    }
}
