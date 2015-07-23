using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CrazyStorm.Script
{
    [Serializable]
    public class CompileException : ApplicationException
    {
        public CompileException() { }
        public CompileException(string message)
            : base(message) { }
        public CompileException(string message, Exception inner)
            : base(message, inner) { }
        public CompileException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
