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
    class Call : SyntaxTree
    {
        public Call(Token name, SyntaxTree arguments)
            : base()
        {
            Token = name;
            AddChild(arguments);
        }

        public SyntaxTree GetArguments() { return GetChildren()[0]; }

        public override object Test(Environment e)
        {
            var function = e.GetFunction((string)Token.GetValue());
            if (function == null)
                throw new ScriptException("Undefination error.");

            var argumentList = (List<object>)GetArguments().Test(e);
            if (argumentList.Count != function.ArgumentCount)
                throw new ScriptException("Argument error.");

            foreach (var item in argumentList)
                if (!(item is int) && !(item is float))
                    throw new ScriptException("Type error.");

            //Execute method is just for testing,
            //which means it doesn't need to call real function.
            return 0.0f;
        }
    }
}
