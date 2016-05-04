/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace CrazyStorm_Player.Framework
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
