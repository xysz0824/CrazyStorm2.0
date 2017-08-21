/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    public class Call : SyntaxTree
    {
        public Call(Token name, SyntaxTree arguments)
            : base()
        {
            Token = name;
            AddChild(arguments);
        }

        public SyntaxTree GetArguments() { return GetChildren()[0]; }

        public override object Eval(Environment e)
        {
            var functionName = (string)Token.GetValue();
            var function = e.GetFunction(functionName);
            if (function == null)
                throw new ExpressionException("UndefinationError");

            var argumentList = GetArguments().Eval(e) as List<object>;
            if (argumentList.Count != function.ArgumentCount)
                throw new ExpressionException("ArgumentError");

            for(int i = 0;i < argumentList.Count;++i)
                if (argumentList[i].GetType() != function.ArgumentTypes[i])
                    throw new ExpressionException("TypeError");

            //Calling function is a runtime operation so it can't be evaluated right here.
            //Return a false value.
            return 0.0f;
        }

        public override void Compile(List<byte> codeStream)
        {
            GetArguments().Compile(codeStream);
            byte[] code = VM.CreateInstruction(VMCode.CALL, (string)Token.GetValue());
            codeStream.AddRange(code);
        }
    }
}
