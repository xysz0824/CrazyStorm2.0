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

            var path = Environment.GetCommandLineArgs()[1];
            var backgroundPath = Environment.GetCommandLineArgs()[2];
            var selectedParticleSystemIndex = Int32.Parse(Environment.GetCommandLineArgs()[3]);
            var width = Int32.Parse(Environment.GetCommandLineArgs()[4]);
            var height = Int32.Parse(Environment.GetCommandLineArgs()[5]);
            var particleMaximum = Int32.Parse(Environment.GetCommandLineArgs()[6]);
            var curveParticleMaximum = Int32.Parse(Environment.GetCommandLineArgs()[7]);
            var windowed = bool.Parse(Environment.GetCommandLineArgs()[8]);
            var customCenterX = Int32.Parse(Environment.GetCommandLineArgs()[9]);
            var customCenterY = Int32.Parse(Environment.GetCommandLineArgs()[10]);
            var controllableImagePath = Environment.GetCommandLineArgs()[11];
            var controllableSetting = Environment.GetCommandLineArgs()[12];

            playerImpl = new PlayerImpl(width, height, particleMaximum, curveParticleMaximum);
            playerImpl.ResourceDirectory = Environment.CurrentDirectory;
            playerImpl.File = new File();
            playerImpl.File.LoadPlayFile(path, VersionInfo.BaseVersion);
            playerImpl.BackgroundPath = backgroundPath;
            playerImpl.SelectedParticleSystemIndex = selectedParticleSystemIndex;
            playerImpl.CustomCenter = new Microsoft.Xna.Framework.Vector2(customCenterX, customCenterY);
            playerImpl.ControllableImagePath = controllableImagePath;
            playerImpl.ControllableSetting = controllableSetting;

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = false;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.IsFullScreen = !windowed;
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
