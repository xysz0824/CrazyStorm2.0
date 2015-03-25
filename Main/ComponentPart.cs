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
using System.Reflection;
using System.ComponentModel;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        static readonly string[] componentImages = new string[] 
        { "EmitterImage", "LaserImage", "MaskImage", "ReboundImage", "ForceImage" };
        #endregion

        #region Private Methods
        void AddComponent(CoreLibrary.Component component, int x, int y)
        {
            component.X = x;
            component.Y = y;
            selectedBarrage.Components.Add(aimComponent);
            selectedLayer.Components.Add(aimComponent);
            UpdateScreen();
        }
        #endregion

        #region Window EventHandler
        private void Component_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            aimBox = VisualDownwardSearch((DependencyObject)BarrageTabControl.SelectedContent, "AimBox");
            aimBox.SetValue(OpacityProperty, 1.0d);
            switch (button.Name)
            {
                case "Emitter":
                    aimComponent = new Emitter();
                    break;
                case "Laser":
                    aimComponent = new Laser();
                    break;
                case "Mask":
                    aimComponent = new Mask();
                    break;
                case "Rebound":
                    aimComponent = new Rebound();
                    break;
                case "Force":
                    aimComponent = new Force();
                    break;
            }
        }
        private void Component_MouseEnter(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            for (int i = 1; i <= componentImages.Length; ++i)
                if (image.Name == componentImages[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + "on.png", UriKind.Relative));
                    break;
                }
        }
        private void Component_MouseLeave(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            for (int i = 1; i <= componentImages.Length; ++i)
                if (image.Name == componentImages[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + ".png", UriKind.Relative));
                    break;
                }
        }
        #endregion
    }
}
