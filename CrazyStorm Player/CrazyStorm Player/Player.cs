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
        protected override void OnInitialize()
        {
            WindowTitle = VersionInfo.AppTitle;
            CrazyStorm.File file = new CrazyStorm.File();
            using (FileStream stream = new FileStream("a.bg", FileMode.Open))
            {
                var reader = new BinaryReader(stream);
                //Play file using UTF-8 encoding
                string header = PlayDataHelper.ReadString(reader);
                if (header == "BG")
                {
                    string version = PlayDataHelper.ReadString(reader);
                    file.LoadPlayData(reader);
                }
            }
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
