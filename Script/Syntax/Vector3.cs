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

        public override object Test(Environment e)
        {
            var x = GetX().Test(e);
            var y = GetY().Test(e);
            var z = GetZ().Test(e);
            if ((!(x is int) && !(x is float)) || (!(y is int) && !(y is float)) || (!(z is int) && !(z is float)))
                throw new ScriptException("Type error.");

            return new Core.Vector3(Convert.ToSingle(x), Convert.ToSingle(y), Convert.ToSingle(z));
        }
    }
}
