/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm_Player.Framework;
using SlimDX;
using SlimDX.Direct3D9;

namespace CrazyStorm_Player
{
    class Player : DirectXFramework
    {
        protected override void OnInitialize()
        {
            WindowTitle = VersionInfo.AppTitle;
        }
        protected override void OnLoad()
        {

        }
        protected override void OnUnLoad()
        {

        }
        protected override void OnUpdate()
        {

        }
        protected override void OnDraw()
        {
            ClearScreen(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0.3f, 0.3f, 0.3f), 1, 0);
        }
    }
}
