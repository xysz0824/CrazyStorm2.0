/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using CrazyStorm.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using File = CrazyStorm.Core.File;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace CrazyStorm_Player
{
    class Character
    {
        int currentFrame;
        bool slow;
        int movableWidth;
        int movableHeight;
        public Vector2 selfPosition;
        public string imagePath = string.Empty;
        public Vector2 selfStart = Vector2.Zero;
        public Vector2 selfSize = Vector2.Zero;
        public Vector2 selfCenter = Vector2.Zero;
        public int selfFrames = 0;
        public int selfDelay = 0;
        public int selfRadius = 0;
        public Character(int movableWidth, int movableHeight)
        {
            selfPosition.Y = movableHeight / 3;
            this.movableWidth = movableWidth;
            this.movableHeight = movableHeight;
        }
        public void Update(KeyboardState state)
        {
            Vector2 direction = Vector2.Zero;
            if (state.IsKeyDown(Keys.Left))
            {
                direction.X = -1;
            }
            else if (state.IsKeyDown(Keys.Right))
            {
                direction.X = 1;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                direction.Y = -1;
            }
            else if (state.IsKeyDown(Keys.Down))
            {
                direction.Y = 1;
            }
            if (direction.LengthSquared() != 0) direction.Normalize();
            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
            {
                slow = true;
                selfPosition += direction * 2.0f;
            }
            else
            {
                slow = false;
                selfPosition += direction * 4.0f;
            }
            selfPosition.X = MathHelper.Clamp(selfPosition.X, -movableWidth / 2, movableWidth / 2);
            selfPosition.Y = MathHelper.Clamp(selfPosition.Y, -movableHeight / 2, movableHeight / 2);            
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D character, Texture2D point, Texture2D slowMode)
        {
            Vector2 center = new Vector2(movableWidth / 2, movableHeight / 2);
            Vector2 position = selfPosition + center;
            Color color = new Color(1f, 1f, 1f, 1f);
            Rectangle rect;
            if (character != null)
            {
                int frame = currentFrame / (selfDelay + 1) % selfFrames;
                rect = new Rectangle((int)selfStart.X + frame * (int)selfSize.X, (int)selfStart.Y, (int)selfSize.X, (int)selfSize.Y);
                spriteBatch.Draw(character, position, rect, color, 0, this.selfCenter, new Vector2(1, 1), SpriteEffects.None, 0);
            }
            Vector2 selfCenter = new Vector2(7, 7);
            rect = new Rectangle(0, 0, 16, 16);
            spriteBatch.Draw(point, position, rect, color, 0, selfCenter, new Vector2(1, 1), SpriteEffects.None, 0);
            if (slow)
            {
                selfCenter = new Vector2(31, 31);
                float rotation = currentFrame / 30.0f;
                rect = new Rectangle(0, 0, 64, 64);
                spriteBatch.Draw(slowMode, position, rect, color, rotation, selfCenter, new Vector2(1, 1), SpriteEffects.None, 0);
            }
            ++currentFrame;
        }
    }
    class Player : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BlendState substration, multiply;
        bool hasBackground;
        Texture2D backgroundTexture;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        List<Texture2D> defaultTextures;
        List<ParticleType> defaultParticleTypes;
        Dictionary<int, Texture2D> customTextures;
        File file;
        int selectedParticleSystemIndex;
        Vector2 customCenter;
        Texture2D characterTexture;
        Texture2D pointTexture;
        Texture2D slowModeTexture;
        Character mainCharacter;
        BlendType lastBlendType = BlendType.None;
        public Player() : base()
        {
            Window.Title = VersionInfo.AppTitle;
            hasBackground = System.IO.File.Exists(Environment.GetCommandLineArgs()[2]);
            selectedParticleSystemIndex = Int32.Parse(Environment.GetCommandLineArgs()[3]);

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = false;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = Int32.Parse(Environment.GetCommandLineArgs()[4]);
            graphics.PreferredBackBufferHeight = Int32.Parse(Environment.GetCommandLineArgs()[5]);
            graphics.IsFullScreen = !bool.Parse(Environment.GetCommandLineArgs()[8]);

            int particleMaximum = Int32.Parse(Environment.GetCommandLineArgs()[6]);
            int curveParticleMaximum = Int32.Parse(Environment.GetCommandLineArgs()[7]);
            ParticleManager.Initialize(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, 50, particleMaximum, curveParticleMaximum);

            customCenter = new Vector2(Int32.Parse(Environment.GetCommandLineArgs()[9]), Int32.Parse(Environment.GetCommandLineArgs()[10]));

            mainCharacter = new Character(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            mainCharacter.imagePath = Environment.GetCommandLineArgs()[11];
            string[] selfSetting = Environment.GetCommandLineArgs()[12].Split(',');
            if (selfSetting.Length == 9)
            {
                mainCharacter.selfStart = new Vector2(Int32.Parse(selfSetting[0]), Int32.Parse(selfSetting[1]));
                mainCharacter.selfSize = new Vector2(Int32.Parse(selfSetting[2]), Int32.Parse(selfSetting[3]));
                mainCharacter.selfCenter = new Vector2(Int32.Parse(selfSetting[4]), Int32.Parse(selfSetting[5]));
                mainCharacter.selfFrames = Int32.Parse(selfSetting[6]);
                mainCharacter.selfDelay = Int32.Parse(selfSetting[7]);
                mainCharacter.selfRadius = Int32.Parse(selfSetting[8]);
            }
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
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

            ParticleManager.OnParticleDraw += (particle) =>
            {
                if (particle.Type == null)
                    return;

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
                Vector2 center = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2) + customCenter;
                Vector2 imageCenter = new Vector2(type.CenterPoint.x, type.CenterPoint.y);
                float fogScale = (ParticleBase.FOG_TIME - particle.FogFrame) / 15.0f;
                Vector2 scale = new Vector2(particle.WidthScale + fogScale, particle.HeightScale + fogScale);
                Vector2 position = new Vector2(particle.PPosition.x, particle.PPosition.y) + center;
                float alpha = particle.Opacity / 100f - (ParticleBase.FOG_TIME - particle.FogFrame) / ParticleBase.FOG_TIME;
                Color color = new Color(particle.RGB.r / 255f, particle.RGB.g / 255f, particle.RGB.b / 255f, alpha);
                int frame = particle.PCurrentFrame / (type.Delay + 1) % type.Frames;
                Rectangle rect = new Rectangle((int)type.StartPoint.x + frame * type.Width, (int)type.StartPoint.y, type.Width, type.Height);
                if (type.ID >= ParticleType.DefaultTypeIndex)
                    spriteBatch.Draw(defaultTextures[0], position, rect, color, MathHelper.ToRadians(particle.PRotation), imageCenter, scale, SpriteEffects.None, 0);
                else if (type.Image != null)
                    spriteBatch.Draw(customTextures[type.Image.ID], position, rect, color, MathHelper.ToRadians(particle.PRotation), imageCenter, scale, SpriteEffects.None, 0);
            };
            ParticleManager.OnCurveParticleDraw += (curveParticle) =>
            {
                if (curveParticle.Type == null)
                    return;

                //TODO Curve Particle
            };
            //Load background
            if (hasBackground)
            {
                using (var file = new FileStream(Environment.GetCommandLineArgs()[2], FileMode.Open))
                {
                    backgroundTexture = Texture2D.FromStream(GraphicsDevice, file);
                    float scale1 = graphics.PreferredBackBufferWidth / (float)backgroundTexture.Width;
                    float scale2 = graphics.PreferredBackBufferHeight / (float)backgroundTexture.Height;
                    if (scale1 < scale2)
                    {
                        backgroundScale = new Vector2(scale1, scale1);
                        backgroundPos.Y = (graphics.PreferredBackBufferHeight - scale1 * backgroundTexture.Height) / 2;
                    }
                    else
                    {
                        backgroundScale = new Vector2(scale2, scale2);
                        backgroundPos.X = (graphics.PreferredBackBufferWidth - scale2 * backgroundTexture.Width) / 2;
                    }
                }
            }
            //Load default textures and types
            var assembly = Assembly.GetExecutingAssembly();
            defaultTextures = new List<Texture2D>();
            Environment.CurrentDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            Stream defaultTexturesStream = assembly.GetManifestResourceStream("CrazyStorm_Player.barrages.png");
            defaultTextures.Add(Texture2D.FromStream(GraphicsDevice, defaultTexturesStream));
            Stream defaultParticleTypesStream = assembly.GetManifestResourceStream("CrazyStorm_Player.set.txt");
            using (StreamReader reader = new StreamReader(defaultParticleTypesStream))
            {
                defaultParticleTypes = new List<ParticleType>();
                ParticleType.LoadDefaultTypes(reader, defaultParticleTypes);
                EventManager.DefaultTypes = defaultParticleTypes;
            }
            //Load main character texture
            if (!StringUtil.IsNullOrWhiteSpace(mainCharacter.imagePath))
            {
                using (var file = new FileStream(mainCharacter.imagePath, FileMode.Open))
                {
                    characterTexture = Texture2D.FromStream(GraphicsDevice, file);
                }
            }
            Stream pointTextureStream = assembly.GetManifestResourceStream("CrazyStorm_Player.point.png");
            pointTexture = Texture2D.FromStream(GraphicsDevice, pointTextureStream);
            Stream slowModeTextureStream = assembly.GetManifestResourceStream("CrazyStorm_Player.ring.png");
            slowModeTexture = Texture2D.FromStream(GraphicsDevice, slowModeTextureStream);
            //Load play file
            using (FileStream stream = new FileStream(Environment.GetCommandLineArgs()[1], FileMode.Open))
            {
                var reader = new BinaryReader(stream);
                //Play file use UTF-8 encoding
                string header = PlayDataHelper.ReadString(reader);
                if (header == "BG")
                {
                    float version = float.Parse(PlayDataHelper.ReadString(reader));
                    if (version >= VersionInfo.BaseVersion)
                    {
                        file = new File(false);
                        file.LoadPlayData(reader, version);
                        RebuildObjectReference(file);
                    }
                    else
                        throw new NotSupportedException();
                }
            }
            //Load custom textures and types
            Environment.CurrentDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[1]);
            customTextures = new Dictionary<int, Texture2D>();
            foreach (var image in file.Images)
            {
                using (var file = new FileStream(image.RelatviePath, FileMode.Open))
                {
                    customTextures[image.ID] = Texture2D.FromStream(GraphicsDevice, file);
                }
            }
            file.ParticleSystems[selectedParticleSystemIndex].Reset();
        }
        protected override void Update(GameTime gameTime)
        {
            mainCharacter.Update(Keyboard.GetState());
            file.SetGlobal("cx", mainCharacter.selfPosition.X);
            file.SetGlobal("cy", mainCharacter.selfPosition.Y);
            EventManager.CustomTypes = file.ParticleSystems[selectedParticleSystemIndex].CustomTypes;
            file.ParticleSystems[selectedParticleSystemIndex].Update();
            ParticleManager.CheckCollision(mainCharacter.selfPosition.X, mainCharacter.selfPosition.Y, mainCharacter.selfRadius);
            ParticleManager.Update();
            EventManager.Update();
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);
            if (backgroundTexture != null)
            {
                spriteBatch.Draw(backgroundTexture, Vector2.Zero, null, Color.White, 0, Vector2.Zero, backgroundScale, SpriteEffects.None, 0);
            }
            mainCharacter.Draw(spriteBatch, characterTexture, pointTexture, slowModeTexture);
            ParticleManager.Draw();
            lastBlendType = BlendType.None;
            spriteBatch.End();
        }
        void RebuildObjectReference(File file)
        {
            foreach (var particleSystem in file.ParticleSystems)
            {
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                    customType.RebuildReferenceFromCollection(file.Images);
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                particleTypes.AddRange(defaultParticleTypes);
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Collect all components
                var components = new List<Component>();
                foreach (var layer in particleSystem.Layers)
                    components.AddRange(layer.Components);
                //Rebuild components reference
                foreach (var component in components)
                {
                    component.RebuildReferenceFromCollection(components);
                    //Rebuild particles reference
                    if (component is Emitter)
                        (component as Emitter).InitialTemplate.RebuildReferenceFromCollection(particleTypes);
                }
            }
        }
    }
}
