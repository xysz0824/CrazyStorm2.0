using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CrazyStorm
{
    [Serializable]
    public class ParameterEmptyException : ApplicationException
    {
        public ParameterEmptyException() { }
        public ParameterEmptyException(string message)
            : base(message) { }
        public ParameterEmptyException(string message, Exception inner)
            : base(message, inner) { }
        public ParameterEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
