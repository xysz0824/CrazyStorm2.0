/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using File = CrazyStorm.Core.File;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SamplerState = Microsoft.Xna.Framework.Graphics.SamplerState;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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

        GraphicsDevice graphicsDevice;
        ShaderContext shaderContext;
        SpriteBatch spriteBatch;
        ParticleBatch particleBatch;
        CurveBatch curveBatch;
        Texture2D background;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        Texture2D defaultTexture;
        Texture2D transparentTexture;
        Dictionary<File, Dictionary<int, Texture2D>> customTextures;
        Dictionary<ParticleSystem, Texture2D> distortTextures;
        Dictionary<ParticleSystem, Texture2D> maskTextures;
        Dictionary<string, SoundEffect> sounds;
        Dictionary<string, SoundEffectInstance> soundInstances;
        Texture2D characterTexture;
        Texture2D pointTexture;
        Texture2D slowModeTexture;
        Controllable controllable;
        List<ParticleSystem> instances;
        BlendType lastBlendType = BlendType.None;
        bool particleBatchBegun;
        bool curveBatchBegun;

        public string TypeLibraryPath { get; set; }
        public FrameOrientation FrameOrientation { get; set; }
        public List<File> Files { get; private set; } = new List<File>();
        public int Width { get; set; }
        public int Height { get; set; }
        public float FrameRate { get; set; }
        public string BackgroundPath { get; set; }
        public int SelectedParticleSystemIndex { get; set; }
        public string ControllableImagePath { get; set; }
        public string ControllableSetting { get; set; }
        public float CurrentFrame { get; set; }
        public PlayerImpl(string typeLibraryPath, int width, int height, float frameRate, int particleMaximum, int curveParticleMaximum)
        {
            Width = width;
            Height = height;
            FrameRate = frameRate;
            CurrentFrame = 1;
            TypeLibraryPath = typeLibraryPath;
            FontHelper.EnsureInitialized();
            ParticleType.LoadDefaultTypes(typeLibraryPath);
            EventManager.Initialize();
            ParticleManager.Initialize(width, height, PARTICLE_PRESERVED_DIST, CURVE_PRESERVED_DIST, 
                particleMaximum, curveParticleMaximum);
        }
        public void Initialize(GraphicsDevice gd)
        {
            //graphics
            graphicsDevice = gd;
            shaderContext = new ShaderContext(gd, Width, Height, FrameOrientation);
            spriteBatch = new SpriteBatch(gd);
            particleBatch = new ParticleBatch(gd, shaderContext);
            curveBatch = new CurveBatch(gd, shaderContext);
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
            transparentTexture = new Texture2D(gd, 1, 1);
            transparentTexture.SetData(new[] { new Color(255, 255, 255, 0) });
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
            ParticleManager.OnParticleDraw += DrawParticle;
            ParticleManager.OnCurveParticleDraw += DrawCurveParticle;
            Text.OnNeedTextType += EnsureTextTypes;
            FrameworkDispatcher.Update();
            instances = new List<ParticleSystem>();
            foreach (var file in Files)
            {
                var instance = file.ParticleSystems[SelectedParticleSystemIndex].Instantiate(file);
                instances.Add(instance);
                instance.Reset(true);
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
            ParticleManager.OnParticleDraw -= DrawParticle;
            ParticleManager.OnCurveParticleDraw -= DrawCurveParticle;
            Text.OnNeedTextType -= EnsureTextTypes;
            particleBatch?.Dispose();
            curveBatch?.Dispose();
            shaderContext?.Dispose();
            spriteBatch?.Dispose();
            background?.Dispose();
            defaultTexture?.Dispose();
            transparentTexture?.Dispose();
            foreach (var dict in customTextures.Values)
            {
                foreach (var tex in dict.Values)
                {
                    tex?.Dispose();
                }
            }
            customTextures.Clear();
            FontTextManager.Clear();
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
        }
        List<ParticleType> EnsureTextTypes(ParticleSystem system, Text text)
        {
            return FontTextManager.UpdateTextResources(system.File, system, text, instances, (fileResource, pngBytes) =>
            {
                Texture2D newTexture = null;
                using (var stream = new MemoryStream(pngBytes, false))
                {
                    newTexture = Texture2D.FromStream(graphicsDevice, stream);
                }
                if (!customTextures.ContainsKey(fileResource.File)) customTextures[fileResource.File] = new Dictionary<int, Texture2D>();
                customTextures[fileResource.File].TryGetValue(fileResource.ID, out var oldTexture);
                customTextures[fileResource.File][fileResource.ID] = newTexture;
                oldTexture?.Dispose();
            });
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
            foreach (var instance in instances)
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
        void DrawLayer(ParticleSystem system, Layer layer, BlendType blendType)
        {
            EndCurveBatch();
            EndParticleBatch();
            ParticleManager.ClearLayerMasks();
            ParticleManager.UpdateLayerMasks(system.Layers, layer);
            shaderContext.UpdateLayerMaskState(system, GetSystemMaskTexture(system));
            BeginParticleBatches(blendType);
            lastBlendType = blendType;
        }
        Texture2D ResolveParticleTexture(ParticleBase particle)
        {
            var type = particle.Type;
            if (type == null) return null;
            if (type.IsTransparentPlaceholder) return transparentTexture;
            var file = particle.System.File;
            return type.ID >= ParticleType.DefaultTypeIndex ? defaultTexture : type.Image != null ? customTextures[file][type.Image.ID] : null;
        }
        Texture2D GetSystemMaskTexture(ParticleSystem system)
        {
            Texture2D texture;
            if (!maskTextures.TryGetValue(system, out texture))
            {
                texture = ResolveMaskTexture(system.File, system);
                maskTextures[system] = texture;
            }
            return texture;
        }
        Texture2D GetSystemDistortTexture(ParticleSystem system)
        {
            Texture2D texture;
            if (!distortTextures.TryGetValue(system, out texture))
            {
                texture = ResolveDistortTexture(system.File, system);
                distortTextures[system] = texture;
            }
            return texture;
        }
        void DrawParticle(Particle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                EndCurveBatch();
                EndParticleBatch();
                BeginParticleBatches(blendType);
            }
            lastBlendType = blendType;
            var type = particle.Type;
            var tex = ResolveParticleTexture(particle);
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
            var renderData = shaderContext.BuildParticleRenderData(particle, tex, rect, color, rad, origin, scale,
                spriteEffects, position, GetSystemMaskTexture(particle.System), GetSystemDistortTexture(particle.System), blendType);
            particleBatch.DrawParticle(renderData);
            if (!particle.AfterimageEffect) return;
            for (int i = 0; i < Particle.AFTERIMAGE_COUNT; ++i)
            {
                var afterImage = particle.AfterImageData[i];
                if (afterImage.alpha > 0)
                {
                    position = new Vector2(afterImage.x, afterImage.y) + center;
                    rad = MathHelper.ToRadians(afterImage.rot + 90);
                    color = new Color(color.R, color.G, color.B, (byte)(afterImage.alpha * alpha * 255f));
                    renderData = shaderContext.BuildParticleRenderData(particle, tex, rect, color, rad, origin, scale,
                        spriteEffects, position, GetSystemMaskTexture(particle.System), GetSystemDistortTexture(particle.System), blendType);
                    particleBatch.DrawParticle(renderData);
                }
                particle.AfterImageData[i] = afterImage;
            }
        }
        void DrawCurveParticle(CurveParticle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                EndCurveBatch();
                EndParticleBatch();
                BeginParticleBatches(blendType);
            }
            lastBlendType = blendType;
            var type = particle.Type;
            var tex = ResolveParticleTexture(particle);
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
            var renderData = shaderContext.BuildCurveRenderData(particle, tex, rect,
                GetSystemMaskTexture(particle.System), GetSystemDistortTexture(particle.System));
            if (renderData.RequiresShader)
            {
                DrawCurveParticleWithShader(particle, center, color, blendType, renderData);
                return;
            }
            if (tex != null) curveBatch.DrawCurve(particle.Curve, renderData, center, color);
        }
        void DrawCurveParticleWithShader(CurveParticle particle, Vector2 center, Color color, BlendType blendType,
            CurveRenderData renderData)
        {
            EndCurveBatch();
            EndParticleBatch();
            shaderContext.ApplyCurveShader(renderData);
            curveBatch.Begin(shaderContext.GetBlendState(blendType), renderData.Pass);
            curveBatch.DrawCurve(particle.Curve, renderData, center, color);
            curveBatch.End();
            shaderContext.ResetCurveShaderState();
            BeginParticleBatches(blendType);
        }
        public void Update(KeyboardState keyboard, GameTime gameTime)
        {
            FrameworkDispatcher.Update();
            var minFrameFactor = 1f;
            foreach (var instance in instances)
            {
                instance.BodyPosition = controllable.selfPos.ToCore();
                if (minFrameFactor > instance.FrameFactor) minFrameFactor = instance.FrameFactor;
            }
            controllable.Update(keyboard, minFrameFactor * ParticleSystem.FRAME_RATE_BASE / FrameRate);
            EventManager.Update(FrameRate);
            foreach (var instance in instances)
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
            foreach (var instance in instances)
            {
                var offset = (instance.ScreenOffset - instance.LogicOffset).ToXna();
                if (maxOffset.X < offset.X) maxOffset.X = offset.X;
                if (maxOffset.Y < offset.Y) maxOffset.Y = offset.Y;
            }
            shaderContext.ResetCurveShaderState();
            particleBatchBegun = false;
            curveBatchBegun = false;
            lastBlendType = BlendType.None;
            foreach (var instance in instances) ParticleManager.Draw(instance);
            EndCurveBatch();
            EndParticleBatch();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null);
            controllable.Draw(spriteBatch, characterTexture, pointTexture, slowModeTexture, maxOffset);
            spriteBatch.End();
        }
        void BeginParticleBatches(BlendType blendType)
        {
            var blendState = shaderContext.GetBlendState(blendType);
            particleBatch.Begin(blendState);
            particleBatchBegun = true;
            curveBatch.Begin(blendState, ParticleBatchPass.Textured);
            curveBatchBegun = true;
        }
        void EndParticleBatch()
        {
            if (!particleBatchBegun) return;
            particleBatch.End();
            particleBatchBegun = false;
        }
        void EndCurveBatch()
        {
            if (!curveBatchBegun) return;
            curveBatch.End();
            curveBatchBegun = false;
        }
        public void SetStatus(int i)
        {
            foreach(var instance in instances) instance.SetStatus(i);
        }
        public void SkipFrame(float targetFrame, bool replayFromStart)
        {
            if (instances == null || instances.Count == 0) return;
            foreach (var instance in instances)
            {
                instance.BodyPosition = controllable.selfPos.ToCore();
                instance.SkipFrame(targetFrame, replayFromStart, FrameRate);
            }
            UpdateCurrentFrame();
        }
    }
}
