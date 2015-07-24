using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Vector2 : SyntaxTree
    {
        public Vector2(SyntaxTree x, SyntaxTree y)
        {
            AddChild(x);
            AddChild(y);
        }

        public SyntaxTree GetX() { return GetChildren()[0]; }

        public SyntaxTree GetY() { return GetChildren()[1]; }
    }
}
