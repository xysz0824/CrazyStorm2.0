/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using Blend = Microsoft.Xna.Framework.Graphics.Blend;
using Color = Microsoft.Xna.Framework.Color;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;
using File = CrazyStorm.Core.File;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SamplerState = Microsoft.Xna.Framework.Graphics.SamplerState;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace CrazyStorm_Player
{
    public enum FrameOrientation
    {
        Horizontal,
        Vertical
    }
    public class PlayerImpl
    {
        const int PARTICLE_PRESERVED_DIST = 50;
        const int CURVE_PRESERVED_DIST = 100;

        Effect shader;
        EffectParameter shaderMaskSize;
        EffectParameter shaderMaskPosition;
        EffectParameter shaderMaskLayer;
        EffectParameter shaderMaskShape;
        EffectParameter shaderMaskRotateTrig;
        EffectParameter shaderMaskEllipseInvSizeSq;
        EffectParameter shaderMaskInvDiameter;
        EffectParameter shaderMaskTexture;
        EffectParameter shaderMaskFrameRect;
        EffectParameter shaderMaskDissolveStrength;
        EffectParameter shaderMaskDissolveEdgeWidth;
        EffectParameter shaderMaskAnimateOffset;
        EffectParameter shaderMaskTextureEnabled;
        EffectParameter shaderParticleMaskFrameRect;
        EffectParameter shaderParticleSourceRect;
        EffectParameter shaderParticleMaskDissolveStrength;
        EffectParameter shaderParticleMaskDissolveEdgeWidth;
        EffectParameter shaderParticleMaskAnimateOffset;
        EffectParameter shaderParticleMaskTextureEnabled;
        EffectParameter shaderDistortTexture;
        EffectParameter shaderParticleDistortFrameRect;
        EffectParameter shaderParticleDistortStrength;
        EffectParameter shaderParticleDistortAnimateOffset;
        EffectParameter shaderParticleDistortTextureEnabled;
        EffectParameter shaderMaskCount;
        EffectParameter shaderRenderCenter;
        SpriteBatch spriteBatch;
        CurveBatch curveBatch;
        BlendState substration, multiply;
        Texture2D background;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        Texture2D defaultTexture;
        Dictionary<File, Dictionary<int, Texture2D>> customTextures;
        Dictionary<ParticleSystem, Texture2D> distortTextures;
        Dictionary<ParticleSystem, Texture2D> maskTextures;
        Dictionary<string, SoundEffect> sounds;
        Dictionary<string, SoundEffectInstance> soundInstances;
        Texture2D characterTexture;
        Texture2D pointTexture;
        Texture2D slowModeTexture;
        Controllable controllable;
        Dictionary<ParticleSystem, File> instances;
        BlendType lastBlendType = BlendType.None;
        bool currentLayerHasMask;
        bool spriteBatchBegun;
        bool curveBatchBegun;
        Texture2D fallbackMaskTexture;
        Vector4[] maskFrameRects;
        float[] maskTextureEnabled;
        Vector2[] maskInvDiameters;
        Vector2[] maskAnimateOffsets;

        public string TypeLibraryPath { get; set; }
        public FrameOrientation FrameOrientation { get; set; }
        public List<File> Files { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float FrameRate { get; set; }
        public string BackgroundPath { get; set; }
        public int SelectedParticleSystemIndex { get; set; }
        public string ControllableImagePath { get; set; }
        public string ControllableSetting { get; set; }
        public float CurrentFrame { get; set; }
        public PlayerImpl(int width, int height, float frameRate, int particleMaximum, int curveParticleMaximum)
        {
            Width = width;
            Height = height;
            FrameRate = frameRate;
            CurrentFrame = 1;
            EventManager.Initialize();
            ParticleManager.Initialize(width, height, PARTICLE_PRESERVED_DIST, CURVE_PRESERVED_DIST, 
                particleMaximum, curveParticleMaximum);
        }
        public void Initialize(GraphicsDevice gd)
        {
            //graphics
            var shaderFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.shader.mgfxo");
            using (var ms = new MemoryStream())
            {
                shaderFile.CopyTo(ms);
                shader = new Effect(gd, ms.ToArray());
                shaderMaskSize = shader.Parameters["MaskSize"];
                shaderMaskPosition = shader.Parameters["MaskPosition"];
                shaderMaskLayer = shader.Parameters["MaskLayer"];
                shaderMaskShape = shader.Parameters["MaskShape"];
                shaderMaskRotateTrig = shader.Parameters["MaskRotateTrig"];
                shaderMaskEllipseInvSizeSq = shader.Parameters["MaskEllipseInvSizeSq"];
                shaderMaskInvDiameter = shader.Parameters["MaskInvDiameter"];
                shaderMaskTexture = shader.Parameters["MaskTexture"];
                shaderMaskFrameRect = shader.Parameters["MaskFrameRect"];
                shaderMaskDissolveStrength = shader.Parameters["MaskDissolveStrength"];
                shaderMaskDissolveEdgeWidth = shader.Parameters["MaskDissolveEdgeWidth"];
                shaderMaskAnimateOffset = shader.Parameters["MaskAnimateOffset"];
                shaderMaskTextureEnabled = shader.Parameters["MaskTextureEnabled"];
                shaderParticleMaskFrameRect = shader.Parameters["ParticleMaskFrameRect"];
                shaderParticleSourceRect = shader.Parameters["ParticleSourceRect"];
                shaderParticleMaskDissolveStrength = shader.Parameters["ParticleMaskDissolveStrength"];
                shaderParticleMaskDissolveEdgeWidth = shader.Parameters["ParticleMaskDissolveEdgeWidth"];
                shaderParticleMaskAnimateOffset = shader.Parameters["ParticleMaskAnimateOffset"];
                shaderParticleMaskTextureEnabled = shader.Parameters["ParticleMaskTextureEnabled"];
                shaderDistortTexture = shader.Parameters["DistortTexture"];
                shaderParticleDistortFrameRect = shader.Parameters["ParticleDistortFrameRect"];
                shaderParticleDistortStrength = shader.Parameters["ParticleDistortStrength"];
                shaderParticleDistortAnimateOffset = shader.Parameters["ParticleDistortAnimateOffset"];
                shaderParticleDistortTextureEnabled = shader.Parameters["ParticleDistortTextureEnabled"];
                shaderMaskCount = shader.Parameters["MaskCount"];
                shaderRenderCenter = shader.Parameters["RenderCenter"];
            }
            shaderFile.Dispose();
            spriteBatch = new SpriteBatch(gd);
            curveBatch = new CurveBatch(gd);
            fallbackMaskTexture = new Texture2D(gd, 1, 1);
            fallbackMaskTexture.SetData(new[] { Color.White });
            maskFrameRects = new Vector4[ParticleManager.MAX_MASK_COUNT];
            maskTextureEnabled = new float[ParticleManager.MAX_MASK_COUNT];
            maskInvDiameters = new Vector2[ParticleManager.MAX_MASK_COUNT];
            maskAnimateOffsets = new Vector2[ParticleManager.MAX_MASK_COUNT];
            ResetParticleMaskShaderParameters();
            ResetParticleDistortShaderParameters();
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
            //controllable
            controllable = new Controllable(Width, Height);
            string[] setting = ControllableSetting.Split(',');
            if (setting.Length == 9)
            {
                controllable.selfStart = new Vector2(int.Parse(setting[0]), int.Parse(setting[1]));
                controllable.selfSize = new Vector2(int.Parse(setting[2]), int.Parse(setting[3]));
                controllable.selfCenter = new Vector2(int.Parse(setting[4]), int.Parse(setting[5]));
                controllable.selfFrames = int.Parse(setting[6]);
                controllable.selfDelay = int.Parse(setting[7]);
                controllable.selfRadius = int.Parse(setting[8]);
            }
            //Load background texture
            if (!string.IsNullOrWhiteSpace(BackgroundPath))
            {
                using (var file = new FileStream(BackgroundPath, FileMode.Open, FileAccess.Read))
                {
                    background = Texture2D.FromStream(gd, file);
                    float scale1 = Width / (float)background.Width;
                    float scale2 = Height / (float)background.Height;
                    if (scale1 < scale2)
                    {
                        backgroundScale = new Vector2(scale1, scale1);
                        backgroundPos.Y = (int)(Height - scale1 * background.Height) / 2;
                    }
                    else
                    {
                        backgroundScale = new Vector2(scale2, scale2);
                        backgroundPos.X = (int)(Width - scale2 * background.Width) / 2;
                    }
                }
            }
            //Load default textures
            var assembly = Assembly.GetExecutingAssembly();
            Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            Stream defaultTextureStream = null;
            if (string.IsNullOrEmpty(TypeLibraryPath))
            {
                defaultTextureStream = assembly.GetManifestResourceStream("CrazyStorm_Player.barrages.png");
            }
            else
            {
                defaultTextureStream = new FileStream($"typelibrary\\{Path.GetFileNameWithoutExtension(TypeLibraryPath)}.png", 
                    FileMode.Open, FileAccess.Read);
            }
            defaultTexture = Texture2D.FromStream(gd, defaultTextureStream);
            defaultTextureStream.Dispose();
            //Load main character texture
            if (!StringUtil.IsNullOrWhiteSpace(controllable.imagePath))
            {
                using (var file = new FileStream(controllable.imagePath, FileMode.Open, FileAccess.Read))
                {
                    characterTexture = Texture2D.FromStream(gd, file);
                }
            }
            Stream pointTextureStream = assembly.GetManifestResourceStream("CrazyStorm_Player.point.png");
            pointTexture = Texture2D.FromStream(gd, pointTextureStream);
            pointTextureStream.Dispose();
            Stream slowModeTextureStream = assembly.GetManifestResourceStream("CrazyStorm_Player.ring.png");
            slowModeTexture = Texture2D.FromStream(gd, slowModeTextureStream);
            slowModeTextureStream.Dispose();
            //Load custom textures and types
            customTextures = new Dictionary<File, Dictionary<int, Texture2D>>();
            foreach (var file in Files)
            {
                Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                if (!string.IsNullOrEmpty(file.ResourceDirectory)) Environment.CurrentDirectory = file.ResourceDirectory;
                foreach (var image in file.Images)
                {
                    if (!customTextures.ContainsKey(file)) customTextures[file] = new Dictionary<int, Texture2D>();
                    if (!System.IO.File.Exists(image.RelatviePath))
                    {
                        customTextures[file][image.ID] = null;
                        continue;
                    }
                    using (var stream = new FileStream(image.RelatviePath, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            customTextures[file][image.ID] = Texture2D.FromStream(gd, stream);
                        }
                        catch
                        {
                            customTextures[file][image.ID] = null;
                        }
                    }
                }
            }
            Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            distortTextures = new Dictionary<ParticleSystem, Texture2D>();
            maskTextures = new Dictionary<ParticleSystem, Texture2D>();
            //sounds
            sounds = new Dictionary<string, SoundEffect>();
            soundInstances = new Dictionary<string, SoundEffectInstance>();
            //final
            ForceField.OnForceImpactBody += ForceImpactBody;
            EventManager.OnSoundPlay += PlaySound;
            EventManager.OnSoundStop += StopSound;
            ParticleManager.OnLayerDraw += DrawLayer;
            ParticleManager.OnParticleDraw += (particle) => DrawParticle(spriteBatch, particle);
            ParticleManager.OnCurveParticleDraw += (particle) => DrawCurveParticle(spriteBatch, curveBatch, particle);
            FrameworkDispatcher.Update();
            instances = new Dictionary<ParticleSystem, File>();
            foreach (var file in Files)
            {
                var instance = file.ParticleSystems[SelectedParticleSystemIndex].Instantiate();
                instance.Reset(true);
                instances[instance] = file;
                distortTextures[instance] = ResolveDistortTexture(file, instance);
                maskTextures[instance] = ResolveMaskTexture(file, instance);
            }
            if (CurrentFrame != 1) SkipFrame(CurrentFrame, true);
        }
        public void Dispose()
        {
            ForceField.OnForceImpactBody -= ForceImpactBody;
            EventManager.OnSoundPlay -= PlaySound;
            EventManager.OnSoundStop -= StopSound;
            ParticleManager.OnLayerDraw -= DrawLayer;
            spriteBatch?.Dispose();
            background?.Dispose();
            defaultTexture?.Dispose();
            fallbackMaskTexture?.Dispose();
            foreach (var dict in customTextures.Values)
            {
                foreach (var tex in dict.Values)
                {
                    tex?.Dispose();
                }
            }
            customTextures.Clear();
            distortTextures?.Clear();
            maskTextures?.Clear();
            if (soundInstances != null)
            {
                foreach (var instance in soundInstances.Values)
                {
                    instance?.Stop();
                    instance?.Dispose();
                }
                soundInstances.Clear();
            }
            foreach (var sound in sounds.Values) sound?.Dispose();
            sounds.Clear();
            characterTexture?.Dispose();
            pointTexture?.Dispose();
            slowModeTexture?.Dispose();
            shader?.Dispose();
        }
        void ForceImpactBody(CrazyStorm.Core.Vector2 speedVector)
        {
            controllable.selfPos += speedVector.ToXna();
        }
        void PlaySound(string path)
        {
            if (!sounds.ContainsKey(path))
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    sounds[path] = SoundEffect.FromStream(stream);
                }
            }
            if (!soundInstances.ContainsKey(path))
            {
                soundInstances[path] = sounds[path].CreateInstance();
            }
            var soundInstance = soundInstances[path];
            soundInstance.Volume = 0.5f;
            soundInstance.Pitch = 0f;
            soundInstance.Pan = 0f;
            soundInstance.Stop();
            soundInstance.Play();
        }
        void StopSound(string path)
        {
            if (soundInstances == null || !soundInstances.ContainsKey(path)) return;
            soundInstances[path]?.Stop();
        }
        void UpdateCurrentFrame()
        {
            var minCurrentFrame = float.MaxValue;
            foreach (var instance in instances.Keys)
            {
                if (minCurrentFrame > instance.CurrentFrame) minCurrentFrame = instance.CurrentFrame;
            }
            if (minCurrentFrame != float.MaxValue) CurrentFrame = minCurrentFrame;
        }
        Texture2D ResolveMaskTexture(File file, ParticleSystem system)
        {
            if (file == null || system == null || system.MaskImage == null) return null;
            Dictionary<int, Texture2D> textures;
            if (!customTextures.TryGetValue(file, out textures)) return null;
            Texture2D texture;
            return textures.TryGetValue(system.MaskImage.ID, out texture) ? texture : null;
        }
        Texture2D ResolveDistortTexture(File file, ParticleSystem system)
        {
            if (file == null || system == null || system.DistortImage == null) return null;
            Dictionary<int, Texture2D> textures;
            if (!customTextures.TryGetValue(file, out textures)) return null;
            Texture2D texture;
            return textures.TryGetValue(system.DistortImage.ID, out texture) ? texture : null;
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

            // Keep editor/runtime behavior aligned: oversized frame rects remain valid and
            // the out-of-range area is resolved by the mask sampler's Clamp addressing.
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

            var width = maskType.Width;
            var height = maskType.Height;
            var x = (int)maskType.StartPoint.x;
            var y = (int)maskType.StartPoint.y;
            return TryBuildFrameRect(texture, x, y, width, height, frame, out rect);
        }
        bool TryBuildDistortFrameRect(Texture2D texture, DistortType distortType, int frame, out Vector4 rect)
        {
            rect = Vector4.Zero;
            if (texture == null || distortType == null || distortType.Width <= 0 || distortType.Height <= 0 ||
                distortType.StartPoint.x < 0 || distortType.StartPoint.y < 0)
            {
                return false;
            }

            var width = distortType.Width;
            var height = distortType.Height;
            var x = (int)distortType.StartPoint.x;
            var y = (int)distortType.StartPoint.y;
            return TryBuildFrameRect(texture, x, y, width, height, frame, out rect);
        }
        void ResetParticleMaskShaderParameters()
        {
            shaderParticleMaskFrameRect?.SetValue(Vector4.Zero);
            shaderParticleSourceRect?.SetValue(Vector4.Zero);
            shaderParticleMaskDissolveStrength?.SetValue(0f);
            shaderParticleMaskDissolveEdgeWidth?.SetValue(0f);
            shaderParticleMaskAnimateOffset?.SetValue(Vector2.Zero);
            shaderParticleMaskTextureEnabled?.SetValue(0f);
        }
        void ResetParticleDistortShaderParameters()
        {
            shaderDistortTexture?.SetValue(fallbackMaskTexture);
            shaderParticleDistortFrameRect?.SetValue(Vector4.Zero);
            shaderParticleDistortStrength?.SetValue(0f);
            shaderParticleDistortAnimateOffset?.SetValue(Vector2.Zero);
            shaderParticleDistortTextureEnabled?.SetValue(0f);
        }
        void UpdateParticleMaskShaderParameters(ParticleBase particle, Texture2D texture, Rectangle rect)
        {
            ResetParticleMaskShaderParameters();

            if (particle == null || particle.MaskType == null || texture == null) return;

            Texture2D maskTexture;
            if (!maskTextures.TryGetValue(particle.System, out maskTexture))
            {
                maskTexture = ResolveMaskTexture(instances[particle.System], particle.System);
                maskTextures[particle.System] = maskTexture;
            }

            Vector4 frameRect;
            if (!TryBuildMaskFrameRect(maskTexture, particle.MaskType, particle.GetMaskFrameIndex(), out frameRect)) return;

            shaderMaskTexture.SetValue(maskTexture ?? fallbackMaskTexture);
            shaderParticleMaskFrameRect.SetValue(frameRect);
            shaderParticleSourceRect.SetValue(BuildSourceRect(texture, rect));
            shaderParticleMaskDissolveStrength.SetValue(particle.DissolveStrength);
            shaderParticleMaskDissolveEdgeWidth.SetValue(particle.DissolveEdgeWidth);
            var animateFrame = (float)Math.Floor(particle.PAnimateFrame);
            shaderParticleMaskAnimateOffset.SetValue(new Vector2(
                animateFrame * particle.DissolveUSpeed,
                animateFrame * particle.DissolveVSpeed));
            shaderParticleMaskTextureEnabled.SetValue(1f);
        }
        void UpdateParticleDistortShaderParameters(ParticleBase particle, Texture2D texture, Rectangle rect)
        {
            ResetParticleDistortShaderParameters();

            if (particle == null || particle.DistortType == null || texture == null) return;

            Texture2D distortTexture;
            if (!distortTextures.TryGetValue(particle.System, out distortTexture))
            {
                distortTexture = ResolveDistortTexture(instances[particle.System], particle.System);
                distortTextures[particle.System] = distortTexture;
            }

            Vector4 frameRect;
            if (!TryBuildDistortFrameRect(distortTexture, particle.DistortType, particle.GetDistortFrameIndex(), out frameRect)) return;

            shaderDistortTexture.SetValue(distortTexture ?? fallbackMaskTexture);
            shaderParticleSourceRect.SetValue(BuildSourceRect(texture, rect));
            shaderParticleDistortFrameRect.SetValue(frameRect);
            shaderParticleDistortStrength.SetValue(particle.DistortStrength);
            shaderParticleDistortAnimateOffset.SetValue(new Vector2(
                (float)Math.Floor(particle.PAnimateFrame) * particle.DistortUSpeed,
                (float)Math.Floor(particle.PAnimateFrame) * particle.DistortVSpeed));
            shaderParticleDistortTextureEnabled.SetValue(1f);
        }
        void UpdateMaskShaderParameters(ParticleSystem system)
        {
            Array.Clear(maskFrameRects, 0, maskFrameRects.Length);
            Array.Clear(maskTextureEnabled, 0, maskTextureEnabled.Length);
            Array.Clear(maskInvDiameters, 0, maskInvDiameters.Length);
            Array.Clear(maskAnimateOffsets, 0, maskAnimateOffsets.Length);

            Texture2D maskTexture;
            if (!maskTextures.TryGetValue(system, out maskTexture))
            {
                maskTexture = ResolveMaskTexture(instances[system], system);
                maskTextures[system] = maskTexture;
            }
            for (int i = 0; i < ParticleManager.MaskCount; ++i)
            {
                Vector4 frameRect;
                if (TryBuildMaskFrameRect(maskTexture, i, out frameRect))
                {
                    maskFrameRects[i] = frameRect;
                    maskTextureEnabled[i] = 1;
                }

                var maskSize = ParticleManager.MaskSizeArray[i];
                if (maskSize.x > 0 && maskSize.y > 0)
                {
                    maskInvDiameters[i] = new Vector2(0.5f / maskSize.x, 0.5f / maskSize.y);
                }

                var animateFrame = (float)Math.Floor(ParticleManager.MaskAnimateFrameArray[i]);
                maskAnimateOffsets[i] = new Vector2(
                    animateFrame * ParticleManager.MaskDissolveUSpeedArray[i],
                    animateFrame * ParticleManager.MaskDissolveVSpeedArray[i]);
            }
            shaderMaskCount.SetValue(ParticleManager.MaskCount);
            shaderMaskSize.SetValue(ParticleManager.MaskSizeArray);
            shaderMaskPosition.SetValue(ParticleManager.MaskPositionArray);
            shaderMaskShape.SetValue(ParticleManager.MaskShapeArray);
            shaderMaskLayer.SetValue(ParticleManager.MaskLayerArray);
            shaderMaskRotateTrig.SetValue(ParticleManager.MaskRotateTrigArray);
            shaderMaskEllipseInvSizeSq.SetValue(ParticleManager.MaskEllipseInvSizeSqArray);
            shaderMaskInvDiameter.SetValue(maskInvDiameters);
            shaderMaskFrameRect.SetValue(maskFrameRects);
            shaderMaskDissolveStrength.SetValue(ParticleManager.MaskDissolveStrengthArray);
            shaderMaskDissolveEdgeWidth.SetValue(ParticleManager.MaskDissolveEdgeWidthArray);
            shaderMaskAnimateOffset.SetValue(maskAnimateOffsets);
            shaderMaskTextureEnabled.SetValue(maskTextureEnabled);
            shaderMaskTexture.SetValue(maskTexture ?? fallbackMaskTexture);
            shaderRenderCenter.SetValue(new Vector2(Width / 2, Height / 2) + system.ScreenOffset.ToXna());
            ResetParticleMaskShaderParameters();
            ResetParticleDistortShaderParameters();
        }
        void DrawLayer(ParticleSystem system, Layer layer, BlendType blendType)
        {
            EndCurveBatch();
            EndSpriteBatch();
            ParticleManager.ClearLayerMasks();
            ParticleManager.UpdateLayerMasks(system.Layers, layer);
            currentLayerHasMask = ParticleManager.MaskCount > 0;
            UpdateMaskShaderParameters(system);
            BeginParticleBatches(blendType);
            lastBlendType = blendType;
        }
        void DrawParticle(SpriteBatch spriteBatch, Particle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                EndSpriteBatch();
                BeginSpriteBatch(blendType);
            }
            lastBlendType = blendType;
            var file = instances[particle.System];
            var type = particle.Type;
            var tex = type.ID >= ParticleType.DefaultTypeIndex ? defaultTexture : type.Image != null ? customTextures[file][type.Image.ID] : null;
            if (tex == null) return;
            var center = new Vector2(Width / 2, Height / 2) + particle.System.ScreenOffset.ToXna();
            var origin = type.CenterPoint.ToXna();
            if (particle.WidthScale < 0) origin.X = type.Width - origin.X;
            if (particle.HeightScale < 0) origin.Y = type.Height - origin.Y;
            SpriteEffects spriteEffects = particle.WidthScale < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteEffects |= particle.HeightScale < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            float fogScale = (ParticleBase.FOG_TIME - particle.FogFrame) / 15.0f;
            var scale = new Vector2(Math.Abs(particle.WidthScale) + fogScale, Math.Abs(particle.HeightScale) + fogScale);
            var position = new Vector2(particle.PPosition.x, particle.PPosition.y) + center;
            float alpha = particle.Opacity / 100f - (ParticleBase.FOG_TIME - particle.FogFrame) / ParticleBase.FOG_TIME;
            var color = new Color(particle.RGB.r / 255f, particle.RGB.g / 255f, particle.RGB.b / 255f, alpha);
            int frame = (int)particle.PAnimateFrame / (type.Delay + 1) % type.Frames;
            float vOffset = (int)particle.PAnimateFrame * particle.VSpeed;
            var rect = new Rectangle((int)type.StartPoint.x, (int)(type.StartPoint.y - vOffset), type.Width, type.Height);
            if (FrameOrientation == FrameOrientation.Vertical)
            {
                var row = (int)Math.Max(1, (tex.Height - type.StartPoint.y) / rect.Height);
                rect.Offset(frame / row * rect.Width, frame % row * rect.Height);
            }
            else
            {
                var col = (int)Math.Max(1, (tex.Width - type.StartPoint.x) / rect.Width);
                rect.Offset(frame % col * rect.Width, frame / col * rect.Height);
            }
            var rad = MathHelper.ToRadians(particle.PRotation + 90);
            if (particle.MaskType != null || particle.DistortType != null)
            {
                DrawParticleWithShader(tex, particle, rect, color, rad, origin, scale, spriteEffects, blendType, position, center, alpha);
                return;
            }
            spriteBatch.Draw(tex, position, rect, color, rad, origin, scale, spriteEffects, 0);
            if (!particle.AfterimageEffect) return;
            for (int i = 0; i < Particle.AFTERIMAGE_COUNT; ++i)
            {
                var afterImage = particle.AfterImageData[i];
                if (afterImage.alpha > 0)
                {
                    position = new Vector2(afterImage.x, afterImage.y) + center;
                    rad = MathHelper.ToRadians(afterImage.rot + 90);
                    color.A = (byte)(afterImage.alpha * alpha * 255f);
                    spriteBatch.Draw(tex, position, rect, color, rad, origin, scale, spriteEffects, 0);
                }
                particle.AfterImageData[i] = afterImage;
            }
        }
        void DrawParticleWithShader(Texture2D texture, Particle particle, Rectangle rect, Color color, float rotation,
            Vector2 origin, Vector2 scale, SpriteEffects spriteEffects, BlendType blendType, Vector2 position, Vector2 center, float alpha)
        {
            EndCurveBatch();
            EndSpriteBatch();
            UpdateParticleMaskShaderParameters(particle, texture, rect);
            UpdateParticleDistortShaderParameters(particle, texture, rect);
            spriteBatch.Begin(SpriteSortMode.Deferred, GetBlendState(blendType), SamplerState.LinearWrap, null, null, shader);
            spriteBatch.Draw(texture, position, rect, color, rotation, origin, scale, spriteEffects, 0);
            if (particle.AfterimageEffect)
            {
                for (int i = 0; i < Particle.AFTERIMAGE_COUNT; ++i)
                {
                    var afterImage = particle.AfterImageData[i];
                    if (afterImage.alpha <= 0) continue;

                    position = new Vector2(afterImage.x, afterImage.y) + center;
                    rotation = MathHelper.ToRadians(afterImage.rot + 90);
                    color.A = (byte)(afterImage.alpha * alpha * 255f);
                    spriteBatch.Draw(texture, position, rect, color, rotation, origin, scale, spriteEffects, 0);
                }
            }
            spriteBatch.End();
            ResetParticleMaskShaderParameters();
            ResetParticleDistortShaderParameters();
            BeginParticleBatches(blendType);
        }
        void DrawCurveParticle(SpriteBatch spriteBatch, CurveBatch curveBatch, CurveParticle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                EndCurveBatch();
                EndSpriteBatch();
                BeginParticleBatches(blendType);
            }
            lastBlendType = blendType;
            var file = instances[particle.System];
            var type = particle.Type;
            var tex = type.ID >= ParticleType.DefaultTypeIndex ? defaultTexture : type.Image != null ? customTextures[file][type.Image.ID] : null;
            if (tex == null) return;
            var center = new Vector2(Width / 2, Height / 2) + particle.System.ScreenOffset.ToXna();
            float alpha = particle.Opacity / 100f - (ParticleBase.FOG_TIME - particle.FogFrame) / ParticleBase.FOG_TIME;
            var color = new Color(particle.RGB.r / 255f, particle.RGB.g / 255f, particle.RGB.b / 255f, alpha);
            int frame = (int)particle.PAnimateFrame / (type.Delay + 1) % type.Frames;
            float vOffset = (int)particle.PAnimateFrame * particle.VSpeed;
            var rect = new Rectangle((int)type.StartPoint.x, (int)(type.StartPoint.y - vOffset), type.Width, type.Height);
            if (FrameOrientation == FrameOrientation.Vertical)
            {
                var row = (int)Math.Max(1, (tex.Height - type.StartPoint.y) / rect.Height);
                rect.Offset(frame / row * rect.Width, frame % row * rect.Height);
            }
            else
            {
                var col = (int)Math.Max(1, (tex.Width - type.StartPoint.x) / rect.Width);
                rect.Offset(frame % col * rect.Width, frame / col * rect.Height);
            }
            if (particle.MaskType != null || particle.DistortType != null)
            {
                DrawCurveParticleWithShader(curveBatch, particle, tex, rect, center, color, blendType);
                return;
            }
            if (tex != null) curveBatch.Draw(particle.Curve, tex, rect, center, color);
        }
        void DrawCurveParticleWithShader(CurveBatch curveBatch, CurveParticle particle, Texture2D texture,
            Rectangle rect, Vector2 center, Color color, BlendType blendType)
        {
            EndCurveBatch();
            EndSpriteBatch();
            UpdateParticleMaskShaderParameters(particle, texture, rect);
            UpdateParticleDistortShaderParameters(particle, texture, rect);
            curveBatch.Begin(GetBlendState(blendType), shader);
            curveBatch.Draw(particle.Curve, texture, rect, center, color);
            curveBatch.End();
            ResetParticleMaskShaderParameters();
            ResetParticleDistortShaderParameters();
            BeginParticleBatches(blendType);
        }
        public void Update(KeyboardState keyboard, GameTime gameTime)
        {
            FrameworkDispatcher.Update();
            var minFrameFactor = 1f;
            foreach (var instance in instances.Keys)
            {
                instance.BodyPosition = controllable.selfPos.ToCore();
                if (minFrameFactor > instance.FrameFactor) minFrameFactor = instance.FrameFactor;
            }
            controllable.Update(keyboard, minFrameFactor * ParticleSystem.FRAME_RATE_BASE / FrameRate);
            EventManager.Update(FrameRate);
            foreach (var instance in instances.Keys)
            {
                instance.Update(FrameRate);
                ParticleManager.ClearLayerMasks();
                ParticleManager.UpdateLayerMasks(instance.Layers);
                var collidedCount = 0;
                CrazyStorm.Core.Vector2 newPos = default;
                var particles = ParticleManager.CheckCollision(false, controllable.selfPosLast.ToCore(),
                    controllable.selfPos.ToCore(), controllable.selfRadius, out collidedCount, out newPos);
                for (int i = 0; i < collidedCount; ++i) particles[i].Die();
                controllable.selfPos = newPos.ToXna();
            }
            ParticleManager.Update(FrameRate);
            UpdateCurrentFrame();
        }
        public void Draw(GraphicsDevice gd, GameTime gameTime)
        {
            gd.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null);
            if (background != null)
            {
                spriteBatch.Draw(background, backgroundPos, null, Color.White, 0, Vector2.Zero, backgroundScale, SpriteEffects.None, 0);
            }
            spriteBatch.End();
            var maxOffset = new Vector2(float.MinValue, float.MinValue);
            foreach (var instance in instances.Keys)
            {
                var offset = (instance.ScreenOffset - instance.LogicOffset).ToXna();
                if (maxOffset.X < offset.X) maxOffset.X = offset.X;
                if (maxOffset.Y < offset.Y) maxOffset.Y = offset.Y;
            }
            currentLayerHasMask = false;
            spriteBatchBegun = false;
            curveBatchBegun = false;
            lastBlendType = BlendType.None;
            foreach (var instance in instances.Keys) ParticleManager.Draw(instance);
            EndCurveBatch();
            EndSpriteBatch();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null);
            controllable.Draw(spriteBatch, characterTexture, pointTexture, slowModeTexture, maxOffset);
            spriteBatch.End();
        }
        BlendState GetBlendState(BlendType blendType)
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
        void BeginSpriteBatch(BlendType blendType)
        {
            var effect = currentLayerHasMask ? shader : null;
            spriteBatch.Begin(SpriteSortMode.Deferred, GetBlendState(blendType), SamplerState.LinearWrap, null, null, effect);
            spriteBatchBegun = true;
        }
        void BeginParticleBatches(BlendType blendType)
        {
            BeginSpriteBatch(blendType);
            curveBatch.Begin(GetBlendState(blendType), currentLayerHasMask ? shader : null);
            curveBatchBegun = true;
        }
        void EndSpriteBatch()
        {
            if (!spriteBatchBegun) return;
            spriteBatch.End();
            spriteBatchBegun = false;
        }
        void EndCurveBatch()
        {
            if (!curveBatchBegun) return;
            curveBatch.End();
            curveBatchBegun = false;
        }
        public void SetStatus(int i)
        {
            foreach(var instance in instances.Keys) instance.SetStatus(i);
        }
        public void SkipFrame(float targetFrame, bool replayFromStart)
        {
            if (instances == null || instances.Count == 0) return;
            foreach (var instance in instances.Keys)
            {
                instance.BodyPosition = controllable.selfPos.ToCore();
                instance.SkipFrame(targetFrame, replayFromStart, FrameRate);
            }
            UpdateCurrentFrame();
        }
    }
}
