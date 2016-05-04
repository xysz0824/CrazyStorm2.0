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
        public const string Version = "0.9";
        public const string Extension = ".bgp";
        public static string AppTitle { get { return AppName + " " + Version; } }
    }
}
