using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Access : SyntaxTree
    {
        public Access(Token var, SyntaxTree obj)
        {
            Token = var;
            AddChild(obj);
        }

        public SyntaxTree GetObject() { return GetChildren()[0]; }
    }
}
