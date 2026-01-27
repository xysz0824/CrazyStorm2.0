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
        public bool SimpleLeftExpression
        {
            get
            {
                var left = GetLeftChild();
                return (!(left is BinaryExpression)) && (!(left is NegativeExpression)) &&
                    (!(left is Call));
            }
        }   
        public override object Eval(Environment e)
        {
            var left = GetLeftChild().Eval(e);
            var right = GetRightChild().Eval(e);
            var op = (string)Token.GetValue();
            if (left is float && right is float)
            {
                switch (op)
                {
                    case "+":
                        return (float)left + (float)right;
                    case "-":
                        return (float)left - (float)right;
                    case "*":
                        return (float)left * (float)right;
                    case "/":
                        if (GetRightChild() is Number && Convert.ToSingle(right) == 0)
                        {
                            throw new ExpressionException("DividedByZero");
                        }
                        return (float)left / (float)right;
                    case "%":
                        if (GetRightChild() is Number && Convert.ToSingle(right) == 0)
                        {
                            throw new ExpressionException("DividedByZero");
                        }
                        return (float)left % (float)right;
                    case ">":
                        return (float)left > (float)right;
                    case ">=":
                        return (float)left >= (float)right;
                    case "<":
                        return (float)left < (float)right;
                    case "<=":
                        return (float)left <= (float)right;
                    case "=":
                        return (float)left == (float)right;
                    case "!=":
                        return (float)left != (float)right;
                }
            }
            else if (left is bool && right is bool)
            {
                switch (op)
                {
                    case "&":
                        return (bool)left && (bool)right;
                    case "|":
                        return (bool)left || (bool)right;
                    case "=":
                        return (bool)left == (bool)right;
                    case "!=":
                        return (bool)left != (bool)right;
                }
            }
            else if (left is Core.Vector2 && right is Core.Vector2)
            {
                switch (op)
                {
                    case "+":
                        return (Core.Vector2)left + (Core.Vector2)right;
                    case "-":
                        return (Core.Vector2)left - (Core.Vector2)right;
                    case "=":
                        return (Core.Vector2)left == (Core.Vector2)right;
                    case "!=":
                        return (Core.Vector2)left != (Core.Vector2)right;
                }
            }
            else if (left is Core.Vector2 && right is float)
            {
                switch (op)
                {
                    case "+":
                        return (Core.Vector2)left + (float)right;
                    case "-":
                        return (Core.Vector2)left - (float)right;
                    case "*":
                        return (Core.Vector2)left * (float)right;
                    case "/":
                        if (GetRightChild() is Number && Convert.ToSingle(right) == 0)
                        {
                            throw new ExpressionException("DividedByZero");
                        }
                        return (Core.Vector2)left / (float)right;
                }
            }
            else if (left is Core.RGB && right is Core.RGB)
            {
                switch (op)
                {
                    case "+":
                        return (Core.RGB)left + (Core.RGB)right;
                    case "-":
                        return (Core.RGB)left - (Core.RGB)right;
                    case "=":
                        return (Core.RGB)left == (Core.RGB)right;
                    case "!=":
                        return (Core.RGB)left != (Core.RGB)right;
                }
            }
            else if (left is Core.RGB && right is float)
            {
                switch (op)
                {
                    case "+":
                        return (Core.RGB)left + (float)right;
                    case "-":
                        return (Core.RGB)left - (float)right;
                    case "*":
                        return (Core.RGB)left * (float)right;
                    case "/":
                        if (GetRightChild() is Number && Convert.ToSingle(right) == 0)
                        {
                            throw new ExpressionException("DividedByZero");
                        }
                        return (Core.RGB)left / (float)right;
                }
            }
            return new ExpressionException("TypeError");
        }

        public override void Compile(List<byte> codeStream)
        {
            SyntaxTree left = GetLeftChild();
            SyntaxTree right = GetRightChild();
            bool leftCanEval = CanEval(left);
            bool rightCanEval = CanEval(right);
            if (leftCanEval && rightCanEval)
            {
                object result = Eval(null);
                byte[] scode = null;
                if (result is bool) scode = VM.CreateInstruction(VMCode.BOOL, (bool)result);
                else if (result is float) scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3((float)result));
                else if (result is Core.Vector2) scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3((Core.Vector2)result));
                else if (result is Core.RGB)
                {
                    var rgb = (Core.RGB)result;
                    scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3(rgb.r, rgb.g, rgb.b));
                }
                codeStream.AddRange(scode);
                return;
            }
            if (leftCanEval)
            {
                object result = left.Eval(null);
                byte[] scode = null;
                if (result is bool) scode = VM.CreateInstruction(VMCode.BOOL, (bool)result);
                else if (result is float) scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3((float)result));
                else if (result is Core.Vector2) scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3((Core.Vector2)result));
                else if (result is Core.RGB)
                {
                    var rgb = (Core.RGB)result;
                    scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3(rgb.r, rgb.g, rgb.b));
                }
                codeStream.AddRange(scode);
            }
            else
                left.Compile(codeStream);

            if (rightCanEval)
            {
                object result = right.Eval(null);
                byte[] scode = null;
                if (result is bool) scode = VM.CreateInstruction(VMCode.BOOL, (bool)result);
                else if (result is float) scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3((float)result));
                else if (result is Core.Vector2) scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3((Core.Vector2)result));
                else if (result is Core.RGB)
                {
                    var rgb = (Core.RGB)result;
                    scode = VM.CreateInstruction(VMCode.VECTOR, new Vector3(rgb.r, rgb.g, rgb.b));
                }
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
                case ">=":
                    code = VM.CreateInstruction(VMCode.MOREOREQUAL);
                    break;
                case "<=":
                    code = VM.CreateInstruction(VMCode.LESSOREQUAL);
                    break;
                case "!=":
                    code = VM.CreateInstruction(VMCode.NOTEQUAL);
                    break;
            }
            codeStream.AddRange(code);
        }
        public override string ToString()
        {
            return $"{GetLeftChild()}{(string)Token.GetValue()}{GetRightChild()}";
        }   
    }
}
