/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public struct RGB
    {
        public float r, g, b;
        public static readonly RGB Zero = new RGB();
        public RGB(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
        public override string ToString()
        {
            return "[" + r + "," + g + "," + b + "]";
        }
        public override bool Equals(object obj)
        {
            if (obj is RGB)
            {
                var comparer = (RGB)obj;
                return r == comparer.r && g == comparer.g && b == comparer.b;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return r.GetHashCode() + g.GetHashCode() + b.GetHashCode();
        }
        //==
        public static bool operator ==(RGB lhs, RGB rhs)
        {
            return lhs.Equals(rhs);
        }
        //!=
        public static bool operator !=(RGB lhs, RGB rhs)
        {
            return !(lhs == rhs);
        }
        //+
        public static RGB Add(RGB lhs, RGB rhs)
        {
            return new RGB(lhs.r + rhs.r, lhs.g + rhs.g, lhs.b + rhs.b);
        }
        public static RGB operator +(RGB lhs, RGB rhs)
        {
            return Add(lhs, rhs);
        }
        public static RGB Add(RGB lhs, float rhs)
        {
            return new RGB(lhs.r + rhs, lhs.g + rhs, lhs.b + rhs);
        }
        public static RGB operator +(RGB lhs, float rhs)
        {
            return Add(lhs, rhs);
        }
        //-
        public static RGB Negate(RGB value)
        {
            return new RGB(-value.r, -value.g, -value.b);
        }
        public static RGB operator -(RGB value)
        {
            return Negate(value);
        }
        public static RGB Subtract(RGB lhs, RGB rhs)
        {
            return new RGB(lhs.r - rhs.r, lhs.g - rhs.g, lhs.b - rhs.b);
        }
        public static RGB operator -(RGB lhs, RGB rhs)
        {
            return Subtract(lhs, rhs);
        }
        public static RGB Subtract(RGB lhs, float rhs)
        {
            return new RGB(lhs.r - rhs, lhs.g - rhs, lhs.b - rhs);
        }
        public static RGB operator -(RGB lhs, float rhs)
        {
            return Subtract(lhs, rhs);
        }
        //*
        public static RGB Multiply(RGB lhs, float rhs)
        {
            return new RGB(lhs.r * rhs, lhs.g * rhs, lhs.b * rhs);
        }
        public static RGB operator *(RGB lhs, float rhs)
        {
            return Multiply(lhs, rhs);
        }
        public static bool TryParse(string value, out RGB result)
        {
            result = RGB.Zero;
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
                    result.r = float.Parse(splits[0]);
                    result.g = float.Parse(splits[1]);
                    result.b = float.Parse(splits[2]);
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
