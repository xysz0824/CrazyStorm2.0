/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using Blend = Microsoft.Xna.Framework.Graphics.Blend;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace CrazyStorm_Player
{
    internal struct ParticleRenderData
    {
        public ParticleBatchInstance Instance;
        public ParticleBatchKey Key;
    }

    internal struct CurveRenderData
    {
        public Texture2D Texture;
        public Vector4 SourceRect;
        public Vector4 MaskFrameRect;
        public Vector4 DistortFrameRect;
        public Vector4 MaskDissolve;
        public Vector4 Distort;
        public Texture2D MaskTexture;
        public Texture2D DistortTexture;
        public bool IsTextType;
        public ParticleBatchPass Pass;
        public bool RequiresShader;
    }

    internal sealed class ShaderContext : IDisposable
    {
        readonly Effect shader;
        readonly Texture2D whiteTexture;
        readonly EffectParameter shaderMaskRect;
        readonly EffectParameter shaderMaskInfo;
        readonly EffectParameter shaderMaskInv;
        readonly EffectParameter shaderMaskTexture;
        readonly EffectParameter shaderMaskFrameRect;
        readonly EffectParameter shaderMaskDissolve;
        readonly EffectParameter shaderCurveMaskFrameRect;
        readonly EffectParameter shaderCurveSourceRect;
        readonly EffectParameter shaderCurveMaskDissolve;
        readonly EffectParameter shaderCurveTypeInfo;
        readonly EffectParameter shaderDistortTexture;
        readonly EffectParameter shaderCurveDistortFrameRect;
        readonly EffectParameter shaderCurveDistort;
        readonly EffectParameter shaderMaskCount;
        readonly EffectParameter shaderRenderCenter;
        readonly EffectParameter shaderViewportSize;
        readonly Vector4[] maskRects;
        readonly Vector4[] maskInfos;
        readonly Vector4[] maskInvs;
        readonly Vector4[] maskDissolves;
        readonly Vector4[] maskFrameRects;
        readonly BlendState substration;
        readonly BlendState multiply;

        public ShaderContext(GraphicsDevice graphicsDevice, int width, int height, FrameOrientation frameOrientation)
        {
            Width = width;
            Height = height;
            FrameOrientation = frameOrientation;

            var shaderFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.shader.mgfxo");
            using (shaderFile)
            using (var ms = new MemoryStream())
            {
                shaderFile.CopyTo(ms);
                shader = new Effect(graphicsDevice, ms.ToArray());
            }

            whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });

            shaderMaskRect = shader.Parameters["MaskRect"];
            shaderMaskInfo = shader.Parameters["MaskInfo"];
            shaderMaskInv = shader.Parameters["MaskInv"];
            shaderMaskTexture = shader.Parameters["MaskTexture"];
            shaderMaskFrameRect = shader.Parameters["MaskFrameRect"];
            shaderMaskDissolve = shader.Parameters["MaskDissolve"];
            shaderCurveMaskFrameRect = shader.Parameters["CurveMaskFrameRect"];
            shaderCurveSourceRect = shader.Parameters["CurveSourceRect"];
            shaderCurveMaskDissolve = shader.Parameters["CurveMaskDissolve"];
            shaderCurveTypeInfo = shader.Parameters["CurveTypeInfo"];
            shaderDistortTexture = shader.Parameters["DistortTexture"];
            shaderCurveDistortFrameRect = shader.Parameters["CurveDistortFrameRect"];
            shaderCurveDistort = shader.Parameters["CurveDistort"];
            shaderMaskCount = shader.Parameters["MaskCount"];
            shaderRenderCenter = shader.Parameters["RenderCenter"];
            shaderViewportSize = shader.Parameters["ViewportSize"];
            shaderViewportSize?.SetValue(new Vector2(width, height));

            maskRects = new Vector4[ParticleManager.MAX_MASK_COUNT];
            maskInfos = new Vector4[ParticleManager.MAX_MASK_COUNT];
            maskInvs = new Vector4[ParticleManager.MAX_MASK_COUNT];
            maskDissolves = new Vector4[ParticleManager.MAX_MASK_COUNT];
            maskFrameRects = new Vector4[ParticleManager.MAX_MASK_COUNT];

            substration = new BlendState();
            substration.ColorSourceBlend = Blend.SourceAlpha;
            substration.AlphaSourceBlend = Blend.One;
            substration.ColorDestinationBlend = Blend.One;
            substration.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            substration.ColorBlendFunction = BlendFunction.Subtract;

            multiply = new BlendState();
            multiply.ColorSourceBlend = Blend.Zero;
            multiply.AlphaSourceBlend = Blend.One;
            multiply.ColorDestinationBlend = Blend.SourceColor;
            multiply.AlphaDestinationBlend = Blend.InverseSourceAlpha;

            ResetCurveShaderState();
        }

        public FrameOrientation FrameOrientation { get; set; }
        public int Width { get; }
        public int Height { get; }
        public bool LayerHasMask { get; private set; }
        internal Effect Effect => shader;
        internal Texture2D FallbackTexture => whiteTexture;

        public void Dispose()
        {
            substration?.Dispose();
            multiply?.Dispose();
            whiteTexture?.Dispose();
            shader?.Dispose();
        }

        public BlendState GetBlendState(BlendType blendType)
        {
            switch (blendType)
            {
                case BlendType.Additive:
                    return BlendState.Additive;
                case BlendType.Substraction:
                    return substration;
                case BlendType.Multiply:
                    return multiply;
                default:
                    return BlendState.NonPremultiplied;
            }
        }

        public void UpdateLayerMaskState(ParticleSystem system, Texture2D maskTexture)
        {
            LayerHasMask = ParticleManager.MaskCount > 0;
            Array.Clear(maskRects, 0, maskRects.Length);
            Array.Clear(maskInfos, 0, maskInfos.Length);
            Array.Clear(maskInvs, 0, maskInvs.Length);
            Array.Clear(maskDissolves, 0, maskDissolves.Length);
            Array.Clear(maskFrameRects, 0, maskFrameRects.Length);

            for (int i = 0; i < ParticleManager.MaskCount; ++i)
            {
                Vector4 frameRect;
                if (TryBuildMaskFrameRect(maskTexture, i, out frameRect))
                {
                    maskFrameRects[i] = frameRect;
                }

                var maskSize = ParticleManager.MaskSizeArray[i];
                var maskPosition = ParticleManager.MaskPositionArray[i];
                maskRects[i] = new Vector4(maskPosition.x, maskPosition.y, maskSize.x, maskSize.y);
                var maskRotateTrig = ParticleManager.MaskRotateTrigArray[i];
                maskInfos[i] = new Vector4(
                    ParticleManager.MaskLayerArray[i],
                    ParticleManager.MaskShapeArray[i],
                    maskRotateTrig.x,
                    maskRotateTrig.y);

                var ellipseInvSizeSq = ParticleManager.MaskEllipseInvSizeSqArray[i];
                var maskInvDiameter = Vector2.Zero;
                if (maskSize.x > 0 && maskSize.y > 0)
                {
                    maskInvDiameter = new Vector2(0.5f / maskSize.x, 0.5f / maskSize.y);
                }
                maskInvs[i] = new Vector4(
                    ellipseInvSizeSq.x,
                    ellipseInvSizeSq.y,
                    maskInvDiameter.X,
                    maskInvDiameter.Y);

                var animateFrame = (float)Math.Floor(ParticleManager.MaskAnimateFrameArray[i]);
                maskDissolves[i] = new Vector4(
                    ParticleManager.MaskDissolveStrengthArray[i],
                    ParticleManager.MaskDissolveEdgeWidthArray[i],
                    animateFrame * ParticleManager.MaskDissolveUSpeedArray[i],
                    animateFrame * ParticleManager.MaskDissolveVSpeedArray[i]);
            }

            shaderMaskCount?.SetValue(ParticleManager.MaskCount);
            shaderMaskRect?.SetValue(maskRects);
            shaderMaskInfo?.SetValue(maskInfos);
            shaderMaskInv?.SetValue(maskInvs);
            shaderMaskFrameRect?.SetValue(maskFrameRects);
            shaderMaskDissolve?.SetValue(maskDissolves);
            shaderMaskTexture?.SetValue(maskTexture ?? whiteTexture);
            shaderRenderCenter?.SetValue(new Vector2(Width / 2f, Height / 2f) + system.ScreenOffset.ToXna());
            ResetCurveShaderState();
        }

        public ParticleRenderData BuildParticleRenderData(ParticleBase particle, Texture2D texture, Rectangle rect, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects, Vector2 position,
            Texture2D maskTexture, Texture2D distortTexture, BlendType blendType)
        {
            var maskFrameRect = Vector4.Zero;
            if (particle.MaskType != null)
            {
                TryBuildMaskFrameRect(maskTexture, particle.MaskType, particle.GetMaskFrameIndex(), out maskFrameRect);
            }

            var distortFrameRect = Vector4.Zero;
            if (particle.DistortType != null)
            {
                TryBuildDistortFrameRect(distortTexture, particle.DistortType, particle.GetDistortFrameIndex(), out distortFrameRect);
            }

            var animateFrame = (float)Math.Floor(particle.PAnimateFrame);
            var pass = ResolveBatchPass(maskFrameRect, distortFrameRect, particle.DistortStrength);
            var useMaskTexture = LayerHasMask ||
                pass == ParticleBatchPass.TexturedMask ||
                pass == ParticleBatchPass.TexturedMaskDistort;

            ParticleBatchInstance instance;
            instance.Transform0 = new Vector4(position.X, position.Y, origin.X, origin.Y);
            instance.Transform1 = new Vector4(rect.Width, rect.Height, scale.X, scale.Y);
            instance.SourceRect = BuildSourceRect(texture, rect);
            instance.Color = color.ToVector4();
            instance.RotationFlip = new Vector4((float)Math.Cos(rotation), (float)Math.Sin(rotation),
                (spriteEffects & SpriteEffects.FlipHorizontally) != 0 ? 1f : 0f,
                (spriteEffects & SpriteEffects.FlipVertically) != 0 ? 1f : 0f);
            instance.MaskFrameRect = maskFrameRect;
            instance.DistortFrameRect = distortFrameRect;
            instance.MaskAndDistort = new Vector4(
                particle.DissolveStrength,
                particle.DissolveEdgeWidth,
                particle.DistortStrength,
                0f);
            instance.AnimateOffsets = new Vector4(
                animateFrame * particle.DissolveUSpeed,
                animateFrame * particle.DissolveVSpeed,
                animateFrame * particle.DistortUSpeed,
                animateFrame * particle.DistortVSpeed);
            instance.TypeInfo = new Vector4(particle.Type != null && particle.Type.IsTextType ? 1f : 0f, 0f, 0f, 0f);

            return new ParticleRenderData
            {
                Instance = instance,
                Key = new ParticleBatchKey
                {
                    BlendType = blendType,
                    Texture = texture,
                    MaskTexture = useMaskTexture ? maskTexture : null,
                    DistortTexture = pass == ParticleBatchPass.TexturedDistort || pass == ParticleBatchPass.TexturedMaskDistort
                        ? distortTexture
                        : null,
                    Pass = pass
                }
            };
        }

        public CurveRenderData BuildCurveRenderData(ParticleBase particle, Texture2D texture, Rectangle rect,
            Texture2D maskTexture, Texture2D distortTexture)
        {
            var renderData = new CurveRenderData
            {
                Texture = texture,
                SourceRect = BuildSourceRect(texture, rect),
                MaskTexture = maskTexture,
                DistortTexture = distortTexture,
                Pass = ParticleBatchPass.Textured,
                IsTextType = particle != null && particle.Type != null && particle.Type.IsTextType
            };
            if (particle == null || texture == null)
            {
                renderData.RequiresShader = LayerHasMask || renderData.IsTextType;
                return renderData;
            }

            if (particle.MaskType != null)
            {
                TryBuildMaskFrameRect(maskTexture, particle.MaskType, particle.GetMaskFrameIndex(), out renderData.MaskFrameRect);
                renderData.MaskDissolve = new Vector4(
                    particle.DissolveStrength,
                    particle.DissolveEdgeWidth,
                    (float)Math.Floor(particle.PAnimateFrame) * particle.DissolveUSpeed,
                    (float)Math.Floor(particle.PAnimateFrame) * particle.DissolveVSpeed);
            }

            if (particle.DistortType != null)
            {
                TryBuildDistortFrameRect(distortTexture, particle.DistortType, particle.GetDistortFrameIndex(), out renderData.DistortFrameRect);
                renderData.Distort = new Vector4(
                    particle.DistortStrength,
                    (float)Math.Floor(particle.PAnimateFrame) * particle.DistortUSpeed,
                    (float)Math.Floor(particle.PAnimateFrame) * particle.DistortVSpeed,
                    0f);
            }

            renderData.Pass = ResolveBatchPass(renderData.MaskFrameRect, renderData.DistortFrameRect, particle.DistortStrength);
            renderData.RequiresShader = LayerHasMask || renderData.Pass != ParticleBatchPass.Textured || renderData.IsTextType;
            return renderData;
        }

        public void ApplyCurveShader(CurveRenderData renderData)
        {
            ResetCurveShaderState();
            if (renderData.Pass == ParticleBatchPass.Textured && !LayerHasMask) return;

            if (HasValidFrameRect(renderData.MaskFrameRect))
            {
                shaderMaskTexture?.SetValue(renderData.MaskTexture ?? whiteTexture);
                shaderCurveMaskFrameRect?.SetValue(renderData.MaskFrameRect);
                shaderCurveMaskDissolve?.SetValue(renderData.MaskDissolve);
            }

            if (HasValidFrameRect(renderData.DistortFrameRect))
            {
                shaderDistortTexture?.SetValue(renderData.DistortTexture ?? whiteTexture);
                shaderCurveDistortFrameRect?.SetValue(renderData.DistortFrameRect);
                shaderCurveDistort?.SetValue(renderData.Distort);
            }

            if (renderData.Pass != ParticleBatchPass.Textured)
            {
                shaderCurveSourceRect?.SetValue(renderData.SourceRect);
            }
            shaderCurveTypeInfo?.SetValue(new Vector4(renderData.IsTextType ? 1f : 0f, 0f, 0f, 0f));
        }

        public void ResetCurveShaderState()
        {
            shaderCurveMaskFrameRect?.SetValue(Vector4.Zero);
            shaderCurveSourceRect?.SetValue(Vector4.Zero);
            shaderCurveMaskDissolve?.SetValue(Vector4.Zero);
            shaderCurveTypeInfo?.SetValue(Vector4.Zero);
            shaderDistortTexture?.SetValue(whiteTexture);
            shaderCurveDistortFrameRect?.SetValue(Vector4.Zero);
            shaderCurveDistort?.SetValue(Vector4.Zero);
        }

        Vector4 BuildSourceRect(Texture2D texture, Rectangle rect)
        {
            if (texture == null || rect.Width <= 0 || rect.Height <= 0) return Vector4.Zero;
            return new Vector4(rect.X / (float)texture.Width, rect.Y / (float)texture.Height,
                rect.Width / (float)texture.Width, rect.Height / (float)texture.Height);
        }

        bool TryBuildFrameRect(Texture2D texture, int x, int y, int width, int height, int frame, out Vector4 rect)
        {
            rect = Vector4.Zero;
            if (texture == null || width <= 0 || height <= 0 || x < 0 || y < 0) return false;
            if (x >= texture.Width || y >= texture.Height) return false;

            if (FrameOrientation == FrameOrientation.Vertical)
            {
                var rows = Math.Max(1, (texture.Height - y) / height);
                var cols = Math.Max(1, (texture.Width - x) / width);
                if (frame >= rows * cols) return false;
                x += frame / rows * width;
                y += frame % rows * height;
            }
            else
            {
                var cols = Math.Max(1, (texture.Width - x) / width);
                var rows = Math.Max(1, (texture.Height - y) / height);
                if (frame >= rows * cols) return false;
                x += frame % cols * width;
                y += frame / cols * height;
            }

            if (x >= texture.Width || y >= texture.Height) return false;
            rect = new Vector4(x / (float)texture.Width, y / (float)texture.Height,
                width / (float)texture.Width, height / (float)texture.Height);
            return true;
        }

        bool TryBuildMaskFrameRect(Texture2D texture, int maskIndex, out Vector4 rect)
        {
            rect = Vector4.Zero;
            if (texture == null || ParticleManager.MaskTextureEnabledArray[maskIndex] == 0) return false;

            var startPoint = ParticleManager.MaskTextureStartPointArray[maskIndex];
            var size = ParticleManager.MaskTextureSizeArray[maskIndex];
            if (size.x <= 0 || size.y <= 0 || startPoint.x < 0 || startPoint.y < 0) return false;

            var width = (int)size.x;
            var height = (int)size.y;
            var x = (int)startPoint.x;
            var y = (int)startPoint.y;
            var frame = (int)ParticleManager.MaskTextureFrameArray[maskIndex];
            return TryBuildFrameRect(texture, x, y, width, height, frame, out rect);
        }

        bool TryBuildMaskFrameRect(Texture2D texture, MaskType maskType, int frame, out Vector4 rect)
        {
            rect = Vector4.Zero;
            if (texture == null || maskType == null || maskType.Width <= 0 || maskType.Height <= 0 ||
                maskType.StartPoint.x < 0 || maskType.StartPoint.y < 0)
            {
                return false;
            }

            return TryBuildFrameRect(texture, (int)maskType.StartPoint.x, (int)maskType.StartPoint.y,
                maskType.Width, maskType.Height, frame, out rect);
        }

        bool TryBuildDistortFrameRect(Texture2D texture, DistortType distortType, int frame, out Vector4 rect)
        {
            rect = Vector4.Zero;
            if (texture == null || distortType == null || distortType.Width <= 0 || distortType.Height <= 0 ||
                distortType.StartPoint.x < 0 || distortType.StartPoint.y < 0)
            {
                return false;
            }

            return TryBuildFrameRect(texture, (int)distortType.StartPoint.x, (int)distortType.StartPoint.y,
                distortType.Width, distortType.Height, frame, out rect);
        }

        static bool HasValidFrameRect(Vector4 rect)
        {
            return rect.Z > 0 && rect.W > 0;
        }

        static ParticleBatchPass ResolveBatchPass(Vector4 maskFrameRect, Vector4 distortFrameRect, float distortStrength)
        {
            var maskEnabled = HasValidFrameRect(maskFrameRect);
            var distortEnabled = HasValidFrameRect(distortFrameRect) && distortStrength != 0;
            if (maskEnabled)
            {
                return distortEnabled ? ParticleBatchPass.TexturedMaskDistort : ParticleBatchPass.TexturedMask;
            }

            return distortEnabled ? ParticleBatchPass.TexturedDistort : ParticleBatchPass.Textured;
        }
    }
}
