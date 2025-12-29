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
        static Stack<Vector3> vectorStack = new Stack<Vector3>();
        static Stack<bool> boolStack = new Stack<bool>();
        static Stack<string> stringStack = new Stack<string>();
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
                    case VMCode.BOOL:
                        bytes.AddRange(BitConverter.GetBytes((bool)operand));
                        break;
                    case VMCode.NAME:
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
                    case VMCode.BOOL:
                        bool b = BitConverter.ToBoolean(bytes, position);
                        position += sizeof(bool);
                        list.Add(new VMInstruction { code = code, boolOperand = b });
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
                    case VMCode.VECTOR:
                        VM.PushVector(instructions[i].vectorOperand);
                        break;
                    case VMCode.NAME:
                        propertyContainer.PushProperty(instructions[i].stringOperand);
                        break;
                    case VMCode.CALL:
                        float count = VM.PopInt();
                        switch (instructions[i].stringOperand)
                        {
                            case "abs":
                                var v = VM.PopVector();
                                VM.PushVector(new Vector3(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z)));
                                break;
                            case "dist":
                                var v2 = VM.PopVector2() - VM.PopVector2();
                                VM.PushFloat((float)Math.Sqrt(v2.x * v2.x + v2.y * v2.y));
                                break;
                            case "angle":
                                v2 = VM.PopVector2() - VM.PopVector2();
                                VM.PushFloat(MathHelper.GetDegree(new Vector2(v2.x, v2.y)));
                                break;
                            case "rand":
                                float ratio = (float)random.NextDouble();
                                VM.PushVector(VM.PopVector() * (1 - ratio) + VM.PopVector() * ratio);
                                break;
                            case "randi":
                                int i2 = (int)VM.PopInt();
                                int i1 = (int)VM.PopInt();
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
                                var newBase = VM.PopFloat();
                                var a = VM.PopFloat();
                                VM.PushFloat((float)Math.Log(a, newBase));
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
                        VM.PushBool(VM.PopVector() == VM.PopVector());
                        break;
                    case VMCode.ADD:
                        VM.PushVector(VM.PopVector() + VM.PopVector());
                        break;
                    case VMCode.SUB:
                        var subtrahend = VM.PopVector();
                        var minuend = VM.PopVector();
                        VM.PushVector(minuend - subtrahend);
                        break;
                    case VMCode.MUL:
                        var vA = VM.PopVector();
                        var vB = VM.PopVector();
                        VM.PushVector(new Vector3(vA.x * vB.x, vA.y * vB.y, vA.z * vB.z));
                        break;
                    case VMCode.DIV:
                        var divisor = VM.PopVector();
                        var dividend = VM.PopVector();
                        VM.PushVector(new Vector3(dividend.x / divisor.x, dividend.y / divisor.y, dividend.z / divisor.z));
                        break;
                    case VMCode.MOD:
                        divisor = VM.PopVector();
                        var number = VM.PopVector();
                        VM.PushVector(new Vector3(number.x % divisor.x, number.y % divisor.y, number.z % divisor.z));
                        break;
                    case VMCode.MORE:
                        var right = VM.PopFloat();
                        var left = VM.PopFloat();
                        VM.PushBool(left > right);
                        break;
                    case VMCode.LESS:
                        right = VM.PopFloat();
                        left = VM.PopFloat();
                        VM.PushBool(left < right);
                        break;
                    case VMCode.MOREOREQUAL:
                        right = VM.PopFloat();
                        left = VM.PopFloat();
                        VM.PushBool(left >= right);
                        break;
                    case VMCode.LESSOREQUAL:
                        right = VM.PopFloat();
                        left = VM.PopFloat();
                        VM.PushBool(left <= right);
                        break;
                    case VMCode.NOTEQUAL:
                        VM.PushBool(VM.PopVector() != VM.PopVector());
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
                    case VMCode.RAND:
                        right = VM.PopFloat();
                        left = VM.PopFloat();
                        float t = (float)random.NextDouble();
                        VM.PushFloat(left * (1 - t) + right * t);
                        break;
                }
            }
        }
        public static void PushVector(Vector3 value)
        {
            if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z))
            {
                throw new NotFiniteNumberException();
            }
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
            if (float.IsNaN(value.x) || float.IsNaN(value.y))
            {
                throw new NotFiniteNumberException();
            }
            vectorStack.Push(new Vector3(value));
        }
        public static Vector2 PopVector2()
        {
            var v = vectorStack.Pop();
            return new Vector2(v.x, v.y);
        }
        public static void PushFloat(float value)
        {
            if (float.IsNaN(value))
            {
                throw new NotFiniteNumberException();
            }
            vectorStack.Push(new Vector3(value));
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
