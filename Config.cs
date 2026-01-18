/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
        string backgroundPath;
        bool gridAlignment;
        bool centerDisplay;
        int particleMaximum;
        int curveParticleMaximum;
        bool collapseLayerAxis;
        string selfImagePath;
        string selfSetting;
        string theme;
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
        public string BackgroundPath
        {
            get { return backgroundPath; }
            set 
            { 
                backgroundPath = value != null ? value : backgroundPath;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("BackgroundPath"));
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
        public int ParticleMaximum
        {
            get { return particleMaximum; }
            set
            {
                particleMaximum = value > 0 ? value : particleMaximum;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ParticleMaximum"));
                }
            }
        }
        public int CurveParticleMaximum
        {
            get { return curveParticleMaximum; }
            set
            {
                curveParticleMaximum = value > 0 ? value : curveParticleMaximum;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurveParticleMaximum"));
                }
            }
        }
        public bool CollapseLayerAxis
        {
            get { return collapseLayerAxis; }
            set
            {
                collapseLayerAxis = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CollapseLayerAxis"));
            }
        }
        public string SelfImagePath
        {
            get { return selfImagePath; }
            set
            {
                selfImagePath = value != null ? value : selfImagePath;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelfImagePath"));
            }
        }
        public string SelfSetting
        {
            get { return selfSetting; }
            set
            {
                selfSetting = value != null ? value : selfSetting;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelfSetting"));
            }
        }
        public string Theme
        {
            get { return theme; }
            set
            {
                theme = value != null ? value : theme;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Theme"));
            }
        }
        #endregion

        #region Constructor
        public Config(string filePath)
        {
            iniHelper = new IniHelper(filePath);
            if (!iniHelper.ExistINIFile())
            {
                System.IO.File.CreateText(filePath);
            }

            Load();
        }
        #endregion

        #region Public Members
        public void Load()
        {
            screenWidth = iniHelper.ReadValue("Screen", "ScreenWidth", 640);
            screenHeight = iniHelper.ReadValue("Screen", "ScreenHeight", 480);
            backgroundPath = iniHelper.ReadValue("Screen", "BackgroundPath", string.Empty);
            gridAlignment = iniHelper.ReadValue("Screen", "GridAlignment", true);
            centerDisplay = iniHelper.ReadValue("Screen", "CenterDisplay", true);
            particleMaximum = iniHelper.ReadValue("Play", "ParticleMaximum", 3000);
            curveParticleMaximum = iniHelper.ReadValue("Play", "CurveParticleMaximum", 200);
            collapseLayerAxis = iniHelper.ReadValue("Play", "CollapseLayerAxis", true);
            selfImagePath = iniHelper.ReadValue("Play", "SelfImagePath", string.Empty);
            selfSetting = iniHelper.ReadValue("Play", "SelfSetting", string.Empty);
            theme = iniHelper.ReadValue("General", "Theme", "StyleDefault");
        }
        public void Save()
        {
            iniHelper.WriteValue("Screen", "ScreenWidth", screenWidth);
            iniHelper.WriteValue("Screen", "ScreenHeight", screenHeight);
            iniHelper.WriteValue("Screen", "BackgroundPath", backgroundPath);
            iniHelper.WriteValue("Screen", "GridAlignment", gridAlignment);
            iniHelper.WriteValue("Screen", "CenterDisplay", centerDisplay);
            iniHelper.WriteValue("Play", "ParticleMaximum", particleMaximum);
            iniHelper.WriteValue("Play", "CurveParticleMaximum", curveParticleMaximum);
            iniHelper.WriteValue("Play", "CollapseLayerAxis", collapseLayerAxis);
            iniHelper.WriteValue("Play", "SelfImagePath", selfImagePath);
            iniHelper.WriteValue("Play", "SelfSetting", selfSetting);
            iniHelper.WriteValue("General", "Theme", theme);
        }
        #endregion
    }
}
