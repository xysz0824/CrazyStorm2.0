/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public struct Vector4
    {
        public float x, y, z, w;
        public static readonly Vector4 Zero = new Vector4();
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public override string ToString()
        {
            return $"[{x},{y},{z},{w}]";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector4)
            {
                var comparer = (Vector4)obj;
                return x == comparer.x && y == comparer.y && z == comparer.z && w == comparer.w;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode() + w.GetHashCode();
        }
        //==
        public static bool operator ==(Vector4 lhs, Vector4 rhs)
        {
            return lhs.Equals(rhs);
        }
        //!=
        public static bool operator !=(Vector4 lhs, Vector4 rhs)
        {
            return !(lhs == rhs);
        }
        //+
        public static Vector4 Add(Vector4 lhs, Vector4 rhs)
        {
            return new Vector4(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
        }
        public static Vector4 operator +(Vector4 lhs, Vector4 rhs)
        {
            return Add(lhs, rhs);
        }
        public static Vector4 Add(Vector4 lhs, float rhs)
        {
            return new Vector4(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs, lhs.w + rhs);
        }
        public static Vector4 operator +(Vector4 lhs, float rhs)
        {
            return Add(lhs, rhs);
        }
        //-
        public static Vector4 Negate(Vector4 value)
        {
            return new Vector4(-value.x, -value.y, -value.z, -value.w);
        }
        public static Vector4 operator -(Vector4 value)
        {
            return Negate(value);
        }
        public static Vector4 Subtract(Vector4 lhs, Vector4 rhs)
        {
            return new Vector4(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z, lhs.w - rhs.w);
        }
        public static Vector4 operator -(Vector4 lhs, Vector4 rhs)
        {
            return Subtract(lhs, rhs);
        }
        public static Vector4 Subtract(Vector4 lhs, float rhs)
        {
            return new Vector4(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs, lhs.w - rhs);
        }
        public static Vector4 operator -(Vector4 lhs, float rhs)
        {
            return Subtract(lhs, rhs);
        }
        //*
        public static Vector4 Multiply(Vector4 lhs, float rhs)
        {
            return new Vector4(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
        }
        public static Vector4 operator *(Vector4 lhs, float rhs)
        {
            return Multiply(lhs, rhs);
        }
        public static float Dot(Vector4 lhs, Vector4 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z + lhs.w * rhs.w;
        }
        //
        public static Vector4 Divide(Vector4 lhs, float rhs)
        {
            return new Vector4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);
        }
        public static Vector4 operator /(Vector4 lhs, float rhs)
        {
            return Divide(lhs, rhs);
        }
        public static bool TryParse(string value, out Vector4 result)
        {
            result = Vector4.Zero;
            value = value.Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                value = value.Substring(1, value.Length - 2);
                try
                {
                    var part = value.Split(',');
                    if (part.Length != 4) return false;
                    part[0] = part[0].Trim();
                    part[1] = part[1].Trim();
                    part[2] = part[2].Trim();
                    part[3] = part[3].Trim();
                    result.x = float.Parse(part[0]);
                    result.y = float.Parse(part[1]);
                    result.z = float.Parse(part[2]);
                    result.w = float.Parse(part[3]);
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
