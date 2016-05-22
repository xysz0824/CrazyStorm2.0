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
    public class Vector2 : SyntaxTree
    {
        public Vector2(SyntaxTree x, SyntaxTree y)
        {
            AddChild(x);
            AddChild(y);
        }

        public SyntaxTree GetX() { return GetChildren()[0]; }

        public SyntaxTree GetY() { return GetChildren()[1]; }

        public override object Eval(Environment e)
        {
            var x = GetX().Eval(e);
            var y = GetY().Eval(e);
            if ((!(x is int) && !(x is float)) || (!(y is int) && !(y is float)))
                throw new ExpressionException("Type error.");

            return new Core.Vector2(Convert.ToSingle(x), Convert.ToSingle(y));
        }

        public override void Compile(List<byte> codeStream)
        {
            GetX().Compile(codeStream);
            GetY().Compile(codeStream);
            byte[] code = VM.CreateCode(VMCode.VECTOR2);
            codeStream.AddRange(code);
        }
    }
}
