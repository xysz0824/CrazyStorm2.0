using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{

    class BinaryExpression : SyntaxTree
    {
        public BinaryExpression(Token operatorToken, SyntaxTree left, SyntaxTree right)
            : base()
        {
            Token = operatorToken;
            AddChild(left);
            AddChild(right);
        }

        public SyntaxTree GetLeftChild() { return GetChildren()[0]; }

        public SyntaxTree GetRightChild() { return GetChildren()[1]; }
    }
}
