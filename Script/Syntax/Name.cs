using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Name : SyntaxTree
    {
        public Name(Token token)
        {
            Token = token;
        }

        public string GetName() { return (string)Token.GetValue(); }
    }
}
