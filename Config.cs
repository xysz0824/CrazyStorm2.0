/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public class Config : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        IniHelper iniHelper;
        int screenWidth;
        int screenHeight;
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
        public Vector2 GridSize { get { return new Vector2(32, 32); } }
        public int GridWidth { get { return 32; } }
        public int GridHeight { get { return 32; } }
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

        #region Constructor
        public Config(string filePath)
        {
            iniHelper = new IniHelper(filePath);
            if (!iniHelper.ExistINIFile())
                throw new FileNotFoundException("Check Config.ini existence.");

            Load();
        }
        #endregion

        #region Public Members
        public void Load()
        {
            screenWidth = iniHelper.ReadValue("Screen", "ScreenWidth", 640);
            screenHeight = iniHelper.ReadValue("Screen", "ScreenHeight", 480);
            imagePath = iniHelper.ReadValue("Screen", "ImagePath", string.Empty);
            gridAlignment = iniHelper.ReadValue("Screen", "GridAlignment", true);
            centerDisplay = iniHelper.ReadValue("Screen", "CenterDisplay", true);
        }
        public void Save()
        {
            iniHelper.WriteValue("Screen", "ScreenWidth", screenWidth);
            iniHelper.WriteValue("Screen", "ScreenHeight", screenHeight);
            iniHelper.WriteValue("Screen", "ImagePath", imagePath);
            iniHelper.WriteValue("Screen", "GridAlignment", gridAlignment);
            iniHelper.WriteValue("Screen", "CenterDisplay", centerDisplay);
        }
        #endregion
    }
}
