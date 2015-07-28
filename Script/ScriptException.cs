using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CrazyStorm.Script
{
    [Serializable]
    public class ScriptException : ApplicationException
    {
        public ScriptException() { }
        public ScriptException(string message)
            : base(message) { }
        public ScriptException(string message, Exception inner)
            : base(message, inner) { }
        public ScriptException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
