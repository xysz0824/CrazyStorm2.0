using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Vector3 : SyntaxTree
    {
        public Vector3(SyntaxTree x, SyntaxTree y, SyntaxTree z)
        {
            AddChild(x);
            AddChild(y);
            AddChild(z);
        }

        public SyntaxTree GetX() { return GetChildren()[0]; }

        public SyntaxTree GetY() { return GetChildren()[1]; }

        public SyntaxTree GetZ() { return GetChildren()[2]; }
    }
}
