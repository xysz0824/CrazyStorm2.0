/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public static class VersionInfo
    {
        public const string AppName = "Crazy Storm";
        public const string Version = "2.0";
        public const string PlayVersion = "0.91";
        public static string AppTitle { get { return AppName + " " + Version; } }
    }
}
