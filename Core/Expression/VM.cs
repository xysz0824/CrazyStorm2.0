/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm.Core;
using System.Runtime.InteropServices;
using System.Data;

namespace CrazyStorm.Core
{
    public enum VMCode : byte
    {
        VECTOR,
        BOOL,
        NAME,
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
        LESS,
        MOREOREQUAL,
        LESSOREQUAL,
        NOTEQUAL,
        VECTOR2,
        RGB,
        RAND,
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct VMInstruction
    {
        [FieldOffset(0)]
        public VMCode code;
        [FieldOffset(1)]
        public Vector3 vectorOperand;
        [FieldOffset(1)]
        public bool boolOperand;
        [FieldOffset(16)]
        public string stringOperand;
    }
    public class VM
    {
        static Random random = new Random();
        static Stack<Vector3> vectorStack = new Stack<Vector3>(16);
        static Stack<bool> boolStack = new Stack<bool>(16);
        static Stack<string> stringStack = new Stack<string>(16);
        public static void Clear()
        {
            vectorStack.Clear();
            boolStack.Clear();
            stringStack.Clear();
        }
        public static byte[] CreateInstruction(VMCode code, object operand)
        {
            var bytes = new List<byte>();
            bytes.Add((byte)code);
            if (operand != null)
            {
                switch (code)
                {
                    case VMCode.VECTOR:
                        bytes.AddRange(PlayDataHelper.GetStructBytes((Vector3)operand));
                        break;
                    case VMCode.NAME:
                        var isStr = operand is string;
                        bytes.Add(isStr ? (byte)1 : (byte)0);
                        if (isStr) bytes.AddRange(PlayDataHelper.GetStringBytes((string)operand));
                        else bytes.AddRange(BitConverter.GetBytes((int)operand));
                        break;
                    case VMCode.BOOL:
                        bytes.AddRange(BitConverter.GetBytes((bool)operand));
                        break;
                    case VMCode.CALL:
                        bytes.AddRange(PlayDataHelper.GetStringBytes((string)operand));
                        break;
                }
            }
            return bytes.ToArray();
        }
        public static byte[] CreateInstruction(VMCode code)
        {
            return CreateInstruction(code, null);
        }
        public static void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }
        public unsafe static VMInstruction[] Decode(byte[] bytes)
        {
            List<VMInstruction> list = new List<VMInstruction>();
            int position = 0;
            while (position != bytes.Length)
            {
                VMCode code = (VMCode)bytes[position++];
                switch (code)
                {
                    case VMCode.VECTOR:
                        var v = PlayDataHelper.ReadStructBytes<Vector3>(bytes, position);
                        position += sizeof(Vector3);
                        list.Add(new VMInstruction { code = code, vectorOperand = v });
                        break;
                    case VMCode.NAME:
                        byte isStr = bytes[position++];
                        if (isStr == 1)
                        {
                            string name = PlayDataHelper.ReadString(bytes, position);
                            position += name.Length + 1;
                            list.Add(new VMInstruction { code = code, stringOperand = name });
                        }
                        else
                        {
                            int i = BitConverter.ToInt32(bytes, position);
                            position += sizeof(int);
                            list.Add(new VMInstruction { code = code, vectorOperand = new Vector3(i) });
                        }
                        break;
                    case VMCode.BOOL:
                        bool b = BitConverter.ToBoolean(bytes, position);
                        position += sizeof(bool);
                        list.Add(new VMInstruction { code = code, boolOperand = b });
                        break;
                    case VMCode.CALL:
                        string func = PlayDataHelper.ReadString(bytes, position);
                        position += func.Length + 1;
                        list.Add(new VMInstruction { code = code, stringOperand = func });
                        break;
                    default:
                        list.Add(new VMInstruction { code = code });
                        break;
                }
            }
            return list.ToArray();
        }
        public static int GetOperandConsumption(VMInstruction instruction)
        {
            switch (instruction.code)
            {
                case VMCode.VECTOR: return -1;
                case VMCode.BOOL: return -1;
                case VMCode.NAME: return -1;
                case VMCode.CALL:
                    int consumption = 1;
                    switch (instruction.stringOperand)
                    {
                        case "abs": consumption += 1; break;
                        case "dist": consumption += 2; break;
                        case "angle": consumption += 2; break;
                        case "rand": consumption += 2; break;
                        case "randi": consumption += 2; break;
                        case "sin": consumption += 1; break;
                        case "cos": consumption += 1; break;
                        case "tan": consumption += 1; break;
                        case "asin": consumption += 1; break;
                        case "acos": consumption += 1; break;
                        case "atan": consumption += 1; break;
                        case "exp": consumption += 1; break;
                        case "log": consumption += 2; break;
                        case "pow": consumption += 2; break;
                        case "sqrt": consumption += 1; break;
                    }
                    return consumption;
                case VMCode.AND:
                case VMCode.OR:
                case VMCode.EQUAL:
                case VMCode.ADD:
                case VMCode.SUB:
                case VMCode.MUL:
                case VMCode.DIV:
                case VMCode.MOD:
                case VMCode.MORE:
                case VMCode.LESS:
                case VMCode.MOREOREQUAL:
                case VMCode.LESSOREQUAL:
                case VMCode.NOTEQUAL:
                case VMCode.VECTOR2:
                    return 2;
                case VMCode.RGB: return 3;
                case VMCode.RAND: return 2;
            }
            return 0;
        }
        public static void Execute(PropertyContainer propertyContainer, VMInstruction[] instructions, float frameScale)
        {
            Execute(propertyContainer, instructions, 0, instructions.Length - 1, frameScale);
        }
        public static void Execute(PropertyContainer propertyContainer, VMInstruction[] instructions, int startIndex, int endIndex, float frameScale)
        {
            for (int i = startIndex; i <= endIndex; ++i)
            {
                switch (instructions[i].code)
                {
                    case VMCode.VECTOR:
                        PushVector(instructions[i].vectorOperand);
                        break;
                    case VMCode.NAME:
                        if (instructions[i].stringOperand != null) PushString(instructions[i].stringOperand);
                        else propertyContainer.PushProperty((int)instructions[i].vectorOperand.x);
                        break;
                    case VMCode.CALL:
                        float count = PopInt();
                        switch (instructions[i].stringOperand)
                        {
                            case "abs":
                                var v = PopVector();
                                PushVector(new Vector3(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z)));
                                break;
                            case "dist":
                                var v2 = PopVector2() - PopVector2();
                                PushFloat((float)Math.Sqrt(v2.x * v2.x + v2.y * v2.y));
                                break;
                            case "angle":
                                v2 = PopVector2() - PopVector2();
                                PushFloat(MathHelper.GetDegree(new Vector2(v2.x, v2.y)));
                                break;
                            case "rand":
                                float ratio = (float)random.NextDouble();
                                PushVector(PopVector() * (1 - ratio) + PopVector() * ratio);
                                break;
                            case "randi":
                                int i2 = (int)PopInt();
                                int i1 = (int)PopInt();
                                if (i1 > i2)
                                {
                                    int temp = i1;
                                    i1 = i2;
                                    i2 = temp;
                                }
                                PushInt(random.Next(i1, i2));
                                break;
                            case "sin":
                                PushFloat((float)Math.Sin(MathHelper.DegToRad(PopFloat())));
                                break;
                            case "cos":
                                PushFloat((float)Math.Cos(MathHelper.DegToRad(PopFloat())));
                                break;
                            case "tan":
                                PushFloat((float)Math.Tan(MathHelper.DegToRad(PopFloat())));
                                break;
                            case "pi":
                                PushFloat((float)Math.PI);
                                break;
                            case "e":
                                PushFloat((float)Math.E);
                                break;
                            case "asin":
                                PushFloat((float)MathHelper.RadToDeg(Math.Asin(PopFloat())));
                                break;
                            case "acos":
                                PushFloat((float)MathHelper.RadToDeg(Math.Acos(PopFloat())));
                                break;
                            case "atan":
                                PushFloat((float)MathHelper.RadToDeg(Math.Atan(PopFloat())));
                                break;
                            case "exp":
                                PushFloat((float)Math.Exp(PopFloat()));
                                break;
                            case "log":
                                var newBase = PopFloat();
                                var a = PopFloat();
                                PushFloat((float)Math.Log(a, newBase));
                                break;
                            case "pow":
                                float power = PopFloat();
                                float value = PopFloat();
                                PushFloat((float)Math.Pow(value, power));
                                break;
                            case "sqrt":
                                PushFloat((float)Math.Sqrt(PopFloat()));
                                break;
                        }
                        break;
                    case VMCode.AND:
                        PushBool(PopBool() & PopBool());
                        break;
                    case VMCode.OR:
                        PushBool(PopBool() | PopBool());
                        break;
                    case VMCode.EQUAL:
                        var vR = PopVector();
                        var vL = PopVector();
                        PushBool(vL.useFrameEqual ? MathHelper.FrameEqual(vL.x, frameScale, (int)vR.x) : 
                            vL == vR);
                        break;
                    case VMCode.ADD:
                        PushVector(PopVector() + PopVector());
                        break;
                    case VMCode.SUB:
                        var subtrahend = PopVector();
                        var minuend = PopVector();
                        PushVector(minuend - subtrahend);
                        break;
                    case VMCode.MUL:
                        var vA = PopVector();
                        var vB = PopVector();
                        PushVector(new Vector3(vA.x * vB.x, vA.y * vB.y, vA.z * vB.z));
                        break;
                    case VMCode.DIV:
                        var divisor = PopVector();
                        var dividend = PopVector();
                        PushVector(new Vector3(
                            (dividend.asInteger && divisor.x == Math.Floor(divisor.x)) ? (int)dividend.x / (int)divisor.x : 
                            dividend.x / divisor.x,
                            (dividend.asInteger && divisor.y == Math.Floor(divisor.y)) ? (int)dividend.y / (int)divisor.y :
                            dividend.y / divisor.y,
                            (dividend.asInteger && divisor.z == Math.Floor(divisor.z)) ? (int)dividend.z / (int)divisor.z :
                            dividend.z / divisor.z));
                        break;
                    case VMCode.MOD:
                        divisor = PopVector();
                        var number = PopVector();
                        PushVector(new Vector3(
                            (number.asInteger && divisor.x == Math.Floor(divisor.x)) ? (int)number.x % (int)divisor.x :
                            number.x % divisor.x,
                            (number.asInteger && divisor.y == Math.Floor(divisor.y)) ? (int)number.y % (int)divisor.y :
                            number.y % divisor.y,
                            (number.asInteger && divisor.z == Math.Floor(divisor.z)) ? (int)number.z % (int)divisor.z :
                            number.z % divisor.z));
                        break;
                    case VMCode.MORE:
                        var right = PopFloat();
                        var left = PopFloat();
                        PushBool(left > right);
                        break;
                    case VMCode.LESS:
                        right = PopFloat();
                        left = PopFloat();
                        PushBool(left < right);
                        break;
                    case VMCode.MOREOREQUAL:
                        right = PopFloat();
                        left = PopFloat();
                        PushBool(left >= right);
                        break;
                    case VMCode.LESSOREQUAL:
                        right = PopFloat();
                        left = PopFloat();
                        PushBool(left <= right);
                        break;
                    case VMCode.NOTEQUAL:
                        PushBool(PopVector() != PopVector());
                        break;
                    case VMCode.VECTOR2:
                        float y = PopFloat();
                        float x = PopFloat();
                        PushVector2(new Vector2(x, y));
                        break;
                    case VMCode.RGB:
                        float b = PopFloat();
                        float g = PopFloat();
                        float r = PopFloat();
                        PushRGB(new RGB(r, g, b));
                        break;
                    case VMCode.RAND:
                        right = PopFloat();
                        left = PopFloat();
                        float t = (float)random.NextDouble();
                        PushFloat(left * (1 - t) + right * t);
                        break;
                }
            }
        }
        public static void PushVector(Vector3 value)
        {
            vectorStack.Push(value);
        }
        public static Vector3 PopVector()
        {
            return vectorStack.Pop();
        }
        public static void PushRGB(RGB value)
        {
            vectorStack.Push(new Vector3(value.r, value.g, value.b));
        }
        public static RGB PopRGB()
        {
            var v = vectorStack.Pop();
            return new RGB(v.x, v.y, v.z);
        }
        public static void PushVector2(Vector2 value)
        {
            vectorStack.Push(new Vector3(value));
        }
        public static Vector2 PopVector2()
        {
            var v = vectorStack.Pop();
            return new Vector2(v.x, v.y);
        }
        public static void PushFloat(float value, bool asInteger = false, bool useFrameEqual = false)
        {
            var v = new Vector3(value);
            v.asInteger = asInteger;
            v.useFrameEqual = useFrameEqual;
            vectorStack.Push(v);
        }
        public static float PopFloat()
        {
            return PopVector().x;
        }
        public static void PushInt(int value)
        {
            vectorStack.Push(new Vector3(value));
        }
        public static int PopInt()
        {
            return (int)PopVector().x;
        }
        public static void PushBool(bool value)
        {
            boolStack.Push(value);
        }
        public static bool PopBool()
        {
            return boolStack.Pop();
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
