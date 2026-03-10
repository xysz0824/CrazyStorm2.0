/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrazyStorm.Core;
using Curve = CrazyStorm.Core.Curve;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace CrazyStorm_Player
{
    public class CurveBatch
    {
        public const int MAX_CURVE_COUNT = 64;
        public const int MAX_SEGMENTS = 64;

        GraphicsDevice g;
        BlendState blendState;
        Curve[] curves = new Curve[MAX_CURVE_COUNT];
        Dictionary<Curve, Texture2D> curveTexs = new Dictionary<Curve, Texture2D>();
        short[] indices = new short[MAX_CURVE_COUNT * MAX_SEGMENTS * 6];
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[MAX_CURVE_COUNT * (MAX_SEGMENTS * 2 + 2)];
        int batchCurveCount;
        int indexIndex;
        int vertexIndex;
        bool beginCalled;
        Effect effect;
        public CurveBatch(GraphicsDevice graphicsDevice)
        {
            g = graphicsDevice;
        }
        public void Begin(BlendState blend, Effect effect)
        {
            if (beginCalled)
            {
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
            }
            beginCalled = true;
            blendState = blend;
            this.effect = effect;
        }
        public void Draw(Curve curve, Texture2D tex, Rectangle rect, Vector2 offset, Color color)
        {
            if (!beginCalled)
            {
                throw new InvalidOperationException("Begin must be called before calling Draw.");
            }
            if (curve == null || tex == null) return;
            curveTexs[curve] = tex;
            if (batchCurveCount > 0 && curves[batchCurveCount - 1] != null && !ReferenceEquals(tex, curveTexs[curves[batchCurveCount - 1]]))
            {
                Flush();
            }
            if (indexIndex + curve.Indices.Length > indices.Length || vertexIndex + curve.Vertices.Length > vertices.Length)
            {
                Flush();
            }
            var coordOffset = new Vector2(rect.X / (float)tex.Width, rect.Y / (float)tex.Height);
            var coordScale = new Vector2(rect.Width / (float)tex.Width, rect.Height / (float)tex.Height);
            curves[batchCurveCount++] = curve;
            for (int i = 0; i < curve.Indices.Length; ++i)
            {
                indices[indexIndex + i] = (short)(vertexIndex + curve.Indices[i]);
            }
            indexIndex += curve.Indices.Length;
            for (int i = 0; i < curve.Vertices.Length; ++i)
            {
                var vertex = curve.Vertices[i];
                var pos = new Vector3(vertex.Pos.x, vertex.Pos.y, vertex.Pos.z);
                var texcoord = new Vector2(vertex.TexCoord.x, vertex.TexCoord.y);
                vertices[vertexIndex + i] = new VertexPositionColorTexture
                {
                    Position = pos,
                };
                vertices[vertexIndex + i].Position.X += offset.X;
                vertices[vertexIndex + i].Position.Y += offset.Y;
                vertices[vertexIndex + i].TextureCoordinate = texcoord * coordScale + coordOffset;
                vertices[vertexIndex + i].Color = color;
            }
            vertexIndex += curve.Vertices.Length;
            if (indexIndex == indices.Length || vertexIndex == vertices.Length || curves.Length == batchCurveCount)
            {
                Flush();
            }
        }
        public void End()
        {
            if (!beginCalled)
            {
                throw new InvalidOperationException("Begin must be called before calling End.");
            }
            beginCalled = false;
            Flush();
        }
        void Flush()
        {
            if (batchCurveCount <= 0) return;
            g.BlendState = blendState;
            if (effect != null)
            {
                var texture = curveTexs[curves[batchCurveCount - 1]];
                var textureParameter = effect.Parameters["Texture"];
                if (textureParameter != null) textureParameter.SetValue(texture);
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    g.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexIndex, indices, 0, indexIndex / 3, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            else
            {
                g.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexIndex, indices, 0, indexIndex / 3, VertexPositionColorTexture.VertexDeclaration);
            }
            batchCurveCount = 0;
            vertexIndex = 0;
            indexIndex = 0;
        }
    }
}
