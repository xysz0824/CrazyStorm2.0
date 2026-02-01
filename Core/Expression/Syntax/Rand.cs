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
        //{a,b}
        public Rand(SyntaxTree left, SyntaxTree right)
        {
            AddChild(left);
            AddChild(right);
        }
        //{x} => {-x, x}
        public Rand(SyntaxTree unique)
        {
            AddChild(unique);
        }

        public SyntaxTree GetLeft() { return GetChildren()[0]; }

        public SyntaxTree GetRight() { return GetChildren()[1]; }
        public bool HasRight() => GetChildren().Count > 1;

        public override object Eval(Environment e)
        {
            var left = GetLeft().Eval(e);
            if (HasRight())
            {
                var right = GetRight().Eval(e);
                if ((!(left is int) && !(left is float)) || (!(right is int) && !(right is float)))
                {
                    throw new ExpressionException("TypeError");
                }
                float ratio = (float)new Random().NextDouble();
                return (float)left * (1 - ratio) + (float)right * ratio;
            }
            else if (GetLeft() is Number)
            {
                float ratio = (float)new Random().NextDouble();
                return (-(float)left) * (1 - ratio) + (float)left * ratio;
            }
            else throw new ExpressionException("IllegalInput");
        }

        public override void Compile(Type type, Type subType, List<VariableResource> variables, List<byte> codeStream)
        {
            if (HasRight())
            {
                GetLeft().Compile(type, subType, variables, codeStream);
                GetRight().Compile(type, subType, variables, codeStream);
                codeStream.AddRange(VM.CreateInstruction(VMCode.RAND));
            }
            else
            {
                //Left must be number
                var v = (float)GetLeft().Eval(null);
                codeStream.AddRange(VM.CreateInstruction(VMCode.VECTOR, new Vector3(-v)));
                codeStream.AddRange(VM.CreateInstruction(VMCode.VECTOR, new Vector3(v)));
                codeStream.AddRange(VM.CreateInstruction(VMCode.RAND));
            }
        }
        public override string ToString()
        {
            return HasRight() ? $"{{{GetLeft()},{GetRight()}}}" : $"{{{GetLeft()}}}";
        }
    }
}
