/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm_Player.DirectX;
using SlimDX;
using SlimDX.Direct3D9;
using System.IO;
using System.Drawing;
using CrazyStorm.Core;
using File = CrazyStorm.Core.File;
using Vector2 = SlimDX.Vector2;
using System.Reflection;

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
        public void Update(SlimDX.DirectInput.KeyboardState state)
        {
            Vector2 direction = Vector2.Zero;
            if (state.IsPressed(SlimDX.DirectInput.Key.LeftArrow))
            {
                direction.X = -1;
            }
            else if (state.IsPressed(SlimDX.DirectInput.Key.RightArrow))
            {
                direction.X = 1;
            }
            if (state.IsPressed(SlimDX.DirectInput.Key.UpArrow))
            {
                direction.Y = -1;
            }
            else if (state.IsPressed(SlimDX.DirectInput.Key.DownArrow))
            {
                direction.Y = 1;
            }
            direction.Normalize();
            if (state.IsPressed(SlimDX.DirectInput.Key.LeftShift) || state.IsPressed(SlimDX.DirectInput.Key.RightShift))
            {
                slow = true;
                selfPosition += direction * 2.0f;
            }
            else
            {
                slow = false;
                selfPosition += direction * 4.0f;
            }
            selfPosition.X = MathHelper.Clamp(selfPosition.X, movableWidth / 2, -movableWidth / 2);
            selfPosition.Y = MathHelper.Clamp(selfPosition.Y, movableHeight / 2, -movableHeight / 2);            
        }
        public void Draw(Sprite sprite, Texture character, Texture point, Texture slowMode)
        {
            Vector2 offset = new Vector2(movableWidth / 2, movableHeight / 2);
            Vector2 position = selfPosition + offset - selfCenter;
            Color4 color = new Color4(1, 1, 1, 1);
            Rectangle rect;
            if (character != null)
            {
                sprite.Transform = Matrix.Transformation2D(selfCenter, 0, new Vector2(1, 1), selfCenter, 0, position);
                int frame = currentFrame / (selfDelay + 1) % selfFrames;
                rect = new Rectangle((int)selfStart.X + frame * (int)selfSize.X, (int)selfStart.Y, (int)selfSize.X, (int)selfSize.Y);
                sprite.Draw(character, rect, color);
            }
            Vector2 center = new Vector2(7, 7);
            position = selfPosition + offset - center;
            sprite.Transform = Matrix.Transformation2D(center, 0, new Vector2(1, 1), center, 0, position);
            rect = new Rectangle(0, 0, 16, 16);
            sprite.Draw(point, rect, color);
            if (slow)
            {
                center = new Vector2(31, 31);
                position = selfPosition + offset - center;
                float rotation = currentFrame / 30.0f;
                sprite.Transform = Matrix.Transformation2D(center, 0, new Vector2(1, 1), center, rotation, position);
                rect = new Rectangle(0, 0, 64, 64);
                sprite.Draw(slowMode, rect, color);
            }
            ++currentFrame;
        }
    }
    class Player : DirectXFramework
    {
        bool hasBackground;
        Texture backgroundTexture;
        Vector2 backgroundScale;
        Vector2 backgroundPos;
        List<Texture> defaultTextures;
        List<ParticleType> defaultParticleTypes;
        Dictionary<int, Texture> customTextures;
        File file;
        int selectedParticleSystemIndex;
        Vector2 customCenter;
        Texture characterTexture;
        Texture pointTexture;
        Texture slowModeTexture;
        Character mainCharacter;
        BlendType lastBlendType = BlendType.None;
        protected override void OnInitialize()
        {
            WindowTitle = VersionInfo.AppTitle;
            hasBackground = System.IO.File.Exists(Environment.GetCommandLineArgs()[2]);
            selectedParticleSystemIndex = Int32.Parse(Environment.GetCommandLineArgs()[3]);
            WindowWidth = Int32.Parse(Environment.GetCommandLineArgs()[4]);
            WindowHeight = Int32.Parse(Environment.GetCommandLineArgs()[5]);
            int particleMaximum = Int32.Parse(Environment.GetCommandLineArgs()[6]);
            int curveParticleMaximum = Int32.Parse(Environment.GetCommandLineArgs()[7]);
            ParticleManager.Initialize(WindowWidth, WindowHeight, 50, particleMaximum, curveParticleMaximum);
            Windowed = bool.Parse(Environment.GetCommandLineArgs()[8]);
            customCenter = new Vector2(Int32.Parse(Environment.GetCommandLineArgs()[9]), Int32.Parse(Environment.GetCommandLineArgs()[10]));

            mainCharacter = new Character(WindowWidth, WindowHeight);
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
            ParticleManager.OnParticleDraw += (particle) =>
            {
                if (particle.Type == null)
                    return;

                BlendType blendType = (BlendType)(9 - particle.RenderOrder % 10);
                if (lastBlendType != blendType)
                {
                    switch (blendType)
                    {
                        case BlendType.Additive:
                            Sprite.End();
                            Sprite.Begin(SpriteFlags.AlphaBlend);
                            Device.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
                            Device.SetRenderState(RenderState.DestinationBlend, (int)Blend.One);
                            break;
                        case BlendType.Substraction:
                            Sprite.End();
                            Sprite.Begin(SpriteFlags.AlphaBlend);
                            Device.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
                            Device.SetRenderState(RenderState.DestinationBlend, (int)Blend.One);
                            Device.SetRenderState(RenderState.BlendOperation, (int)BlendOperation.Subtract);
                            break;
                        case BlendType.Multiply:
                            Sprite.End();
                            Sprite.Begin(SpriteFlags.AlphaBlend);
                            Device.SetRenderState(RenderState.SourceBlend, (int)Blend.Zero);
                            Device.SetRenderState(RenderState.DestinationBlend, (int)Blend.SourceColor);
                            break;
                    }
                }
                lastBlendType = blendType;
                ParticleType type = particle.Type;
                Vector2 center = new Vector2(WindowWidth / 2, WindowHeight / 2) + customCenter;
                Vector2 imageCenter = new Vector2(type.CenterPoint.x, type.CenterPoint.y);
                float fogScale = (10 - particle.FogFrame) / 15.0f;
                Vector2 scale = new Vector2(particle.WidthScale + fogScale, particle.HeightScale + fogScale);
                Vector2 position = new Vector2(particle.PPosition.x, particle.PPosition.y) + center - imageCenter;
                Sprite.Transform = Matrix.Transformation2D(imageCenter, 0, scale, imageCenter, (float)MathHelper.DegToRad(particle.PRotation), position);
                Color4 color = new Color4((particle.Opacity - (10 - particle.FogFrame) * 10) / 100, particle.RGB.r / 255, particle.RGB.g / 255, particle.RGB.b / 255);
                int frame = particle.PCurrentFrame / (type.Delay + 1) % type.Frames;
                Rectangle rect = new Rectangle((int)type.StartPoint.x + frame * type.Width, (int)type.StartPoint.y, type.Width, type.Height);
                if (type.ID >= ParticleType.DefaultTypeIndex)
                    Sprite.Draw(defaultTextures[0], rect, color);
                else if (type.Image != null)
                    Sprite.Draw(customTextures[type.Image.ID], rect, color);
            };
            ParticleManager.OnCurveParticleDraw += (curveParticle) =>
            {
                if (curveParticle.Type == null)
                    return;

                //TODO Curve Particle
            };
        }
        protected override void OnLoad()
        {
            //Load background
            if (hasBackground)
            {
                var info = new ImageInformation();
                backgroundTexture = Texture.FromFile(Device, Environment.GetCommandLineArgs()[2], 
                    D3DX.DefaultNonPowerOf2, D3DX.DefaultNonPowerOf2, 1, Usage.None, Format.Unknown, 
                    Pool.Managed, Filter.None, Filter.None, 0, out info);
                float scale1 = WindowWidth / (float)info.Width;
                float scale2 = WindowHeight / (float)info.Height;
                if (scale1 < scale2)
                {
                    backgroundScale = new Vector2(scale1, scale1);
                    backgroundPos.Y = (WindowHeight - scale1 * info.Height) / 2;
                }
                else
                {
                    backgroundScale = new Vector2(scale2, scale2);
                    backgroundPos.X = (WindowWidth - scale2 * info.Width) / 2;
                }
            }
            //Load default textures and types
            defaultTextures = new List<Texture>();
            Environment.CurrentDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            Stream defaultTexturesStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.barrages.png");
            defaultTextures.Add(Texture.FromStream(Device, defaultTexturesStream, Usage.None, Pool.Managed));
            Stream defaultParticleTypesStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.set.txt");
            using (StreamReader reader = new StreamReader(defaultParticleTypesStream))
            {
                defaultParticleTypes = new List<ParticleType>();
                ParticleType.LoadDefaultTypes(reader, defaultParticleTypes);
                EventManager.DefaultTypes = defaultParticleTypes;
            }
            //Load main character texture
            if (!StringUtil.IsNullOrWhiteSpace(mainCharacter.imagePath))
            {
                characterTexture = Texture.FromFile(Device, mainCharacter.imagePath, Usage.None, Pool.Managed);
            }
            Stream pointTextureStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.point.png");
            pointTexture = Texture.FromStream(Device, pointTextureStream, Usage.None, Pool.Managed);
            Stream slowModeTextureStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CrazyStorm_Player.ring.png");
            slowModeTexture = Texture.FromStream(Device, slowModeTextureStream, Usage.None, Pool.Managed);
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
            customTextures = new Dictionary<int, Texture>();
            foreach (var image in file.Images)
                customTextures[image.ID] = Texture.FromFile(Device, image.RelatviePath, Usage.None, Pool.Managed);

            file.ParticleSystems[selectedParticleSystemIndex].Reset();
        }
        protected override void OnUpdate()
        {
            mainCharacter.Update(KeyboardState);
            file.SetGlobal("cx", mainCharacter.selfPosition.X);
            file.SetGlobal("cy", mainCharacter.selfPosition.Y);
            EventManager.CustomTypes = file.ParticleSystems[selectedParticleSystemIndex].CustomTypes;
            file.ParticleSystems[selectedParticleSystemIndex].Update();
            ParticleManager.Update();
            EventManager.Update();
        }
        protected override void OnDraw()
        {
            ClearScreen(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1, 0);
            Sprite.Begin(SpriteFlags.AlphaBlend);
            if (backgroundTexture != null)
            {
                Sprite.Transform = Matrix.Transformation2D(Vector2.Zero, 0, backgroundScale, Vector2.Zero, 0, backgroundPos);
                Sprite.Draw(backgroundTexture, Color.White);
            }
            mainCharacter.Draw(Sprite, characterTexture, pointTexture, slowModeTexture);
            ParticleManager.Draw();
            lastBlendType = BlendType.None;
            Sprite.End();
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
