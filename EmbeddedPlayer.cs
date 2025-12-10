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

namespace CrazyStorm
{
    public class EmbeddedPlayer : WpfGame
    {
        IGraphicsDeviceService graphics;
        WpfKeyboard keyboard;
        
        public PlayerImpl PlayerImpl { get; set; }
        public bool Pause { get; set; }
        protected override void Initialize()
        {
            graphics = new WpfGraphicsDeviceService(this);

            keyboard = new WpfKeyboard(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            PlayerImpl.Initialize(GraphicsDevice);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PlayerImpl.Dispose();
        }

        protected override void Update(GameTime time)
        {
            if (Pause) return;
            PlayerImpl.Update(keyboard.GetState(), time);
        }

        protected override void Draw(GameTime time)
        {
            PlayerImpl.Draw(GraphicsDevice, time);
        }
    }
}
