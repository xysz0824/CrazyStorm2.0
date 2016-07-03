/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    class IdentifierToken : Token
    {
        string value;
        bool isOperator;
        public IdentifierToken(int lineNumber, int index, string value, bool isOperator)
            : base(lineNumber, index)
        {
            this.value = value;
            this.isOperator = isOperator;
        }

        public bool IsOperator { get { return isOperator; } }

        public override object GetValue()
        {
            return value;
        }
    }
}
