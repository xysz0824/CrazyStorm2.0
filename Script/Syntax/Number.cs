using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Number : SyntaxTree
    {
        public Number(Token token)
            : base()
        {
            Token = token;
        }

        public double GetNumber() { return (float)Token.GetValue(); }
    }
}
