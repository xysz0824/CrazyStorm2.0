using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm_Player.CrazyStorm
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
        public float floatOperand;
        public bool boolOperand;
        public string stringOperand;
    }
    class VM
    {
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
                        float floatOperand = BitConverter.ToSingle(bytes, position);
                        position += sizeof(float);
                        list.Add(new VMInstruction { code = code, floatOperand = floatOperand });
                        break;
                    case VMCode.BOOL:
                        bool boolOperand = BitConverter.ToBoolean(bytes, position);
                        position += sizeof(bool);
                        list.Add(new VMInstruction { code = code, boolOperand = boolOperand });
                        break;
                    case VMCode.NAME:
                        string name = PlayDataHelper.ReadString(bytes, position);
                        position += name.Length + 1;
                        list.Add(new VMInstruction { code = code, stringOperand = name });
                        break;
                    case VMCode.ARGUMENTS:
                        int intOperand = BitConverter.ToInt32(bytes, position);
                        position += sizeof(int);
                        list.Add(new VMInstruction { code = code, intOperand = intOperand });
                        break;
                    default:
                        list.Add(new VMInstruction { code = code });
                        break;
                }
            }
            return list.ToArray();
        }
    }
}
