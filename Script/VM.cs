using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm.Expression
{
    enum VMCode : byte
    {
        NUMBER = 1,
        BOOL,
        NAME,
        VECTOR2,
        RGB,
        ARGUMENTS,
        CALL,
        AND,
        OR,
        EQUAL,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        MORE,
        LESS
    }
    class VM
    {
        public static byte[] CreateCode(VMCode code, object operand)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)code);
            if (operand != null)
            {
                byte[] operandBytes = PlayDataHelper.GetBytes(operand);
                bytes.AddRange(PlayDataHelper.GetBytes(operandBytes.Length));
                bytes.AddRange(operandBytes);
            }
            return bytes.ToArray();
        }
        public static byte[] CreateCode(VMCode code)
        {
            return CreateCode(code, null);
        }
    }
}
