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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        string[] componentNames = new string[] { "EmitterImage", "LaserImage", "MaskImage", "ReboundImage", "ForceImage" };
        #endregion

        #region Window EventHandler
        private void Component_MouseEnter(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            for (int i = 1; i <= componentNames.Length; ++i)
                if (image.Name == componentNames[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + "on.png", UriKind.Relative));
                    break;
                }
        }
        private void Component_MouseLeave(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            for (int i = 1; i <= componentNames.Length; ++i)
                if (image.Name == componentNames[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + ".png", UriKind.Relative));
                    break;
                }
        }
        #endregion
    }
}
