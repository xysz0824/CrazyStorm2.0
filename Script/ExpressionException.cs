/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CrazyStorm.Expression
{
    [Serializable]
    public class ExpressionException : ApplicationException
    {
        public ExpressionException() { }
        public ExpressionException(string message)
            : base(message) { }
        public ExpressionException(string message, Exception inner)
            : base(message, inner) { }
        public ExpressionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
