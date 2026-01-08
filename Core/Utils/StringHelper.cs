using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public class StringUtil
    {
        public static int StableHash32_Fnv1a(string s)
        {
            unchecked
            {
                const uint fnvOffset = 2166136261;
                const uint fnvPrime = 16777619;

                uint hash = fnvOffset;
                var bytes = Encoding.UTF8.GetBytes(s);

                for (int i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= fnvPrime;
                }

                return (int)hash;
            }
        }
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
