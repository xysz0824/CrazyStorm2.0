using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm
{
    class ParameterEmptyException : ApplicationException
    {
        public ParameterEmptyException() { }
        public ParameterEmptyException(string message)
            : base(message) { }
        public ParameterEmptyException(string message, Exception inner)
            : base(message, inner) { }
    }
}
