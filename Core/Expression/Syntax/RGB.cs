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
    public class RGB : SyntaxTree
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

        public override object Eval(Environment e)
        {
            var r = GetR().Eval(e);
            var g = GetG().Eval(e);
            var b = GetB().Eval(e);
            if ((!(r is int) && !(r is float)) || (!(g is int) && !(g is float)) || (!(b is int) && !(b is float)))
                throw new ExpressionException("TypeError");

            return new Core.RGB(Convert.ToSingle(r), Convert.ToSingle(g), Convert.ToSingle(b));
        }

        public override void Compile(Type type, Type subType, IList<VariableResource> variables, List<byte> codeStream)
        {
            GetR().Compile(type, subType, variables, codeStream);
            GetG().Compile(type, subType, variables, codeStream);
            GetB().Compile(type, subType, variables, codeStream);
            byte[] code = VM.CreateInstruction(VMCode.RGB);
            codeStream.AddRange(code);
        }
        public override string ToString()
        {
            return $"[{GetR()},{GetG()},{GetB()}]";
        }
    }
}
