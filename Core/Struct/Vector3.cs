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
    public struct Vector3 : IEquatable<Vector3>
    {
        public float x, y, z;
        public bool asInteger;
        public bool useFrameEqual;
        public static readonly Vector3 Zero = new Vector3();
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            asInteger = false;
            useFrameEqual = false;
        }
        public Vector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            asInteger = true;
            useFrameEqual = false;
        }
        public Vector3(float x, float y) : this(x, y, 0) { }
        public Vector3(int x, int y) : this(x, y, 0) { }
        public Vector3(float x) : this(x, x, x) { }
        public Vector3(int x) : this(x, x, x) { }
        public Vector3(Vector2 xy, float z)
        {
            this.x = xy.x;
            this.y = xy.y;
            this.z = z;
            asInteger = false;
            useFrameEqual = false;
        }
        public Vector3(Vector2 xy) : this(xy, 0) { }
        public override string ToString()
        {
            return $"[{x},{y},{z}]";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                return Equals((Vector3)obj);
            }
            return false;
        }
        public bool Equals(Vector3 other)
        {
            if (x == other.x && y == other.y) return z == other.z;
            return false;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }
        //==
        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return lhs.Equals(rhs);
        }
        //!=
        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }
        //+
        public static Vector3 Add(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }
        public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
        {
            return Add(lhs, rhs);
        }
        public static Vector3 Add(Vector3 lhs, float rhs)
        {
            return new Vector3(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs);
        }
        public static Vector3 operator +(Vector3 lhs, float rhs)
        {
            return Add(lhs, rhs);
        }
        //-
        public static Vector3 Negate(Vector3 value)
        {
            return new Vector3(-value.x, -value.y, -value.z);
        }
        public static Vector3 operator -(Vector3 value)
        {
            return Negate(value);
        }
        public static Vector3 Subtract(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }
        public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
        {
            return Subtract(lhs, rhs);
        }
        public static Vector3 Subtract(Vector3 lhs, float rhs)
        {
            return new Vector3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
        }
        public static Vector3 operator -(Vector3 lhs, float rhs)
        {
            return Subtract(lhs, rhs);
        }
        //*
        public static Vector3 Multiply(Vector3 lhs, float rhs)
        {
            return new Vector3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
        }
        public static Vector3 operator *(Vector3 lhs, float rhs)
        {
            return Multiply(lhs, rhs);
        }
        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }
        //
        public static Vector3 Divide(Vector3 lhs, float rhs)
        {
            return new Vector3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        }
        public static Vector3 operator /(Vector3 lhs, float rhs)
        {
            return Divide(lhs, rhs);
        }
        public static bool TryParse(string value, out Vector3 result)
        {
            result = Vector3.Zero;
            value = value.Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                value = value.Substring(1, value.Length - 2);
                try
                {
                    var part = value.Split(',');
                    if (part.Length != 3) return false;
                    part[0] = part[0].Trim();
                    part[1] = part[1].Trim();
                    part[2] = part[2].Trim();
                    result.x = float.Parse(part[0]);
                    result.y = float.Parse(part[1]);
                    result.z = float.Parse(part[2]);
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
