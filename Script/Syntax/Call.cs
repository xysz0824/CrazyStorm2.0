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
    class Call : SyntaxTree
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
                throw new ExpressionException("Undefination error.");

            var argumentList = GetArguments().Eval(e) as List<object>;
            if (argumentList.Count != function.ArgumentCount)
                throw new ExpressionException("Argument error.");

            foreach (var item in argumentList)
                if (!(item is int) && !(item is float))
                    throw new ExpressionException("Type error.");

            //Calling function is a runtime operation so it can't be evaluated right here.
            //Return a false value.
            return 0.0f;
        }
    }
}
