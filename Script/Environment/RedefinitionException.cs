using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CrazyStorm.Expression
{
    [Serializable]
    public class RedefinitionException : ApplicationException
    {
        public RedefinitionException() { }
        public RedefinitionException(string message)
            : base(message) { }
        public RedefinitionException(string message, Exception inner)
            : base(message, inner) { }
        public RedefinitionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
