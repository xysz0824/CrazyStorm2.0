using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public static Vector2 GetRelative(Vector2 absolute, Vector2 center, float deg)
        {
            var relativeX = absolute.x - center.x;
            var relativeY = absolute.y - center.y;
            var rad = deg / 180 * Math.PI;
            var cos = (float)Math.Cos(rad);
            var sin = (float)Math.Sin(rad);
            var relative = new Vector2();
            relative.x = cos * relativeX + sin * relativeY;
            relative.y = cos * relativeY - sin * relativeX;
            return relative;
        }
        public static Vector2 GetAbsolute(Vector2 relative, Vector2 center, float deg)
        {
            var rad = deg / 180 * Math.PI;
            var cos = (float)Math.Cos(rad);
            var sin = (float)Math.Sin(rad);
            var absolute = new Vector2();
            absolute.x = cos * relative.x - sin * relative.y;
            absolute.y = cos * relative.y + sin * relative.x;
            absolute += center;
            return absolute;
        }
        public static Vector2 Intersect(Vector2 p0, Vector2 d0, Vector2 p1, Vector2 d1)
        {
            Vector2 e = p1 - p0;
            float kross = d0.x * d1.y - d0.y * d1.x;
            float sqrtKross = kross * kross;
            float sqrLen0 = d0.LengthSquared();
            float sqrLen1 = d1.LengthSquared();
            if (sqrtKross > 0.0001f * sqrLen0 * sqrLen1)
            {
                float s = (e.x * d1.y - e.y * d1.x) / kross;
                if (s >= 0 && s <= 1) return p0 + d0 * s;
            }
            return new Vector2(float.MinValue, float.MaxValue);
        }
        public static bool RectContains(Vector2 rectStart, Vector2 rectSize, Vector2 checkPoint)
        {
            if (rectStart.x <= checkPoint.x && checkPoint.x < rectStart.x + rectSize.x && rectStart.y <= checkPoint.y)
            {
                return checkPoint.y < rectStart.y + rectSize.y;
            }
            return false;
        }
        public static bool VolumeJudge(bool bodyDead, Vector2 bodyPosLast, Vector2 bodyPos, Vector2 posLast, Vector2 pos, float deg, 
            Vector2 origin, Vector2 scale, Vector2 volumeStart, Vector2 volumeSize, float percent, out Vector2 newBodyPos)
        {
            newBodyPos = bodyPos;
            var relativepp = GetRelative(bodyPos, pos, deg);
            var relativebpp = GetRelative(bodyPosLast, pos, deg);
            var left = scale.x > 0 ? (int)((volumeStart.x - origin.x) * scale.x) : (int)((volumeStart.x + volumeSize.x - origin.x) * scale.x);
            var top = scale.y > 0 ? (int)((volumeStart.y - origin.y) * scale.y) : (int)((volumeStart.y + volumeSize.y - origin.y) * scale.y);
            var boxStart = new Vector2(left, top);
            var boxSize = new Vector2((int)(volumeSize.x * Math.Abs(scale.x)), (int)(volumeSize.y * Math.Abs(scale.y)));
            var boxCenter = boxStart + boxSize / 2;
            if (RectContains(boxStart, boxSize, relativepp))
            {
                var f = percent / 100f;
                left = (int)((boxStart.x - boxCenter.x) * f + boxCenter.x);
                top = (int)((boxStart.y - boxCenter.y) * f + boxCenter.y);
                var judgeBoxStart = new Vector2(left, top);
                var judgeBoxSize = new Vector2((int)(boxSize.x * f), (int)(boxSize.y * f));
                if (RectContains(judgeBoxStart, judgeBoxSize, relativepp)) return true;
                if (bodyDead && RectContains(boxStart, boxSize, relativebpp)) return true;
                newBodyPos = bodyPosLast;
                if (boxSize.x != 0f && boxSize.y != 0f)
                {
                    var relativepspeed = -GetRelative(posLast, pos, deg);
                    var relativeppspeed = relativepp - relativebpp;
                    var relativeVector = relativepspeed + relativeppspeed;
                    if (relativeVector.Length() <= 10) relativeVector = Vector2.Normalize(relativeVector) * 10;
                    var p0 = relativebpp;
                    var d0 = relativeVector;
                    var result = Intersect(p0, d0, new Vector2(boxStart.x - 4, boxStart.y), new Vector2(boxSize.x + 4, 0));
                    if (result.x != float.MinValue && result.y != float.MaxValue)
                    {
                        newBodyPos = GetAbsolute(result + new Vector2(0, -1) * relativepspeed.Length(), pos, deg);
                    }
                    result = Intersect(p0, d0, new Vector2(boxStart.x - 4, boxStart.y + boxSize.y), new Vector2(boxSize.x + 4, 0));
                    if (result.x != float.MinValue && result.y != float.MaxValue)
                    {
                        newBodyPos = GetAbsolute(result + new Vector2(0, 1) * relativepspeed.Length(), pos, deg);
                    }
                    result = Intersect(p0, d0, new Vector2(boxStart.x, boxStart.y - 4), new Vector2(0, boxSize.y + 4));
                    if (result.x != float.MinValue && result.y != float.MaxValue)

                    {
                        newBodyPos = GetAbsolute(result + new Vector2(-1, 0) * relativepspeed.Length(), pos, deg);
                    }
                    result = Intersect(p0, d0, new Vector2(boxStart.x + boxSize.x, boxStart.y - 4), new Vector2(0, boxSize.y + 4));
                    if (result.x != float.MinValue && result.y != float.MaxValue)
                    {
                        newBodyPos = GetAbsolute(result + new Vector2(1, 0) * relativepspeed.Length(), pos, deg);
                    }
                }
            }
            return false;
        }
    }
}
