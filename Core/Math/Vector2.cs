/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public struct Vector2
    {
        public float x, y;
        public static readonly Vector2 Zero = new Vector2();
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                var comparer = (Vector2)obj;
                return x == comparer.x && y == comparer.y;
            }
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
        public static bool TryParse(string value, out Vector2 result)
        {
            result = Vector2.Zero;
            value = value.Trim();
            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                value = value.Substring(1, value.Length - 2);
                try
                {
                    var part = value.Split(',');
                    if (part.Length != 2)
                        return false;
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
