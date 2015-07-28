using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{

    class BinaryExpression : SyntaxTree
    {
        public BinaryExpression(Token op, SyntaxTree left, SyntaxTree right)
            : base()
        {
            Token = op;
            AddChild(left);
            AddChild(right);
        }

        public SyntaxTree GetLeftChild() { return GetChildren()[0]; }

        public SyntaxTree GetRightChild() { return GetChildren()[1]; }

        public override object Test(Environment e)
        {
            var left = GetLeftChild().Test(e);
            var right = GetRightChild().Test(e);
            if ((!(left is int) && !(left is float)) || (!(right is int) && !(right is float)))
                return new ScriptException("Type error.");

            switch ((string)Token.GetValue())
            {
                case "/":
                    if (Convert.ToSingle(right) == 0)
                        throw new ScriptException("Divided by zero.");
                    break;
                case "%":
                    if (Convert.ToSingle(right) == 0)
                        throw new ScriptException("Divided by zero.");
                    break;
            }
            //Execute method is just for testing,
            //which means it doesn't need to call real function.
            return 0.0f;
        }
    }
}
