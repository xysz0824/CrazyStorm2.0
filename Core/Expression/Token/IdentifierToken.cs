/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Expression
{
    public enum OperatorTokenType
    {
        None,
        Logic,
        Arithmetic,
    }
    public class IdentifierToken : Token
    {
        string value;
        OperatorTokenType operatorType;
        public IdentifierToken(int lineNumber, int index, string value, OperatorTokenType operatorType)
            : base(lineNumber, index)
        {
            this.value = value;
            this.operatorType = operatorType;
        }

        public OperatorTokenType OperatorType => operatorType;

        public override object GetValue()
        {
            return value;
        }
        public override void SetValue(object value)
        {
            this.value = (string)value;
        }
    }
}
