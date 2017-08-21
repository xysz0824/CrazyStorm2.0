/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CrazyStorm.Core
{
    [Serializable]
    public class PlayDataException : Exception
    {
        public PlayDataException() { }
        public PlayDataException(string message)
            : base(message) { }
        public PlayDataException(string message, Exception inner)
            : base(message, inner) { }
        public PlayDataException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
