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

        public override void Compile(Type type, Type subType, List<VariableResource> variables, List<byte> codeStream)
        {
            var name = (string)Token.GetValue();
            var propertyID = Environment.GetPropertyID(name, type, subType, variables);
            byte[] code = null;
            if (propertyID != int.MinValue) code = VM.CreateInstruction(VMCode.NAME, propertyID);
            else code = VM.CreateInstruction(VMCode.NAME, name);
            codeStream.AddRange(code);
        }
        public override string ToString()
        {
            return (string)Token.GetValue();
        }
    }
}
