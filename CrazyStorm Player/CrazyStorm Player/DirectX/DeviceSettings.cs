/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace CrazyStorm_Player.DirectX
{
    class DeviceSettings
    {
        public int Adapter { get; set; }
        public CreateFlags CreateFlags { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Windowed { get; set; }
    }
}
