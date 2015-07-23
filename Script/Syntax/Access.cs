using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Access : SyntaxTree
    {
        public Access(SyntaxTree obj)
        {
            AddChild(obj);
        }

        public SyntaxTree GetObject() { return GetChildren()[0]; }
    }
}
