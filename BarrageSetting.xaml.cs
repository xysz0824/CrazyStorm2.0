/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    /// <summary>
    /// BrrageSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BarrageSetting : Window
    {
        #region Private Members
        private Barrage barrage;
        #endregion

        #region Constructor
        public BarrageSetting(Barrage barrage)
        {
            this.barrage = barrage;
            InitializeComponent();
        }
        #endregion
    }
}
