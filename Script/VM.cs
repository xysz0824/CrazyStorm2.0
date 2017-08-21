/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm.Expression
{
    public enum VMCode : byte
    {
        NUMBER,
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
    public class VM
    {
        public static byte[] CreateInstruction(VMCode code, object operand)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)code);
            if (operand != null)
            {
                switch (code)
                {
                    case VMCode.NUMBER:
                        bytes.AddRange(PlayDataHelper.GetBytes((float)operand));
                        break;
                    case VMCode.BOOL:
                        bytes.AddRange(PlayDataHelper.GetBytes((bool)operand));
                        break;
                    case VMCode.NAME:
                        bytes.AddRange(PlayDataHelper.GetBytes((string)operand));
                        break;
                    case VMCode.CALL:
                        bytes.AddRange(PlayDataHelper.GetBytes((string)operand));
                        break;
                    case VMCode.ARGUMENTS:
                        bytes.AddRange(PlayDataHelper.GetBytes((int)operand));
                        break;
                }
            }
            return bytes.ToArray();
        }
        public static byte[] CreateInstruction(VMCode code)
        {
            return CreateInstruction(code, null);
        }
    }
}
