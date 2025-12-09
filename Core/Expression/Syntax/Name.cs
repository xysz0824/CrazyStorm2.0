/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm.Expression
{
    public class Name : SyntaxTree
    {
        public Name(Token token)
            : base()
        {
            Token = token;
        }

        public override object Eval(Environment e)
        {
            var name = (string)Token.GetValue();
            var result = e.GetProperty(name);
            if (result is int)
                return (float)(int)result;

            if (result == null)
            {
                result = e.GetLocal(name);
                if (result == null)
                {
                    result = e.GetGlobal(name);
                    if (result == null)
                        throw new ExpressionException("UndefinationError");
                }
            }
            return result;
        }

        public override void Compile(List<byte> codeStream)
        {
            byte[] code = VM.CreateInstruction(VMCode.NAME, (string)Token.GetValue());
            codeStream.AddRange(code);
        }
    }
}
