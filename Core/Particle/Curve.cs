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
    public enum CurveType
    {
        Line,
        Ray,
        Curve
    }
    public struct CurveInitData
    {
        public Vector2 pos;
        public Vector2 head;
        public int length;
        public int segment;
        public CurveType type;
        public bool snakeUpdate;
    }
    public class Curve : PoolObject<Curve, CurveInitData>
    {
        const float POSITION_EPSILON = 0.0001f;
        const float POSITION_EPSILON_SQUARED = POSITION_EPSILON * POSITION_EPSILON;

        CurveType type;
        bool snakeUpdate;
        Vector2[] points;
        int current;
        float actualLength;
        short[] indices;
        CurveVertex[] vertices;
        int activePointCount;
        int activeIndexCount;
        int activeVertexCount;

        public CurveType Type => type;
        public Vector2[] Points => points;
        public short[] Indices => indices;
        public CurveVertex[] Vertices => vertices;
        public int ActiveIndexCount => activeIndexCount;
        public int ActiveVertexCount => activeVertexCount;

        public override void Initialize(CurveInitData init)
        {
            type = init.type;
            snakeUpdate = init.snakeUpdate && type == CurveType.Curve;
            points = new Vector2[init.segment + 1];
            indices = new short[init.segment * 6];
            vertices = new CurveVertex[init.segment * 2 + 2];
            current = 0;
            activePointCount = 0;
            actualLength = 0;
            activeIndexCount = 0;
            activeVertexCount = 0;
            if (type == CurveType.Ray)
            {
                actualLength = init.length;
                RebuildStraightPoints(init.pos, init.head, actualLength);
            }
            else
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    points[i] = init.pos;
                }
                activePointCount = snakeUpdate ? 1 : points.Length;
            }
            BuildVertices(0, init.length);
        }
        public Vector2 GetCurveEnd()
        {
            if (activePointCount <= 0) return Vector2.Zero;
            Vector2 result = points[current];
            IterateEffectiveSegments((tailPoint, headPoint, scale) =>
            {
                result = tailPoint;
                return false;
            }, actualLength);
            return result;
        }
        public bool IterateSegment(Func<Vector2, Vector2, float, Vector2, Vector2, Vector2, float, float, bool> func, Vector2 bp, Vector2 p, Vector2 s, float pdr, float deg, float length)
        {
            if (func == null) return false;
            return IterateEffectiveSegments((tailPoint, headPoint, scale) =>
            {
                return func(tailPoint, headPoint, scale, bp, p, s, pdr, deg);
            }, length);
        }
        public void SetSnakeUpdate(bool enabled)
        {
            snakeUpdate = enabled && type == CurveType.Curve;
        }
        public void Update(Vector2 pos, Vector2 head, float width, float length)
        {
            if (type == CurveType.Curve)
            {
                if (!snakeUpdate || activePointCount <= 0 || (pos - points[current]).LengthSquared() > POSITION_EPSILON_SQUARED)
                {
                    AppendCurvePoint(pos);
                }
            }
            else
            {
                actualLength = length;
                RebuildStraightPoints(pos, head, actualLength);
            }
            BuildVertices(width, length);
        }

        int GetPointIndex(int age)
        {
            var index = current - age;
            if (index < 0) index += points.Length;
            return index;
        }
        void AppendCurvePoint(Vector2 pos)
        {
            if (activePointCount <= 0)
            {
                points[0] = pos;
                current = 0;
                activePointCount = 1;
                actualLength = 0;
                return;
            }

            var newIndex = current + 1;
            if (newIndex >= points.Length) newIndex = 0;
            var previous = points[current];
            if (activePointCount == points.Length)
            {
                var oldestIndex = newIndex;
                var nextOldestIndex = oldestIndex + 1 >= points.Length ? 0 : oldestIndex + 1;
                actualLength -= (points[nextOldestIndex] - points[oldestIndex]).Length();
                if (actualLength < 0) actualLength = 0;
            }
            else
            {
                activePointCount++;
            }
            points[newIndex] = pos;
            current = newIndex;
            actualLength += (points[current] - previous).Length();
        }
        void RebuildStraightPoints(Vector2 pos, Vector2 head, float length)
        {
            head.Normalize();

            activePointCount = points.Length;
            current = 0;
            for (int i = 0; i < points.Length; ++i)
            {
                if (type == CurveType.Line) points[i] = pos - head * (float)i / (points.Length - 1) * length;
                else points[i] = pos + head * (float)i / (points.Length - 1) * length;
            }
        }
        bool IterateEffectiveSegments(Func<Vector2, Vector2, float, bool> func, float length)
        {
            if (func == null || activePointCount <= 1) return false;
            var scaleLength = Math.Min(actualLength, Math.Max(0, length));
            if (scaleLength <= POSITION_EPSILON) return false;

            var currentLength = 0f;
            for (int i = 0; i < activePointCount - 1; ++i)
            {
                var headPoint = points[GetPointIndex(i)];
                var tailPoint = points[GetPointIndex(i + 1)];
                var delta = headPoint - tailPoint;
                var segmentLength = delta.Length();
                if (segmentLength <= POSITION_EPSILON) continue;

                var dir = delta / segmentLength;
                var remainingLength = scaleLength - currentLength;
                if (segmentLength > remainingLength)
                {
                    tailPoint = headPoint - dir * remainingLength;
                    segmentLength = remainingLength;
                }

                currentLength += segmentLength;
                var scale = currentLength / scaleLength;
                if (func(tailPoint, headPoint, scale)) return true;
                if (currentLength >= scaleLength - POSITION_EPSILON) break;
            }
            return false;
        }
        void BuildVertices(float width, float length)
        {
            activeIndexCount = 0;
            activeVertexCount = 0;

            var scaleLength = Math.Min(actualLength, Math.Max(0, length));
            if (activePointCount <= 1 || scaleLength <= POSITION_EPSILON) return;

            var halfWidth = width * 0.5f;
            var segmentIndex = 0;
            IterateEffectiveSegments((tailPoint, headPoint, scale) =>
            {
                var delta = headPoint - tailPoint;
                var segmentLength = delta.Length();
                if (segmentLength <= POSITION_EPSILON) return false;

                var dir = delta / segmentLength;
                var left = MathHelper.Rotate(dir, -90) * halfWidth;
                var right = MathHelper.Rotate(dir, 90) * halfWidth;
                if (segmentIndex == 0)
                {
                    vertices[activeVertexCount].Pos = new Vector3(headPoint + left, 0);
                    vertices[activeVertexCount].TexCoord = new Vector2(0, 0);
                    activeVertexCount++;
                    vertices[activeVertexCount].Pos = new Vector3(headPoint + right, 0);
                    vertices[activeVertexCount].TexCoord = new Vector2(1, 0);
                    activeVertexCount++;
                }

                var baseVertexIndex = segmentIndex * 2;
                indices[activeIndexCount++] = (short)baseVertexIndex;
                indices[activeIndexCount++] = (short)(baseVertexIndex + 1);
                indices[activeIndexCount++] = (short)(baseVertexIndex + 2);
                indices[activeIndexCount++] = (short)(baseVertexIndex + 1);
                indices[activeIndexCount++] = (short)(baseVertexIndex + 3);
                indices[activeIndexCount++] = (short)(baseVertexIndex + 2);

                vertices[activeVertexCount].Pos = new Vector3(tailPoint + left, 0);
                vertices[activeVertexCount].TexCoord = new Vector2(0, scale);
                activeVertexCount++;
                vertices[activeVertexCount].Pos = new Vector3(tailPoint + right, 0);
                vertices[activeVertexCount].TexCoord = new Vector2(1, scale);
                activeVertexCount++;
                segmentIndex++;
                return false;
            }, length);
        }
    }
}
