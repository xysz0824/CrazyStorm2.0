/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm_Player
{
    public static class VersionInfo
    {
        public const string AppName = "Crazy Storm Player";
        public const float BaseVersion = 0.9f;
        public const float Version = 0.91f;
        public static string AppTitle { get { return AppName + " " + Version; } }
    }
}
