using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrazyStorm.Script
{
    public abstract class Token : IComparable
    {
        int lineNumber;
        int index;
        public int LineNumber { get { return lineNumber; } }
        public int Index { get { return index; } }

        public Token(int lineNumber, int index)
        {
            this.lineNumber = lineNumber;
            this.index = index;
        }

        public abstract object GetValue();

        public int CompareTo(object obj)
        {
            Token token = obj as Token;
            return Index.CompareTo(token.Index);
        }
    }
}
