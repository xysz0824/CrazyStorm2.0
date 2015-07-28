using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
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
