/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm.Core;
using System.Runtime.InteropServices;

namespace CrazyStorm.Core
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
    [StructLayout(LayoutKind.Explicit)]
    public struct VMInstruction
    {
        [FieldOffset(0)]
        public VMCode code;
        [FieldOffset(1)]
        public int intOperand;
        [FieldOffset(1)]
        public float floatOperand;
        [FieldOffset(1)]
        public bool boolOperand;
        [FieldOffset(8)]
        public string stringOperand;
    }
    public class VM
    {
        static Random random = new Random();
        static Stack<float> floatStack = new Stack<float>();
        static Stack<int> enumStack = new Stack<int>();
        static Stack<Vector2> vector2Stack = new Stack<Vector2>();
        static Stack<RGB> rgbStack = new Stack<RGB>();
        static Stack<string> stringStack = new Stack<string>();
        public static void SetRandomSeed(int seed)
        {
            random = new Random(seed);
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
                    case VMCode.CALL:
                        string func = PlayDataHelper.ReadString(bytes, position);
                        position += func.Length + 1;
                        list.Add(new VMInstruction { code = code, stringOperand = func });
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
        public static void Execute(PropertyContainer propertyContainer, VMInstruction[] instructions)
        {
            for (int i = 0; i < instructions.Length; ++i)
            {
                switch (instructions[i].code)
                {
                    case VMCode.NUMBER:
                        VM.PushFloat(instructions[i].floatOperand);
                        break;
                    case VMCode.BOOL:
                        VM.PushBool(instructions[i].boolOperand);
                        break;
                    case VMCode.NAME:
                        propertyContainer.PushProperty(instructions[i].stringOperand);
                        break;
                    case VMCode.VECTOR2:
                        float y = VM.PopFloat();
                        float x = VM.PopFloat();
                        VM.PushVector2(new Vector2(x, y));
                        break;
                    case VMCode.RGB:
                        float b = VM.PopFloat();
                        float g = VM.PopFloat();
                        float r = VM.PopFloat();
                        VM.PushRGB(new RGB(r, g, b));
                        break;
                    case VMCode.ARGUMENTS:
                        VM.PushFloat(instructions[i].intOperand);
                        break;
                    case VMCode.CALL:
                        float count = VM.PopFloat();
                        switch (instructions[i].stringOperand)
                        {
                            case "dist":
                                Vector2 v = VM.PopVector2() - VM.PopVector2();
                                VM.PushFloat((float)Math.Sqrt(v.x * v.x + v.y * v.y));
                                break;
                            case "angle":
                                v = VM.PopVector2() - VM.PopVector2();
                                VM.PushFloat(MathHelper.GetDegree(v));
                                break;
                            case "rand":
                                float ratio = (float)random.NextDouble();
                                VM.PushFloat((1 - ratio) * VM.PopFloat() + ratio * VM.PopFloat());
                                break;
                            case "randi":
                                int i2 = VM.PopInt();
                                int i1 = VM.PopInt();
                                if (i1 > i2)
                                {
                                    int temp = i1;
                                    i1 = i2;
                                    i2 = temp;
                                }
                                VM.PushInt(random.Next(i1, i2));
                                break;
                            case "sin":
                                VM.PushFloat((float)Math.Sin(MathHelper.DegToRad(VM.PopFloat())));
                                break;
                            case "cos":
                                VM.PushFloat((float)Math.Cos(MathHelper.DegToRad(VM.PopFloat())));
                                break;
                            case "tan":
                                VM.PushFloat((float)Math.Tan(MathHelper.DegToRad(VM.PopFloat())));
                                break;
                            case "pi":
                                VM.PushFloat((float)Math.PI);
                                break;
                            case "e":
                                VM.PushFloat((float)Math.E);
                                break;
                            case "asin":
                                VM.PushFloat((float)MathHelper.RadToDeg(Math.Asin(VM.PopFloat())));
                                break;
                            case "acos":
                                VM.PushFloat((float)MathHelper.RadToDeg(Math.Acos(VM.PopFloat())));
                                break;
                            case "atan":
                                VM.PushFloat((float)MathHelper.RadToDeg(Math.Atan(VM.PopFloat())));
                                break;
                            case "exp":
                                VM.PushFloat((float)Math.Exp(VM.PopFloat()));
                                break;
                            case "log":
                                VM.PushFloat((float)Math.Log(VM.PopFloat(), VM.PopFloat()));
                                break;
                            case "pow":
                                float power = VM.PopFloat();
                                float value = VM.PopFloat();
                                VM.PushFloat((float)Math.Pow(value, power));
                                break;
                            case "sqrt":
                                VM.PushFloat((float)Math.Sqrt(VM.PopFloat()));
                                break;
                        }
                        break;
                    case VMCode.AND:
                        VM.PushBool(VM.PopBool() & VM.PopBool());
                        break;
                    case VMCode.OR:
                        VM.PushBool(VM.PopBool() | VM.PopBool());
                        break;
                    case VMCode.EQUAL:
                        VM.PushBool(VM.PopFloat() == VM.PopFloat());
                        break;
                    case VMCode.ADD:
                        VM.PushFloat(VM.PopFloat() + VM.PopFloat());
                        break;
                    case VMCode.SUB:
                        float subtrahend = VM.PopFloat();
                        float minuend = VM.PopFloat();
                        VM.PushFloat(minuend - subtrahend);
                        break;
                    case VMCode.MUL:
                        VM.PushFloat(VM.PopFloat() * VM.PopFloat());
                        break;
                    case VMCode.DIV:
                        float divisor = VM.PopFloat();
                        float dividend = VM.PopFloat();
                        VM.PushFloat(dividend / divisor);
                        break;
                    case VMCode.MOD:
                        divisor = VM.PopFloat();
                        float number = VM.PopFloat();
                        VM.PushFloat(number % divisor);
                        break;
                    case VMCode.MORE:
                        float right = VM.PopFloat();
                        float left = VM.PopFloat();
                        VM.PushBool(left > right);
                        break;
                    case VMCode.LESS:
                        right = VM.PopFloat();
                        left = VM.PopFloat();
                        VM.PushBool(left < right);
                        break;
                }
            }
        }
        public static void PushBool(bool value)
        {
            floatStack.Push(value ? 1 : 0);
        }
        public static bool PopBool()
        {
            return floatStack.Pop() == 1;
        }
        public static void PushInt(int value)
        {
            floatStack.Push(value);
        }
        public static int PopInt()
        {
            return (int)floatStack.Pop();
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
