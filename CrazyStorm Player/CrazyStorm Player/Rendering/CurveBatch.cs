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
using Vector4 = Microsoft.Xna.Framework.Vector4;

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
        BasicEffect basicEffect;
        readonly ShaderContext shaderContext;
        readonly RasterizerState rasterizerState;
        ParticleBatchPass shaderPass;
        internal CurveBatch(GraphicsDevice graphicsDevice, ShaderContext shaderContext)
        {
            g = graphicsDevice;
            this.shaderContext = shaderContext;
            rasterizerState = new RasterizerState { CullMode = CullMode.None };
            basicEffect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }
        internal void Begin(BlendState blend, ParticleBatchPass pass = ParticleBatchPass.Textured)
        {
            if (beginCalled)
            {
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
            }
            beginCalled = true;
            blendState = blend;
            shaderPass = pass;
        }
        internal void DrawCurve(Curve curve, CurveRenderData renderData, Vector2 offset, Color color)
        {
            Draw(curve, renderData.Texture, renderData.SourceRect, offset, color);
        }
        public void Draw(Curve curve, Texture2D tex, Rectangle rect, Vector2 offset, Color color)
        {
            if (tex == null)
            {
                Draw(curve, tex, Vector4.Zero, offset, color);
                return;
            }
            Draw(curve, tex, new Vector4(rect.X / (float)tex.Width, rect.Y / (float)tex.Height,
                rect.Width / (float)tex.Width, rect.Height / (float)tex.Height), offset, color);
        }
        void Draw(Curve curve, Texture2D tex, Vector4 sourceRect, Vector2 offset, Color color)
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
            if (curve.ActiveIndexCount <= 0 || curve.ActiveVertexCount <= 0) return;
            if (indexIndex + curve.ActiveIndexCount > indices.Length || vertexIndex + curve.ActiveVertexCount > vertices.Length)
            {
                Flush();
            }
            var coordOffset = new Vector2(sourceRect.X, sourceRect.Y);
            var coordScale = new Vector2(sourceRect.Z, sourceRect.W);
            curves[batchCurveCount++] = curve;
            for (int i = 0; i < curve.ActiveIndexCount; ++i)
            {
                indices[indexIndex + i] = (short)(vertexIndex + curve.Indices[i]);
            }
            indexIndex += curve.ActiveIndexCount;
            for (int i = 0; i < curve.ActiveVertexCount; ++i)
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
            vertexIndex += curve.ActiveVertexCount;
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
            g.DepthStencilState = DepthStencilState.None;
            g.RasterizerState = rasterizerState;
            g.SamplerStates[0] = SamplerState.LinearWrap;
            var texture = curveTexs[curves[batchCurveCount - 1]];
            var effect = shaderContext != null ? shaderContext.Effect : null;
            if (effect != null)
            {
                var technique = GetTechnique(shaderPass);
                if (technique != null) effect.CurrentTechnique = technique;
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
                var viewport = g.Viewport;
                basicEffect.World = Matrix.Identity;
                basicEffect.View = Matrix.Identity;
                basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
                basicEffect.Texture = texture;
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    g.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexIndex, indices, 0, indexIndex / 3, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            batchCurveCount = 0;
            vertexIndex = 0;
            indexIndex = 0;
        }
        EffectTechnique GetTechnique(ParticleBatchPass pass)
        {
            var effect = shaderContext?.Effect;
            if (effect == null) return null;
            switch (pass)
            {
                case ParticleBatchPass.Textured:
                    return effect.Techniques["CurveTextured"];
                case ParticleBatchPass.TexturedMask:
                    return effect.Techniques["CurveTexturedMask"] ?? effect.Techniques["CurveTextured"];
                case ParticleBatchPass.TexturedDistort:
                    return effect.Techniques["CurveTexturedDistort"] ?? effect.Techniques["CurveTextured"];
                case ParticleBatchPass.TexturedMaskDistort:
                    return effect.Techniques["CurveTexturedMaskDistort"] ?? effect.Techniques["CurveTextured"];
                default:
                    return effect.Techniques["CurveTextured"];
            }
        }
    }
}
