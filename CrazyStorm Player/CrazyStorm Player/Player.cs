/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm_Player.DirectX;
using SlimDX;
using SlimDX.Direct3D9;
using System.IO;
using CrazyStorm_Player.CrazyStorm;

namespace CrazyStorm_Player
{
    class Player : DirectXFramework
    {
        List<Texture> defaultTextures;
        List<ParticleType> defaultParticleTypes;
        CrazyStorm.File file;
        protected override void OnInitialize()
        {
            WindowTitle = VersionInfo.AppTitle;
            ParticleManager.Initialize(WindowWidth, WindowHeight, 10000, 1000);
            ParticleManager.OnParticleDraw += (particle) =>
            {
                Vector2 center = new Vector2(particle.Type.CenterPoint.x, particle.Type.CenterPoint.y);
                Vector2 scale = new Vector2(particle.WidthScale, particle.HeightScale);
                Vector2 position = new Vector2(particle.PPosition.x, particle.PPosition.y);
                Sprite.Transform = Matrix.Transformation2D(center, 0, scale, center,
                    (float)MathHelper.DegToRad(particle.PRotation), position);
                Color4 color = new Color4(particle.Opacity, particle.RGB.r, particle.RGB.g, particle.RGB.b);
                if (particle.TypeID >= 1000)
                    Sprite.Draw(defaultTextures[0], color);
            };
            ParticleManager.OnCurveParticleDraw += (curveParticle) =>
            {
                //TODO
            };
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            defaultTextures = new List<Texture>();
            defaultTextures.Add(Texture.FromFile(Device, "Resources/Default/barrages.png", Usage.None, Pool.Managed));
            using (StreamReader reader = new StreamReader("Resources/Default/set.txt"))
            {
                defaultParticleTypes = new List<ParticleType>();
                ParticleType.LoadDefaultTypes(reader, defaultParticleTypes);
            }
            using (FileStream stream = new FileStream("a.bg", FileMode.Open))
            {
                var reader = new BinaryReader(stream);
                //Play file use UTF-8 encoding
                string header = PlayDataHelper.ReadString(reader);
                if (header == "BG")
                {
                    string version = PlayDataHelper.ReadString(reader);
                    file = new CrazyStorm.File();
                    file.LoadPlayData(reader);
                    RebuildObjectReference(file);
                }
            }
        }
        protected override void OnUnLoad()
        {
            base.OnUnLoad();
        }
        protected override void OnUpdate()
        {

        }
        protected override void OnDraw()
        {
            ClearScreen(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0.3f, 0.3f, 0.3f), 1, 0);
            Sprite.Begin(SpriteFlags.AlphaBlend);
            ParticleManager.Draw();
            Sprite.End();
        }
        void RebuildObjectReference(CrazyStorm.File file)
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
                        (component as Emitter).Template.RebuildReferenceFromCollection(particleTypes);
                }
            }
        }
    }
}
