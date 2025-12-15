using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class MathHelper
    {
        public const float Pi = 3.14159274F;
        public const float PiOver2 = 1.57079637F;
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
        public static Vector2 GetVector2(float size, float angle)
        {
            Vector2 v = new Vector2();
            SetVector2(ref v, size, angle);
            return v;
        }
        public static float GetDegree(Vector2 speedVector)
        {
            double vf = 0;
            if (speedVector.y != 0)
            {
                vf = Math.PI / 2 - Math.Atan(speedVector.x / speedVector.y);
                if (speedVector.y < 0)
                    vf += Math.PI;
            }
            else
            {
                vf = speedVector.x >= 0 ? 0 : Math.PI;
            }
            return (float)RadToDeg(vf);
        }
        public static bool LineIntersectWithCircle(Vector2 p1, Vector2 p2, Vector2 center, float radius)
        {
            return PointToSegDist(p1, p2, center) <= radius;
        }
        public static double PointToSegDist(Vector2 p1, Vector2 p2, Vector2 center)
        {
            Vector2 p1c = p1 - center;
            Vector2 p12 = p1 - p2;
            double dot = Vector2.Dot(p1c, p12);
            if (dot <= 0) return Math.Sqrt((center.x - p1.x) * (center.x - p1.x) + (center.y - p1.y) * (center.y - p1.y));

            double d2 = (p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y);
            if (dot >= d2) return Math.Sqrt((center.x - p2.x) * (center.x - p2.x) + (center.y - p2.y) * (center.y - p2.y));

            double r = dot / d2;
            double px = p1.x + (p2.x - p1.x) * r;
            double py = p1.y + (p2.y - p1.y) * r;
            return Math.Sqrt((center.x - px) * (center.x - px) + (center.y - py) * (center.y - py));
        }
        public static bool TwoCirclesIntersect(Vector2 c1, float r1, Vector2 c2, float r2)
        {
            Vector2 v = c1 - c2;
            double dist = Math.Sqrt(v.x * v.x + v.y * v.y);
            return dist < r1 + r2 && dist > Math.Abs(r1 - r2);
        }
        public static bool PointInsideCircle(Vector2 center, float radius, Vector2 point)
        {
            Vector2 v = center - point;
            double dist = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
            return dist <= radius;
        }
        public static Vector2 Rotate(Vector2 v, float deg)
        {
            var rad = deg / 180 * Math.PI;
            var cos = (float)Math.Cos(rad);
            var sin = (float)Math.Sin(rad);
            return new Vector2(cos * v.x - sin * v.y, cos * v.y + sin * v.x);
        }
        public static bool Judge(Vector2 posLast, Vector2 pos, Vector2 bodyPosLast, Vector2 bodyPos, Vector2 scale, float r, float deg)
        {
            if (r <= 0) return false;
            r++;
            bodyPosLast.x = pos.x + bodyPosLast.x - posLast.x;
            bodyPosLast.y = pos.y + bodyPosLast.y - posLast.y;
            float dx = (bodyPos.x - bodyPosLast.x);
            float dy = (bodyPos.y - bodyPosLast.y);
            float jx, jy;
            if (dx != 0)
            {
                float dk = dy / dx;
                if (dk != 0)
                {
                    jx = (pos.y - bodyPosLast.y + (1f / dk) * pos.x + dk * bodyPosLast.x) / (dk + 1f / dk);
                    jy = bodyPosLast.y + dk * (jx - bodyPosLast.x);
                }
                else
                {
                    jx = pos.x; jy = bodyPos.y;
                }
                if (Math.Abs(Math.Abs(bodyPos.x - jx) + Math.Abs(bodyPosLast.x - jx) - Math.Abs(bodyPos.x - bodyPosLast.x)) > 0)
                {
                    jx = bodyPos.x; jy = bodyPos.y;
                }
            }
            else if (dy != 0)
            {
                jx = bodyPos.x; jy = pos.y;
                if (Math.Abs(Math.Abs(bodyPos.y - jy) + Math.Abs(bodyPosLast.y - jy) - Math.Abs(bodyPos.y - bodyPosLast.y)) > 0)
                {
                    jx = bodyPos.x; jy = bodyPos.y;
                }
            }
            else
            {
                jx = bodyPos.x; jy = bodyPos.y;
            }
            var rad = (float)MathHelper.DegToRad(deg);
            double vec;
            if (jx - pos.x != 0)
            {
                vec = Math.Atan((jy - pos.y) / (jx - pos.x));
                if (jx - pos.x < 0) vec += MathHelper.Pi;
            }
            else
            {
                if (jy - pos.y > 0) vec = MathHelper.PiOver2;
                else vec = -MathHelper.PiOver2;
            }
            float d = (float)Math.Sqrt((pos.x - jx) * (pos.x - jx) + (pos.y - jy) * (pos.y - jy));
            jx = pos.x + d * (float)Math.Cos(vec - rad);
            jy = pos.y + d * (float)Math.Sin(vec - rad);
            pos.x = (pos.x - jx) * (pos.x - jx);
            pos.y = (pos.y - jy) * (pos.y - jy);
            float w = (r * scale.x) * (r * scale.x);
            float h = (r * scale.y) * (r * scale.y);
            if (pos.x / h + pos.y / w <= 1) return true;
            else return false;
        }
    }
}
