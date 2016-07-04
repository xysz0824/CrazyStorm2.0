/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
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
            return (float)MathHelper.RadToDeg(vf);
        }
        public static bool LineIntersectWithCircle(Vector2 p1, Vector2 p2, Vector2 center, float radius)
        {
            return PointToSegDist(p1, p2, center) <= radius;
        }
        public static double PointToSegDist(Vector2 p1, Vector2 p2, Vector2 center)
        {
            Vector2 p1c = p1 - center;
            Vector2 p12 = p1 - p2;
            double dot = Vector2.Dot(p1c,p12);
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
    }
}
