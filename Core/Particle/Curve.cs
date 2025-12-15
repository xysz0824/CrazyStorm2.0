/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyStorm.Core
{
    public struct CurveVertex
    {
        public Vector3 Pos;
        public Vector2 TexCoord;
    }
    public struct CurveInitData
    {
        public Vector2 pos;
        public int segment;
    }
    public class Curve : PoolObject<Curve, CurveInitData>
    {
        Vector2[] points;
        int current;
        float actualLength;
        short[] indices;
        CurveVertex[] vertices;

        public Vector2[] Points => points;
        public short[] Indices => indices;
        public CurveVertex[] Vertices => vertices;

        public override void Initialize(CurveInitData init)
        {
            points = new Vector2[init.segment + 1];
            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = init.pos;
            }
            indices = new short[init.segment * 6];
            vertices = new CurveVertex[init.segment * 2 + 2];
            current = 0;
            actualLength = 0;
        }
        public Vector2 GetCurveEnd()
        {
            return points[(current - (points.Length - 1) < 0) ? current + 1 : (current - (points.Length - 1))];
        }
        public bool IsCompacted()
        {
            for (int i = 0; i < points.Length - 1; ++i)
            {
                if (points[i] != points[i + 1]) return false;
            }
            return true;
        }
        public bool IterateSegment(Func<Vector2, Vector2, float, Vector2, Vector2, Vector2, float, float, bool> func, Vector2 bp, Vector2 p, Vector2 s, float pdr, float deg, float length)
        {
            if (func == null) return false;
            var index = (current - 1 < 0) ? points.Length - 1 : current - 1;
            var currentLength = 0f;
            var scaleLength = Math.Min(actualLength, length);
            for (int i = 0; i < points.Length - 1; ++i)
            {
                var headPoint = points[index];
                index = (index - 1 < 0) ? points.Length - 1 : index - 1;
                var tailPoint = points[index];
                var dir = tailPoint == headPoint ? new Vector2(0, 0) : Vector2.Normalize(headPoint - tailPoint);
                var segmentLength = (headPoint - tailPoint).Length();
                if (currentLength + segmentLength > scaleLength)
                {
                    tailPoint = headPoint - dir * (scaleLength - currentLength);
                    currentLength = scaleLength;
                }
                else
                {
                    currentLength += segmentLength;
                }
                var scale = currentLength / scaleLength;
                if (func(tailPoint, headPoint, scale, bp, p, s, pdr, deg)) return true;
            }
            return false;
        }
        public void Update(Vector2 pos, float width, float length)
        {
            points[current] = pos;
            var lastOne = points[(current - (points.Length - 1) < 0) ? current + 1 : (current - (points.Length - 1))];
            var lastTwo = points[(current - (points.Length - 2) < 0) ? current + 2 : (current - (points.Length - 2))];
            actualLength -= (lastOne - lastTwo).Length();
            actualLength += (points[current] - points[(current - 1 < 0) ? points.Length - 1 : current - 1]).Length();
            var scaleLength = Math.Min(actualLength, length);

            var halfWidth = width * 0.5f;
            var index = current;
            var vertexIndex = 0;
            var indexIndex = 0;
            var currentLength = 0f;
            for (int i = 0; i < points.Length - 1; ++i)
            {
                indices[indexIndex++] = (short)(i * 2);
                indices[indexIndex++] = (short)(i * 2 + 1);
                indices[indexIndex++] = (short)(i * 2 + 2);
                indices[indexIndex++] = (short)(i * 2 + 1);
                indices[indexIndex++] = (short)(i * 2 + 3);
                indices[indexIndex++] = (short)(i * 2 + 2);
                var headPoint = points[index];
                index = (index - 1 < 0) ? points.Length - 1 : index - 1;
                var tailPoint = points[index];
                var dir = tailPoint == headPoint ? new Vector2(0, 0) : Vector2.Normalize(headPoint - tailPoint);
                var segmentLength = (headPoint - tailPoint).Length();
                if (currentLength + segmentLength > scaleLength)
                {
                    tailPoint = headPoint - dir * (scaleLength - currentLength);
                    currentLength = scaleLength;
                }
                else
                { 
                    currentLength += segmentLength;
                }
                var left = MathHelper.Rotate(dir, -90) * halfWidth;
                var right = MathHelper.Rotate(dir, 90) * halfWidth;
                if (i == 0)
                {
                    vertices[vertexIndex].Pos = new Vector3(headPoint + left, 0);
                    vertices[vertexIndex].TexCoord = new Vector2(0, 0);
                    vertexIndex++;
                    vertices[vertexIndex].Pos = new Vector3(headPoint + right, 0);
                    vertices[vertexIndex].TexCoord = new Vector2(1, 0);
                    vertexIndex++;
                }
                var scale = currentLength / scaleLength;
                vertices[vertexIndex].Pos = new Vector3(tailPoint + left, 0);
                vertices[vertexIndex].TexCoord = new Vector2(0, scale);
                vertexIndex++;
                vertices[vertexIndex].Pos = new Vector3(tailPoint + right, 0);
                vertices[vertexIndex].TexCoord = new Vector2(1, scale);
                vertexIndex++;
            }

            current++;
            if (current >= points.Length)
            {
                current = 0;
            }
        }
    }
}
