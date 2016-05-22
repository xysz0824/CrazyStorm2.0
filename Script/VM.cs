using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm.Expression
{
    public enum VMCode : byte
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
    public struct VMInstruction
    {
        public VMCode code;
        public int intOperand;
        public bool boolOperand;
        public string stringOperand;
    }
    public class VM
    {
        public static byte[] CreateInstruction(VMCode code, object operand)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)code);
            if (operand != null)
            {
                byte[] operandBytes = PlayDataHelper.GetBytes(operand);
                bytes.AddRange(operandBytes);
            }
            return bytes.ToArray();
        }
        public static byte[] CreateInstruction(VMCode code)
        {
            return CreateInstruction(code, null);
        }
        public static VMInstruction[] Decode(byte[] bytes)
        {
            List<VMInstruction> list = new List<VMInstruction>();
            int position = 0;
            while (position != bytes.Length)
            {
                VMCode code = (VMCode)bytes[position++];
                switch (code)
                {
                    case VMCode.NUMBER:
                        int intOperand = BitConverter.ToInt32(bytes, position);
                        position += sizeof(int);
                        list.Add(new VMInstruction { code = code, intOperand = intOperand });
                        break;
                    case VMCode.BOOL:
                        bool boolOperand = BitConverter.ToBoolean(bytes, position);
                        position += sizeof(bool);
                        list.Add(new VMInstruction { code = code, boolOperand = boolOperand });
                        break;
                    case VMCode.NAME:
                        string name = ReadString(bytes, position);
                        position += name.Length + 1;
                        list.Add(new VMInstruction { code = code, stringOperand = name });
                        break;
                    case VMCode.ARGUMENTS:
                        intOperand = BitConverter.ToInt32(bytes, position);
                        position += sizeof(int);
                        list.Add(new VMInstruction { code = code, intOperand = intOperand });
                        break;
                }
            }
            return list.ToArray();
        }
        public static string ReadString(byte[] bytes, int startIndex)
        {
            List<byte> stringBytes = new List<byte>();
            while (true)
            {
                byte stringByte = bytes[startIndex++];
                if (stringByte != '\0')
                    stringBytes.Add(stringByte);
                else
                    break;
            }
            return Encoding.UTF8.GetString(stringBytes.ToArray());
        }
    }
}
