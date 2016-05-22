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
    public class Number : SyntaxTree
    {
        public Number(Token token)
            : base()
        {
            Token = token;
        }

        public override object Eval(Environment e)
        {
            return Token.GetValue();
        }

        public override void Compile(List<byte> codeStream)
        {
            byte[] code = VM.CreateInstruction(VMCode.NUMBER, Eval(null));
            codeStream.AddRange(code);
        }
    }
}
