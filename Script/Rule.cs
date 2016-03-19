using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Script
{
    public class Rule
    {
        public static bool IsMatchWith(System.Type typeA, System.Type typeB)
        {
            if (typeA.Equals(typeB))
                return true;

            System.Type intType = typeof(int);
            System.Type floatType = typeof(float);
            if ((typeA.Equals(intType) && typeB.Equals(floatType)) ||
                (typeA.Equals(floatType) && typeB.Equals(intType)))
                return true;

            return false;
        }
    }
}
