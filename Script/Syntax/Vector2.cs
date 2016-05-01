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
    class Vector2 : SyntaxTree
    {
        public Vector2(SyntaxTree x, SyntaxTree y)
        {
            AddChild(x);
            AddChild(y);
        }

        public SyntaxTree GetX() { return GetChildren()[0]; }

        public SyntaxTree GetY() { return GetChildren()[1]; }

        public override object Test(Environment e)
        {
            var x = GetX().Test(e);
            var y = GetY().Test(e);
            if ((!(x is int) && !(x is float)) || (!(y is int) && !(y is float)))
                throw new ExpressionException("Type error.");

            return new Core.Vector2(Convert.ToSingle(x), Convert.ToSingle(y));
        }
    }
}
