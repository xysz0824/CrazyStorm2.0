/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;
using BlendType = CrazyStorm.Core.BlendType;

namespace CrazyStorm_Player
{
    public enum ParticleBatchPass
    {
        Textured,
        TexturedMask,
        TexturedDistort,
        TexturedMaskDistort
    }

    public struct ParticleBatchKey
    {
        public BlendType BlendType;
        public Texture2D Texture;
        public Texture2D MaskTexture;
        public Texture2D DistortTexture;
        public ParticleBatchPass Pass;

        public bool Equals(ParticleBatchKey other)
        {
            if (BlendType != other.BlendType || Pass != other.Pass || !ReferenceEquals(Texture, other.Texture))
            {
                return false;
            }

            switch (Pass)
            {
                case ParticleBatchPass.Textured:
                    return true;
                case ParticleBatchPass.TexturedMask:
                    return ReferenceEquals(MaskTexture, other.MaskTexture);
                case ParticleBatchPass.TexturedDistort:
                    return ReferenceEquals(DistortTexture, other.DistortTexture);
                case ParticleBatchPass.TexturedMaskDistort:
                    return ReferenceEquals(MaskTexture, other.MaskTexture) &&
                        ReferenceEquals(DistortTexture, other.DistortTexture);
                default:
                    return false;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleBatchInstance : IVertexType
    {
        public Vector4 Transform0;
        public Vector4 Transform1;
        public Vector4 SourceRect;
        public Vector4 Color;
        public Vector4 RotationFlip;
        public Vector4 MaskFrameRect;
        public Vector4 DistortFrameRect;
        public Vector4 MaskAndDistort;
        public Vector4 AnimateOffsets;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
            new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
            new VertexElement(80, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 6),
            new VertexElement(96, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 7),
            new VertexElement(112, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 8),
            new VertexElement(128, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 9));

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }

    public class ParticleBatch : IDisposable
    {
        public const int MAX_INSTANCE_COUNT = 1024;

        struct ParticleQuadVertex : IVertexType
        {
            public Vector2 Position;
            public Vector2 TextureCoordinate;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        }

        readonly GraphicsDevice graphicsDevice;
        readonly PlayerShaderContext shaderContext;
        readonly DynamicVertexBuffer instanceBuffer;
        readonly VertexBuffer quadVertexBuffer;
        readonly IndexBuffer quadIndexBuffer;
        readonly ParticleBatchInstance[] instances;
        readonly RasterizerState rasterizerState;
        ParticleBatchKey currentKey;
        BlendState blendState;
        int instanceCount;
        bool hasBatchKey;
        bool beginCalled;

        internal ParticleBatch(GraphicsDevice graphicsDevice, PlayerShaderContext shaderContext)
        {
            this.graphicsDevice = graphicsDevice;
            this.shaderContext = shaderContext;
            instances = new ParticleBatchInstance[MAX_INSTANCE_COUNT];
            instanceBuffer = new DynamicVertexBuffer(graphicsDevice, ParticleBatchInstance.VertexDeclaration, MAX_INSTANCE_COUNT, BufferUsage.WriteOnly);
            quadVertexBuffer = new VertexBuffer(graphicsDevice, ParticleQuadVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            quadVertexBuffer.SetData(new[]
            {
                new ParticleQuadVertex { Position = new Vector2(0, 0), TextureCoordinate = new Vector2(0, 0) },
                new ParticleQuadVertex { Position = new Vector2(1, 0), TextureCoordinate = new Vector2(1, 0) },
                new ParticleQuadVertex { Position = new Vector2(1, 1), TextureCoordinate = new Vector2(1, 1) },
                new ParticleQuadVertex { Position = new Vector2(0, 1), TextureCoordinate = new Vector2(0, 1) }
            });
            quadIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            quadIndexBuffer.SetData(new short[] { 0, 1, 2, 0, 2, 3 });
            rasterizerState = new RasterizerState { CullMode = CullMode.None };
        }

        public void Begin(BlendState blendState)
        {
            if (beginCalled)
            {
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");
            }
            beginCalled = true;
            this.blendState = blendState;
            instanceCount = 0;
            hasBatchKey = false;
        }

        internal void DrawParticle(ParticleRenderData renderData)
        {
            Draw(renderData.Instance, renderData.Key);
        }

        public void Draw(ParticleBatchInstance instance, ParticleBatchKey key)
        {
            if (!beginCalled)
            {
                throw new InvalidOperationException("Begin must be called before calling Draw.");
            }
            if (instanceCount > 0 && !currentKey.Equals(key))
            {
                Flush();
            }
            if (instanceCount == instances.Length)
            {
                Flush();
            }
            if (!hasBatchKey)
            {
                currentKey = key;
                hasBatchKey = true;
            }
            instances[instanceCount++] = instance;
        }

        public void End()
        {
            if (!beginCalled)
            {
                throw new InvalidOperationException("Begin must be called before calling End.");
            }
            Flush();
            beginCalled = false;
        }

        public void Dispose()
        {
            instanceBuffer?.Dispose();
            quadVertexBuffer?.Dispose();
            quadIndexBuffer?.Dispose();
            rasterizerState?.Dispose();
        }

        void Flush()
        {
            if (instanceCount <= 0 || !hasBatchKey) return;

            instanceBuffer.SetData(instances, 0, instanceCount, SetDataOptions.Discard);
            graphicsDevice.BlendState = blendState;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = rasterizerState;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            graphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
            graphicsDevice.Indices = quadIndexBuffer;
            graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(quadVertexBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1));

            var effect = shaderContext.Effect;
            var technique = GetTechnique(currentKey.Pass);
            if (technique != null)
            {
                effect.CurrentTechnique = technique;
            }
            effect.Parameters["Texture"]?.SetValue(currentKey.Texture ?? shaderContext.FallbackTexture);
            effect.Parameters["MaskTexture"]?.SetValue(currentKey.MaskTexture ?? shaderContext.FallbackTexture);
            effect.Parameters["DistortTexture"]?.SetValue(currentKey.DistortTexture ?? shaderContext.FallbackTexture);
            effect.Parameters["ViewportSize"]?.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, instanceCount);
            }

            instanceCount = 0;
            hasBatchKey = false;
        }

        EffectTechnique GetTechnique(ParticleBatchPass pass)
        {
            var effect = shaderContext.Effect;
            var texturedTechnique = effect.Techniques["BatchTexturedInstanced"];
            var texturedMaskTechnique = effect.Techniques["BatchTexturedMaskInstanced"];
            var texturedDistortTechnique = effect.Techniques["BatchTexturedDistortInstanced"];
            var texturedMaskDistortTechnique = effect.Techniques["BatchTexturedMaskDistortInstanced"];
            switch (pass)
            {
                case ParticleBatchPass.Textured:
                    return texturedTechnique;
                case ParticleBatchPass.TexturedMask:
                    return texturedMaskTechnique ?? texturedTechnique;
                case ParticleBatchPass.TexturedDistort:
                    return texturedDistortTechnique ?? texturedTechnique;
                case ParticleBatchPass.TexturedMaskDistort:
                    return texturedMaskDistortTechnique ?? texturedTechnique;
                default:
                    return texturedTechnique;
            }
        }
    }
}
