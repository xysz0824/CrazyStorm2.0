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
            //Find in property variables.
            var result = e.GetProperty(name);
            if (result == null)
            {
                //Find in local variables.
                result = e.GetLocal(name);
                if (result == null)
                {
                    //Find in global variables.
                    result = e.GetGlobal(name);
                    if (result == null)
                        throw new ExpressionException("Undefination error.");
                }
            }
            return result;
        }
    }
}
