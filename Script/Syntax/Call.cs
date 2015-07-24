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
    }
}
