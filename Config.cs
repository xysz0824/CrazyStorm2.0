/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm
{
    static class Config
    {
        private static int screenWidth = 640;
        private static int screenHeight = 480;
        private static bool gridAlignment = true;

        public static int ScreenWidth 
        { 
            get { return screenWidth; }
            set { screenWidth = value > 0 ? value : screenWidth; }
        }
        public static int ScreenHeight
        {
            get { return screenHeight; }
            set { screenHeight = value > 0 ? value : screenHeight; }
        }
        public static bool GridAlignment
        {
            get { return gridAlignment; }
            set { gridAlignment = value; }
        }
    }
}
