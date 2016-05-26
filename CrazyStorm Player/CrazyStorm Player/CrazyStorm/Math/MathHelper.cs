using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class MathHelper
    {
        public static double DegToRad(double degree)
        {
            return degree / 180.0 * Math.PI;
        }
        public static double RadToDeg(double radian)
        {
            return radian / Math.PI * 180.0;
        }
        public static void SetVector2(ref Vector2 v, float size, float angle)
        {
            v.x = (float)(size * Math.Cos(MathHelper.DegToRad(angle)));
            v.y = (float)(size * Math.Sin(MathHelper.DegToRad(angle)));
        }
    }
}
