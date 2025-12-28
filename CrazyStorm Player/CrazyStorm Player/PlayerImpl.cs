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
using System.Media;

namespace CrazyStorm_Player
{
    public class PlayerImpl
    {
        const int PARTICLE_PRESERVED_DIST = 50;
        const int CURVE_PRESERVED_DIST = 100;

        SpriteBatch spriteBatch;
        CurveBatch curveBatch;
        BlendState substration, multiply;
        Texture2D background;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        List<Texture2D> defaultTextures;
        Dictionary<int, Texture2D> customTextures;
        Dictionary<string, SoundEffect> sounds;
        Texture2D characterTexture;
        Texture2D pointTexture;
        Texture2D slowModeTexture;
        Controllable controllable;
        BlendType lastBlendType = BlendType.None;

        public string ResourceDirectory { get; set; } 
        public File File { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string BackgroundPath { get; set; }
        public int SelectedParticleSystemIndex { get; set; }
        public string ControllableImagePath { get; set; }
        public string ControllableSetting { get; set; }
        public int CurrentFrame { get; set; }
        public PlayerImpl(int width, int height, int particleMaximum, int curveParticleMaximum)
        {
            Width = width;
            Height = height;

            EventManager.Initialize();
            ParticleManager.Initialize(width, height, PARTICLE_PRESERVED_DIST, CURVE_PRESERVED_DIST, 
                particleMaximum, curveParticleMaximum);
        }
        public void Initialize(GraphicsDevice gd)
        {
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
                controllable.selfStart = new Vector2(Int32.Parse(setting[0]), Int32.Parse(setting[1]));
                controllable.selfSize = new Vector2(Int32.Parse(setting[2]), Int32.Parse(setting[3]));
                controllable.selfCenter = new Vector2(Int32.Parse(setting[4]), Int32.Parse(setting[5]));
                controllable.selfFrames = Int32.Parse(setting[6]);
                controllable.selfDelay = Int32.Parse(setting[7]);
                controllable.selfRadius = Int32.Parse(setting[8]);
            }
            //Load background texture
            if (!string.IsNullOrWhiteSpace(BackgroundPath))
            {
                using (var file = new System.IO.FileStream(BackgroundPath, System.IO.FileMode.Open))
                {
                    background = Texture2D.FromStream(gd, file);
                    float scale1 = Width / (float)background.Width;
                    float scale2 = Height / (float)background.Height;
                    if (scale1 < scale2)
                    {
                        backgroundScale = new Vector2(scale1, scale1);
                        backgroundPos.Y = (Height - scale1 * background.Height) / 2;
                    }
                    else
                    {
                        backgroundScale = new Vector2(scale2, scale2);
                        backgroundPos.X = (Width - scale2 * background.Width) / 2;
                    }
                }
            }
            //Load default textures and types
            var assembly = Assembly.GetExecutingAssembly();
            defaultTextures = new List<Texture2D>();
            Environment.CurrentDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            Stream defaultTexturesStream = assembly.GetManifestResourceStream("CrazyStorm_Player.barrages.png");
            defaultTextures.Add(Texture2D.FromStream(gd, defaultTexturesStream));
            //Load main character texture
            if (!StringUtil.IsNullOrWhiteSpace(controllable.imagePath))
            {
                using (var file = new FileStream(controllable.imagePath, FileMode.Open))
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
                using (var file = new FileStream(image.RelatviePath, FileMode.Open))
                {
                    customTextures[image.ID] = Texture2D.FromStream(gd, file);
                }
            }
            FrameworkDispatcher.Update();
            sounds = new Dictionary<string, SoundEffect>();
            File.ParticleSystems[SelectedParticleSystemIndex].Reset();
            EventManager.OnSoundPlay += PlaySound;
            ParticleManager.OnParticleDraw += (particle) => DrawParticle(spriteBatch, particle);
            ParticleManager.OnCurveParticleDraw += (particle) => DrawCurveParticle(spriteBatch, curveBatch, particle);
        }
        public void Dispose()
        {
            spriteBatch?.Dispose();
            background?.Dispose();
            foreach (var tex in defaultTextures) tex?.Dispose();
            defaultTextures.Clear();
            foreach (var tex in customTextures.Values) tex?.Dispose();
            customTextures.Clear();
            foreach (var sound in sounds.Values) sound?.Dispose();
            sounds.Clear();
            characterTexture?.Dispose();
            pointTexture?.Dispose();
            slowModeTexture?.Dispose();
        }
        void PlaySound(string path, float volume)
        {
            if (!sounds.ContainsKey(path))
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    sounds[path] = SoundEffect.FromStream(stream);
                }
            }
            sounds[path].Play(volume / 100f, 0f, 0f);
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
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
                        break;
                    case BlendType.Additive:
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap);
                        break;
                    case BlendType.Substraction:
                        spriteBatch.Begin(SpriteSortMode.Deferred, substration, SamplerState.LinearWrap);
                        break;
                    case BlendType.Multiply:
                        spriteBatch.Begin(SpriteSortMode.Deferred, multiply, SamplerState.LinearWrap);
                        break;
                }
            }
            lastBlendType = blendType;
            ParticleType type = particle.Type;
            Vector2 center = new Vector2(Width / 2, Height / 2);
            Vector2 imageCenter = new Vector2(type.CenterPoint.x, type.CenterPoint.y);
            float fogScale = (ParticleBase.FOG_TIME - particle.FogFrame) / 15.0f;
            Vector2 scale = new Vector2(particle.WidthScale + fogScale, particle.HeightScale + fogScale);
            Vector2 position = new Vector2(particle.PPosition.x, particle.PPosition.y) + center;
            float alpha = particle.Opacity / 100f - (ParticleBase.FOG_TIME - particle.FogFrame) / ParticleBase.FOG_TIME;
            Color color = new Color(particle.RGB.r / 255f, particle.RGB.g / 255f, particle.RGB.b / 255f, alpha);
            int frame = particle.PCurrentFrame / (type.Delay + 1) % type.Frames;
            Rectangle rect = new Rectangle((int)type.StartPoint.x + frame * type.Width, (int)type.StartPoint.y, type.Width, type.Height);
            var tex = type.ID >= ParticleType.DefaultTypeIndex ? defaultTextures[0] : type.Image != null ? customTextures[type.Image.ID] : null;
            if (tex != null)
            {
                var rad = MathHelper.ToRadians(particle.PRotation);
                spriteBatch.Draw(tex, position, rect, color, rad, imageCenter, scale, SpriteEffects.None, 0);
                if (!particle.AfterimageEffect) return;
                for (int i = 0; i < Particle.AFTERIMAGE_COUNT; ++i)
                {
                    var afterImage = particle.AfterImageData[i];
                    if (afterImage.alpha > 0)
                    {
                        position = new Vector2(afterImage.x, afterImage.y) + center;
                        rad = MathHelper.ToRadians(afterImage.rot);
                        color.A = (byte)(afterImage.alpha * alpha * 255f);
                        spriteBatch.Draw(tex, position, rect, color, rad, imageCenter, scale, SpriteEffects.None, 0);
                    }
                    particle.AfterImageData[i] = afterImage;
                }
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
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
                        curveBatch.Begin(BlendState.NonPremultiplied);
                        break;
                    case BlendType.Additive:
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap);
                        curveBatch.Begin(BlendState.Additive);
                        break;
                    case BlendType.Substraction:
                        spriteBatch.Begin(SpriteSortMode.Deferred, substration, SamplerState.LinearWrap);
                        curveBatch.Begin(substration);
                        break;
                    case BlendType.Multiply:
                        spriteBatch.Begin(SpriteSortMode.Deferred, multiply, SamplerState.LinearWrap);
                        curveBatch.Begin(multiply);
                        break;
                }
            }
            lastBlendType = blendType;
            ParticleType type = particle.Type;
            Vector2 center = new Vector2(Width / 2, Height / 2);
            float alpha = particle.Opacity / 100f - (ParticleBase.FOG_TIME - particle.FogFrame) / ParticleBase.FOG_TIME;
            Color color = new Color(particle.RGB.r / 255f, particle.RGB.g / 255f, particle.RGB.b / 255f, alpha);
            int frame = particle.PCurrentFrame / (type.Delay + 1) % type.Frames;
            Rectangle rect = new Rectangle((int)type.StartPoint.x + frame * type.Width, (int)type.StartPoint.y, type.Width, type.Height);
            var tex = type.ID >= ParticleType.DefaultTypeIndex ? defaultTextures[0] : type.Image != null ? customTextures[type.Image.ID] : null;
            if (tex != null) curveBatch.Draw(particle.Curve, tex, rect, center, color);
        }
        public void Update(KeyboardState keyboard, GameTime gameTime)
        {
            FrameworkDispatcher.Update();
            controllable.Update(keyboard);
            File.SetGlobal(SpecialVariableType.BodyPositionX, controllable.selfPos.X);
            File.SetGlobal(SpecialVariableType.BodyPositionY, controllable.selfPos.Y);
            EventManager.CustomTypes = File.ParticleSystems[SelectedParticleSystemIndex].CustomTypes;
            EventManager.Sounds = File.Sounds;
            EventManager.Update();
            File.ParticleSystems[SelectedParticleSystemIndex].Update(CurrentFrame);
            var collidedCount = 0;
            var particles = ParticleManager.CheckCollision(controllable.selfPosLast.X, controllable.selfPosLast.Y,
                controllable.selfPos.X, controllable.selfPos.Y, controllable.selfRadius, out collidedCount);
            for (int i = 0; i < collidedCount; ++i) particles[i].Die();
            ParticleManager.Update();
            CurrentFrame = File.ParticleSystems[SelectedParticleSystemIndex].CurrentFrame;
        }
        public void Draw(GraphicsDevice gd, GameTime gameTime)
        {
            gd.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
            curveBatch.Begin(BlendState.NonPremultiplied);
            if (background != null)
            {
                spriteBatch.Draw(background, Vector2.Zero, null, Color.White, 0, Vector2.Zero, backgroundScale, SpriteEffects.None, 0);
            }
            controllable.Draw(spriteBatch, characterTexture, pointTexture, slowModeTexture);
            ParticleManager.Draw();
            lastBlendType = BlendType.None;
            curveBatch.End();
            spriteBatch.End();
        }
    }
}
