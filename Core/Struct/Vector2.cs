/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CrazyStorm.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector2 : IEquatable<Vector2>
    {
        public float x, y;
        public static readonly Vector2 Zero = new Vector2();
        public static readonly Vector2 One = new Vector2(1, 1);
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public void Normalize()
        {
            float num = 1f / (float)Math.Sqrt(x * x + y * y);
            x *= num;
            y *= num;
        }
        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }
        public float LengthSquared()
        {
            return x * x + y * y;
        }
        public override string ToString()
        {
            return $"[{x},{y}]";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                return Equals((Vector2)obj);
            }
            return false;
        }
        public bool Equals(Vector2 other)
        {
            if (x == other.x) return y == other.y;
            return false;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();
        }
        //==
        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return lhs.Equals(rhs);
        }
        //!=
        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return !(lhs == rhs);
        }
        //+
        public static Vector2 Add(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x + rhs.x, lhs.y + rhs.y);
        }
        public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
        {
            return Add(lhs, rhs);
        }
        public static Vector2 Add(Vector2 lhs, float rhs)
        {
            return new Vector2(lhs.x + rhs, lhs.y + rhs);
        }
        public static Vector2 operator +(Vector2 lhs, float rhs)
        {
            return Add(lhs, rhs);
        }
        //-
        public static Vector2 Negate(Vector2 value)
        {
            return new Vector2(-value.x, -value.y);
        }
        public static Vector2 operator -(Vector2 value)
        {
            return Negate(value);
        }
        public static Vector2 Subtract(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x - rhs.x, lhs.y - rhs.y);
        }
        public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
        {
            return Subtract(lhs, rhs);
        }
        public static Vector2 Subtract(Vector2 lhs, float rhs)
        {
            return new Vector2(lhs.x - rhs, lhs.y - rhs);
        }
        public static Vector2 operator -(Vector2 lhs, float rhs)
        {
            return Subtract(lhs, rhs);
        }
        //*
        public static Vector2 Multiply(Vector2 lhs, float rhs)
        {
            return new Vector2(lhs.x * rhs, lhs.y * rhs);
        }
        public static Vector2 operator *(Vector2 lhs, float rhs)
        {
            return Multiply(lhs, rhs);
        }
        public static float Dot(Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }
        public static Vector2 Scale(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x * rhs.x, lhs.y * rhs.y);
        }
        //
        public static Vector2 Divide(Vector2 lhs, float rhs)
        {
            return new Vector2(lhs.x / rhs, lhs.y / rhs);
        }
        public static Vector2 operator /(Vector2 lhs, float rhs)
        {
            return Divide(lhs, rhs);
        }
        public static Vector2 Normalize(Vector2 value)
        {
            float num = 1f / (float)Math.Sqrt(value.x * value.x + value.y * value.y);
            value.x *= num;
            value.y *= num;
            return value;
        }
        public static bool TryParse(string value, out Vector2 result)
        {
            result = Vector2.Zero;
            value = value.Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                value = value.Substring(1, value.Length - 2);
                try
                {
                    var part = value.Split(',');
                    if (part.Length != 2) return false;
                    part[0] = part[0].Trim();
                    part[1] = part[1].Trim();
                    result.x = float.Parse(part[0]);
                    result.y = float.Parse(part[1]);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
