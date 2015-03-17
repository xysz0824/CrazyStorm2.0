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
    public class Config : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        int screenWidth = 640;
        int screenHeight = 480;
        string imagePath = string.Empty;
        bool gridAlignment = true;
        bool centerDisplay = true;
        #endregion

        #region Public Members
        public int ScreenWidth 
        { 
            get { return screenWidth; }
            set 
            {
                screenWidth = value > 0 ? value : screenWidth;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ScreenWidth"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ScreenWidthOver2"));
                }
            }
        }
        public int ScreenWidthOver2 { get { return screenWidth / 2; } }
        public int ScreenHeight
        {
            get { return screenHeight; }
            set 
            { 
                screenHeight = value > 0 ? value : screenHeight;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ScreenHeight"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ScreenHeightOver2"));
                }
            }
        }
        public int ScreenHeightOver2 { get { return screenHeight / 2; } }
        public string ImagePath
        {
            get { return imagePath; }
            set 
            { 
                imagePath = value != null ? value : imagePath;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ImagePath"));
            }
        }
        public bool GridAlignment
        {
            get { return gridAlignment; }
            set 
            { 
                gridAlignment = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("GridAlignment"));
            }
        }
        public bool CenterDisplay
        {
            get { return centerDisplay; }
            set 
            { 
                centerDisplay = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterDisplay"));
            }
        }
        #endregion
    }
}
