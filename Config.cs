/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CrazyStorm
{
    class Config
    {
        #region Private Members
        int screenWidth = 640;
        int screenHeight = 480;
        bool gridAlignment = true;
        #endregion

        #region Public Members
        public int ScreenWidth 
        { 
            get { return screenWidth; }
            set { screenWidth = value > 0 ? value : screenWidth; }
        }
        public int ScreenWidthOver2 { get { return screenWidth / 2; } }
        public int ScreenHeight
        {
            get { return screenHeight; }
            set { screenHeight = value > 0 ? value : screenHeight; }
        }
        public int ScreenHeightOver2 { get { return screenHeight / 2; } }
        public bool GridAlignment
        {
            get { return gridAlignment; }
            set { gridAlignment = value; }
        }
        #endregion
    }
}
