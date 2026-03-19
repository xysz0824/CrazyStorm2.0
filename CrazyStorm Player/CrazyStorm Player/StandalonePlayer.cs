using CrazyStorm.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Taskbar;

namespace CrazyStorm_Player
{
    class StandalonePlayer : Game
    {
        GraphicsDeviceManager graphics;
        PlayerImpl playerImpl;
        public StandalonePlayer() : base()
        {
            Window.Title = VersionInfo.AppTitle;

            var paths = Environment.GetCommandLineArgs()[1];
            var backgroundPath = Environment.GetCommandLineArgs()[2];
            var selectedParticleSystemIndex = int.Parse(Environment.GetCommandLineArgs()[3]);
            var width = int.Parse(Environment.GetCommandLineArgs()[4]);
            var height = int.Parse(Environment.GetCommandLineArgs()[5]);
            var frameRate = int.Parse(Environment.GetCommandLineArgs()[6]);
            var particleMaximum = int.Parse(Environment.GetCommandLineArgs()[7]);
            var curveParticleMaximum = int.Parse(Environment.GetCommandLineArgs()[8]);
            var windowed = bool.Parse(Environment.GetCommandLineArgs()[9]);
            var controllableImagePath = Environment.GetCommandLineArgs()[10];
            var controllableSetting = Environment.GetCommandLineArgs()[11];
            var typeLibraryPath = Environment.GetCommandLineArgs()[12];
            var frameOrientation = int.Parse(Environment.GetCommandLineArgs()[13]);

            playerImpl = new PlayerImpl(width, height, frameRate, particleMaximum, curveParticleMaximum);
            playerImpl.TypeLibraryPath = typeLibraryPath;
            playerImpl.FrameOrientation = (FrameOrientation)frameOrientation;
            playerImpl.BackgroundPath = backgroundPath;
            playerImpl.SelectedParticleSystemIndex = selectedParticleSystemIndex;
            playerImpl.ControllableImagePath = controllableImagePath;
            playerImpl.ControllableSetting = controllableSetting;

            ParticleType.LoadDefaultTypes(typeLibraryPath);

            var split = paths.Split(',');
            foreach (var path in split)
            {
                var file = new File();
                file.LoadPlayFile(path, VersionInfo.BaseVersion);
                playerImpl.Files.Add(file);
            }

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = false;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.IsFullScreen = !windowed;
            IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = false;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / frameRate);
        }
        protected override void LoadContent()
        {
            playerImpl.Initialize(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            playerImpl.Update(Keyboard.GetState(), gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            playerImpl.Draw(GraphicsDevice, gameTime);
        }
    }
}
