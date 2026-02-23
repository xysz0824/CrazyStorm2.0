/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrazyStorm.Core;
using CrazyStorm_Player;
using System.Windows;

namespace CrazyStorm
{
    public class EmbeddedPlayer : WpfGame
    {
        WpfGraphicsDeviceService graphics;
        WpfKeyboard keyboard;
        
        public PlayerImpl PlayerImpl { get; set; }
        public bool Pause { get; set; }
        public bool HasError { get; private set; }
        protected override void Initialize()
        {
            graphics = new WpfGraphicsDeviceService(this);

            keyboard = new WpfKeyboard(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            try
            {
                PlayerImpl.Initialize(GraphicsDevice);
            }
            catch (PoolOverflowException ex)
            {
                MessageBox.Show(ex.Message, "!", MessageBoxButton.OK, MessageBoxImage.Error);
                Pause = true;
                HasError = true;
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PlayerImpl.Dispose();
        }

        protected override void Update(GameTime time)
        {
            if (Pause) return;
            try
            {
                PlayerImpl.Update(keyboard.GetState(), time);
            }
            catch (PoolOverflowException ex)
            {
                MessageBox.Show(ex.Message, "!", MessageBoxButton.OK, MessageBoxImage.Error);
                Pause = true;
                HasError = true;
            }
        }

        protected override void Draw(GameTime time)
        {
            PlayerImpl.Draw(GraphicsDevice, time);
        }
    }
}
