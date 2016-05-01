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
    class Bool : SyntaxTree
    {
        public Bool(Token token)
            : base()
        {
            Token = token;
        }

        public override object Test(Environment e)
        {
            return bool.Parse((string)Token.GetValue());
        }
    }
}
