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
    class Name : SyntaxTree
    {
        public Name(Token token)
            : base()
        {
            Token = token;
        }

        public override object Eval(Environment e)
        {
            var name = (string)Token.GetValue();
            //Find this in local variable.
            var result = e.GetLocal(name);
            if (result == null)
            {
                //Find this in global variable.
                result = e.GetGlobal(name);
                if (result == null)
                    throw new ExpressionException("Undefination error.");
            }
            return result;
        }
    }
}
