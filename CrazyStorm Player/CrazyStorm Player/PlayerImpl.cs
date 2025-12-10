/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
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

namespace CrazyStorm_Player
{
    public class PlayerImpl
    {
        SpriteBatch spriteBatch;
        BlendState substration, multiply;
        Texture2D background;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        List<Texture2D> defaultTextures;
        Dictionary<int, Texture2D> customTextures;
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
        public Vector2 CustomCenter { get; set; }
        public string ControllableImagePath { get; set; }
        public string ControllableSetting { get; set; }
        public int CurrentFrame { get; set; }
        public PlayerImpl(int width, int height, int particleMaximum, int curveParticleMaximum)
        {
            Width = width;
            Height = height;

            ParticleManager.Initialize(width, height, 50, particleMaximum, curveParticleMaximum);
        }
        public void Initialize(GraphicsDevice gd)
        {
            spriteBatch = new SpriteBatch(gd);
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
            File.ParticleSystems[SelectedParticleSystemIndex].Reset();
            ParticleManager.OnParticleDraw += (particle) => DrawParticle(spriteBatch, particle);
            ParticleManager.OnCurveParticleDraw += (particle) => DrawCurveParticle(spriteBatch, particle);
        }
        public void Dispose()
        {
            spriteBatch?.Dispose();
            background?.Dispose();
            foreach (var tex in defaultTextures) tex?.Dispose();
            foreach (var tex in customTextures.Values) tex?.Dispose();
            characterTexture?.Dispose();
            pointTexture?.Dispose();
            slowModeTexture?.Dispose();
        }
        void DrawParticle(SpriteBatch spriteBatch, Particle particle)
        {
            if (particle.Type == null) return;
            BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
            if (lastBlendType != blendType)
            {
                switch (blendType)
                {
                    case BlendType.AlphaBlend:
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
                        break;
                    case BlendType.Additive:
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap);
                        break;
                    case BlendType.Substraction:
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, substration, SamplerState.LinearWrap);
                        break;
                    case BlendType.Multiply:
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Deferred, multiply, SamplerState.LinearWrap);
                        break;
                }
            }
            lastBlendType = blendType;
            ParticleType type = particle.Type;
            Vector2 center = new Vector2(Width / 2, Height / 2) + CustomCenter;
            Vector2 imageCenter = new Vector2(type.CenterPoint.x, type.CenterPoint.y);
            float fogScale = (ParticleBase.FOG_TIME - particle.FogFrame) / 15.0f;
            Vector2 scale = new Vector2(particle.WidthScale + fogScale, particle.HeightScale + fogScale);
            Vector2 position = new Vector2(particle.PPosition.x, particle.PPosition.y) + center;
            float alpha = particle.Opacity / 100f - (ParticleBase.FOG_TIME - particle.FogFrame) / ParticleBase.FOG_TIME;
            Color color = new Color(particle.RGB.r / 255f, particle.RGB.g / 255f, particle.RGB.b / 255f, alpha);
            int frame = particle.PCurrentFrame / (type.Delay + 1) % type.Frames;
            Rectangle rect = new Rectangle((int)type.StartPoint.x + frame * type.Width, (int)type.StartPoint.y, type.Width, type.Height);
            if (type.ID >= ParticleType.DefaultTypeIndex)
            {
                spriteBatch.Draw(defaultTextures[0], position, rect, color, MathHelper.ToRadians(particle.PRotation), imageCenter, scale, SpriteEffects.None, 0);
            }
            else if (type.Image != null)
            {
                spriteBatch.Draw(customTextures[type.Image.ID], position, rect, color, MathHelper.ToRadians(particle.PRotation), imageCenter, scale, SpriteEffects.None, 0);
            }
        }
        void DrawCurveParticle(SpriteBatch spriteBatch, CurveParticle particle)
        {
            if (particle.Type == null) return;

            //TODO Curve Particle
        }
        public void Update(KeyboardState keyboard, GameTime gameTime)
        {
            controllable.Update(keyboard);
            File.SetGlobal("cx", controllable.selfPosition.X);
            File.SetGlobal("cy", controllable.selfPosition.Y);
            EventManager.CustomTypes = File.ParticleSystems[SelectedParticleSystemIndex].CustomTypes;
            File.ParticleSystems[SelectedParticleSystemIndex].Update(CurrentFrame);
            ParticleManager.CheckCollision(controllable.selfPosition.X, controllable.selfPosition.Y, controllable.selfRadius);
            ParticleManager.Update();
            EventManager.Update();
            CurrentFrame = File.ParticleSystems[SelectedParticleSystemIndex].CurrentFrame;
        }
        public void Draw(GraphicsDevice gd, GameTime gameTime)
        {
            gd.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
            if (background != null)
            {
                spriteBatch.Draw(background, Vector2.Zero, null, Color.White, 0, Vector2.Zero, backgroundScale, SpriteEffects.None, 0);
            }
            controllable.Draw(spriteBatch, characterTexture, pointTexture, slowModeTexture);
            ParticleManager.Draw();
            lastBlendType = BlendType.None;
            spriteBatch.End();
        }
    }
}
