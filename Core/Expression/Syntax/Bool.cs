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
    public class Bool : SyntaxTree
    {
        public Bool(Token token)
            : base()
        {
            Token = token;
        }

        public override object Eval(Environment e)
        {
            return bool.Parse((string)Token.GetValue());
        }

        public override void Compile(Type type, Type subType, List<VariableResource> variables, List<byte> codeStream)
        {
            byte[] code = VM.CreateInstruction(VMCode.BOOL, Eval(null));
            codeStream.AddRange(code);
        }
        public override string ToString()
        {
            return (string)Token.GetValue();
        }
    }
}
