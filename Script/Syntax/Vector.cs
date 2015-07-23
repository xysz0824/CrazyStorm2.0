using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    class Vector : SyntaxTree
    {
        public Vector(List<SyntaxTree> coordinates)
        {
            foreach (var coordinate in coordinates)
                AddChild(coordinate);
        }

        public int Dimension { get { return GetChildren().Count; } }

        public SyntaxTree GetCoordinate(int i) { return GetChildren()[i]; }
    }
}
