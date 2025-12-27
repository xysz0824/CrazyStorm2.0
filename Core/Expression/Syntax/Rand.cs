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
    public class Rand : SyntaxTree
    {
        public Rand(SyntaxTree left, SyntaxTree right)
        {
            AddChild(left);
            AddChild(right);
        }

        public SyntaxTree GetLeft() { return GetChildren()[0]; }

        public SyntaxTree GetRight() { return GetChildren()[1]; }

        public override object Eval(Environment e)
        {
            var left = GetLeft().Eval(e);
            var right = GetRight().Eval(e);
            if ((!(left is int) && !(left is float)) || (!(right is int) && !(right is float)))
            {
                throw new ExpressionException("TypeError");
            }
            float ratio = (float)new Random().NextDouble();
            return (float)left * (1 - ratio) + (float)right * ratio;
        }

        public override void Compile(List<byte> codeStream)
        {
            GetLeft().Compile(codeStream);
            GetRight().Compile(codeStream);
            byte[] code = VM.CreateInstruction(VMCode.RAND);
            codeStream.AddRange(code);
        }
        public override string ToString()
        {
            return $"{{{GetLeft()},{GetRight()}}}";
        }
    }
}
