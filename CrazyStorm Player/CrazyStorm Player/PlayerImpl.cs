/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using Microsoft.Xna.Framework;
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
using File = CrazyStorm.Core.File;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Blend = Microsoft.Xna.Framework.Graphics.Blend;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SamplerState = Microsoft.Xna.Framework.Graphics.SamplerState;
using Microsoft.Xna.Framework.Audio;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;

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
        EffectParameter shaderMaskType;
        EffectParameter shaderMaskShape;
        EffectParameter shaderMaskRotate;
        EffectParameter shaderMaskCount;
        EffectParameter shaderRenderCenter;
        SpriteBatch spriteBatch;
        CurveBatch curveBatch;
        BlendState substration, multiply;
        Texture2D background;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        Texture2D defaultTexture;
        Dictionary<int, Texture2D> customTextures;
        Dictionary<string, SoundEffect> sounds;
        Texture2D characterTexture;
        Texture2D pointTexture;
        Texture2D slowModeTexture;
        Controllable controllable;
        BlendType lastBlendType = BlendType.None;

        public string ResourceDirectory { get; set; } 
        public string TypeLibraryPath { get; set; }
        public FrameOrientation FrameOrientation { get; set; }
        public File File { get; set; }
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
            var shaderFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.shader.mgfxo");
            using (var ms = new MemoryStream())
            {
                shaderFile.CopyTo(ms);
                shader = new Effect(gd, ms.ToArray());
                shaderMaskSize = shader.Parameters["MaskSize"];
                shaderMaskPosition = shader.Parameters["MaskPosition"];
                shaderMaskType = shader.Parameters["MaskType"];
                shaderMaskShape = shader.Parameters["MaskShape"];
                shaderMaskRotate = shader.Parameters["MaskRotate"];
                shaderMaskCount = shader.Parameters["MaskCount"];
                shaderRenderCenter = shader.Parameters["RenderCenter"];
            }
            spriteBatch = new SpriteBatch(gd);
            curveBatch = new CurveBatch(gd);
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
            Stream slowModeTextureStream = assembly.GetManifestResourceStream("CrazyStorm_Player.ring.png");
            slowModeTexture = Texture2D.FromStream(gd, slowModeTextureStream);
            //Load custom textures and types
            Environment.CurrentDirectory = ResourceDirectory;
            customTextures = new Dictionary<int, Texture2D>();
            foreach (var image in File.Images)
            {
                if (!System.IO.File.Exists(image.RelatviePath))
                {
                    customTextures[image.ID] = null;
                    continue;
                }
                using (var file = new FileStream(image.RelatviePath, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        customTextures[image.ID] = Texture2D.FromStream(gd, file);
                    }
                    catch
                    {
                        customTextures[image.ID] = null;
                    }
                }
            }
            Environment.CurrentDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            FrameworkDispatcher.Update();
            File.BodyPosition = controllable.selfPos.ToCore();
            File.ParticleSystems[SelectedParticleSystemIndex].Reset(true);
            sounds = new Dictionary<string, SoundEffect>();
            ForceField.OnForceImpactBody += ForceImpactBody;
            EventManager.OnSoundPlay += PlaySound;
            ParticleManager.OnParticleDraw += (particle) => DrawParticle(spriteBatch, particle);
            ParticleManager.OnCurveParticleDraw += (particle) => DrawCurveParticle(spriteBatch, curveBatch, particle);
        }
        public void Dispose()
        {
            spriteBatch?.Dispose();
            background?.Dispose();
            defaultTexture?.Dispose();
            foreach (var tex in customTextures.Values) tex?.Dispose();
            customTextures.Clear();
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
            sounds[path].Play(0.5f, 0f, 0f);
        }
        void DrawParticle(SpriteBatch spriteBatch, Particle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                spriteBatch.End();
                switch (blendType)
                {
                    case BlendType.AlphaBlend:
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, shader);
                        break;
                    case BlendType.Additive:
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, null, null, shader);
                        break;
                    case BlendType.Substraction:
                        spriteBatch.Begin(SpriteSortMode.Deferred, substration, SamplerState.LinearWrap, null, null, shader);
                        break;
                    case BlendType.Multiply:
                        spriteBatch.Begin(SpriteSortMode.Deferred, multiply, SamplerState.LinearWrap, null, null, shader);
                        break;
                }
            }
            lastBlendType = blendType;
            var type = particle.Type;
            var tex = type.ID >= ParticleType.DefaultTypeIndex ? defaultTexture : type.Image != null ? customTextures[type.Image.ID] : null;
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
            var rad = MathHelper.ToRadians(particle.PRotation);
            spriteBatch.Draw(tex, position, rect, color, rad, origin, scale, spriteEffects, 0);
            if (!particle.AfterimageEffect) return;
            for (int i = 0; i < Particle.AFTERIMAGE_COUNT; ++i)
            {
                var afterImage = particle.AfterImageData[i];
                if (afterImage.alpha > 0)
                {
                    position = new Vector2(afterImage.x, afterImage.y) + center;
                    rad = MathHelper.ToRadians(afterImage.rot);
                    color.A = (byte)(afterImage.alpha * alpha * 255f);
                    spriteBatch.Draw(tex, position, rect, color, rad, origin, scale, spriteEffects, 0);
                }
                particle.AfterImageData[i] = afterImage;
            }
        }
        void DrawCurveParticle(SpriteBatch spriteBatch, CurveBatch curveBatch, CurveParticle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                curveBatch.End();
                spriteBatch.End();
                switch (blendType)
                {
                    case BlendType.AlphaBlend:
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, shader);
                        curveBatch.Begin(BlendState.NonPremultiplied);
                        break;
                    case BlendType.Additive:
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, null, null, shader);
                        curveBatch.Begin(BlendState.Additive);
                        break;
                    case BlendType.Substraction:
                        spriteBatch.Begin(SpriteSortMode.Deferred, substration, SamplerState.LinearWrap, null, null, shader);
                        curveBatch.Begin(substration);
                        break;
                    case BlendType.Multiply:
                        spriteBatch.Begin(SpriteSortMode.Deferred, multiply, SamplerState.LinearWrap, null, null, shader);
                        curveBatch.Begin(multiply);
                        break;
                }
            }
            lastBlendType = blendType;
            var type = particle.Type;
            var tex = type.ID >= ParticleType.DefaultTypeIndex ? defaultTexture : type.Image != null ? customTextures[type.Image.ID] : null;
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
            if (tex != null) curveBatch.Draw(particle.Curve, tex, rect, center, color);
        }
        public void Update(KeyboardState keyboard, GameTime gameTime)
        {
            FrameworkDispatcher.Update();
            var selectedParticle = File.ParticleSystems[SelectedParticleSystemIndex];
            controllable.Update(keyboard, selectedParticle.FrameFactor * ParticleSystem.FRAME_RATE_BASE / FrameRate);
            File.BodyPosition = controllable.selfPos.ToCore();
            EventManager.CustomTypes = selectedParticle.CustomTypes;
            EventManager.Sounds = File.Sounds;
            EventManager.TypeSoundMap = selectedParticle.TypeSoundMap;
            EventManager.Update(FrameRate);
            selectedParticle.Update(FrameRate, CurrentFrame);
            ParticleManager.UpdateLayerMasks(selectedParticle.Layers);
            shaderMaskCount.SetValue(ParticleManager.MaskCount);
            shaderMaskSize.SetValue(ParticleManager.MaskSizeArray);
            shaderMaskPosition.SetValue(ParticleManager.MaskPositionArray);
            shaderMaskShape.SetValue(ParticleManager.MaskShapeArray);
            shaderMaskType.SetValue(ParticleManager.MaskTypeArray);
            shaderMaskRotate.SetValue(ParticleManager.MaskRotateArray);
            shaderRenderCenter.SetValue(new Vector2(Width / 2, Height / 2));
            var collidedCount = 0;
            CrazyStorm.Core.Vector2 newPos = default;
            var particles = ParticleManager.CheckCollision(false, controllable.selfPosLast.ToCore(),
                controllable.selfPos.ToCore(), controllable.selfRadius, out collidedCount, out newPos);
            for (int i = 0; i < collidedCount; ++i) particles[i].Die();
            controllable.selfPos = newPos.ToXna();
            ParticleManager.Update(FrameRate);
            CurrentFrame = selectedParticle.CurrentFrame;
        }
        public void Draw(GraphicsDevice gd, GameTime gameTime)
        {
            gd.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, shader);
            curveBatch.Begin(BlendState.NonPremultiplied);
            if (background != null)
            {
                spriteBatch.Draw(background, backgroundPos, null, Color.White, 0, Vector2.Zero, backgroundScale, SpriteEffects.None, 0);
            }
            var particle = File.ParticleSystems[SelectedParticleSystemIndex];
            controllable.Draw(spriteBatch, characterTexture, pointTexture, slowModeTexture, particle.ScreenOffset.ToXna());
            ParticleManager.Draw(particle.OrderType);
            lastBlendType = BlendType.None;
            curveBatch.End();
            spriteBatch.End();
        }
    }
}
