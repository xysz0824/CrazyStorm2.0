using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Arguments : SyntaxTree
    {
        public Arguments(List<SyntaxTree> arguments)
            : base()
        {
            foreach (var argument in arguments)
                AddChild(argument);
        }

        public int Count { get { return GetChildren().Count; } }

        public SyntaxTree GetArgument(int i) { return GetChildren()[i]; }
    }
}
