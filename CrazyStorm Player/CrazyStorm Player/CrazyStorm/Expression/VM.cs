using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
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
        static Stack<bool> boolStack = new Stack<bool>();
        static Stack<int> intStack = new Stack<int>();
        static Stack<float> floatStack = new Stack<float>();
        static Stack<int> enumStack = new Stack<int>();
        static Stack<Vector2> vector2Stack = new Stack<Vector2>();
        static Stack<RGB> rgbStack = new Stack<RGB>();
        static Stack<string> stringStack = new Stack<string>();
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
        public static void Execute(VMInstruction[] instructions)
        {
            if (instructions == null)
                PushBool(true);

            //TODO
        }
        public static void PushBool(bool value)
        {
            boolStack.Push(value);
        }
        public static bool PopBool()
        {
            return boolStack.Pop();
        }
        public static void PushInt(int value)
        {
            intStack.Push(value);
        }
        public static int PopInt()
        {
            return intStack.Pop();
        }
        public static void PushFloat(float value)
        {
            floatStack.Push(value);
        }
        public static float PopFloat()
        {
            return floatStack.Pop();
        }
        public static void PushEnum(int value)
        {
            enumStack.Push(value);
        }
        public static int PopEnum()
        {
            return enumStack.Pop();
        }
        public static void PushVector2(Vector2 value)
        {
            vector2Stack.Push(value);
        }
        public static Vector2 PopVector2()
        {
            return vector2Stack.Pop();
        }
        public static void PushRGB(RGB value)
        {
            rgbStack.Push(value);
        }
        public static RGB PopRGB()
        {
            return rgbStack.Pop();
        }
        public static void PushString(string value)
        {
            stringStack.Push(value);
        }
        public static string PopString()
        {
            return stringStack.Pop();
        }
    }
}
