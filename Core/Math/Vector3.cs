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
    public struct Vector3
    {
        public float x, y, z;
        public static readonly Vector3 Zero = new Vector3();
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override string ToString()
        {
            return "[" + x + "," + y + "," + z + "]";
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                var comparer = (Vector3)obj;
                return x == comparer.x && y == comparer.y && z == comparer.z;
            }
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
        public static bool TryParse(string value, out Vector3 result)
        {
            result = Vector3.Zero;
            value = value.Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                value = value.Substring(1, value.Length - 2);
                try
                {
                    var splits = value.Split(',');
                    if (splits.Length != 3)
                        return false;
                    splits[0] = splits[0].Trim();
                    splits[1] = splits[1].Trim();
                    splits[2] = splits[2].Trim();
                    result.x = float.Parse(splits[0]);
                    result.y = float.Parse(splits[1]);
                    result.z = float.Parse(splits[2]);
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
