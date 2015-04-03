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
        //Mainly implement operation to data.
        #region Private Methods
        void CreateAimedComponent(string name)
        {
            switch (name)
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
        #endregion
        //Implement control and interaction with UI.
        #region Window EventHandler
        private void Component_Click(object sender, RoutedEventArgs e)
        {
            //Create corresponding component according to different button.
            var button = sender as Button;
            aimRect = VisualDownwardSearch((DependencyObject)BarrageTabControl.SelectedContent, "AimBox");
            aimRect.SetValue(OpacityProperty, 1.0d);
            CreateAimedComponent(button.Name);
        }
        private void Component_MouseEnter(object sender, MouseEventArgs e)
        {
            //Light up button when mouse enter.
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
            //Reset button when mouse leave.
            var image = sender as Image;
            for (int i = 1; i <= componentImages.Length; ++i)
                if (image.Name == componentImages[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + ".png", UriKind.Relative));
                    break;
                }
        }
        private void ComponentList_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            //Select pointed component when mouse left-button down. 
            var textBlock = sender as TextBlock;
            foreach (var component in selectedBarrage.Components)
                if (component == textBlock.DataContext)
                {
                    SelectComponent(component);
                    break;
                }
        }
        #endregion
    }
}
