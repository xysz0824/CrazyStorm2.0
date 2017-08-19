/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CrazyStorm.Core
{
    [Serializable]
    class PlayDataException : Exception
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
