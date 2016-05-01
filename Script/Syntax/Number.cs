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

        public override object Test(Environment e)
        {
            return Token.GetValue();
        }
    }
}
