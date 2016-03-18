/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class RGB : SyntaxTree
    {
        public RGB(SyntaxTree r, SyntaxTree g, SyntaxTree b)
        {
            AddChild(r);
            AddChild(g);
            AddChild(b);
        }

        public SyntaxTree GetR() { return GetChildren()[0]; }

        public SyntaxTree GetG() { return GetChildren()[1]; }

        public SyntaxTree GetB() { return GetChildren()[2]; }

        public override object Test(Environment e)
        {
            var r = GetR().Test(e);
            var g = GetG().Test(e);
            var b = GetB().Test(e);
            if ((!(r is int) && !(r is float)) || (!(g is int) && !(g is float)) || (!(b is int) && !(b is float)))
                throw new ScriptException("Type error.");

            return new Core.RGB(Convert.ToSingle(r), Convert.ToSingle(g), Convert.ToSingle(b));
        }
    }
}
