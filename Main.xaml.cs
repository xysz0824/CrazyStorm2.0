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
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        #region Private Members
        File file;
        #endregion

        #region Constructor
        public Main()
        {
            InitializeComponent();
            InitializeSystem();
        }
        #endregion

        #region Private Methods
        void InitializeSystem()
        {
            file = new File("Untitled");
            Title = AppInfo.AppTitle + " - " + file.FileName;
            CreateSceneTab(file.Barrages.First());
            //Initialize layer
            LayerTree.ItemsSource = file.Barrages.First().Layers;
        }
        void CreateSceneTab(Barrage barrage)
        {
            TabItem tabItem = new TabItem();
            Binding binding = new Binding();
            binding.Source = barrage;
            binding.Path = new PropertyPath("Name");
            tabItem.SetBinding(TabItem.HeaderProperty, binding);
            Canvas canvas = new Canvas()
            {
                Width = 640,
                Height = 480,
                Background = new SolidColorBrush(Color.FromRgb(0,0,0))
            };
            tabItem.Content = canvas;
            SceneTabControl.Items.Add(tabItem);
        }
        #endregion

        #region EventHandler
        private void EmitterImage_MouseEnter(object sender, MouseEventArgs e)
        {
            EmitterImage.Source = new BitmapImage(new Uri(@"Images\button1on.png", UriKind.Relative));
        }

        private void EmitterImage_MouseLeave(object sender, MouseEventArgs e)
        {
            EmitterImage.Source = new BitmapImage(new Uri(@"Images\button1.png", UriKind.Relative));
        }

        private void LaserImage_MouseEnter(object sender, MouseEventArgs e)
        {
            LaserImage.Source = new BitmapImage(new Uri(@"Images\button2on.png", UriKind.Relative));
        }

        private void LaserImage_MouseLeave(object sender, MouseEventArgs e)
        {
            LaserImage.Source = new BitmapImage(new Uri(@"Images\button2.png", UriKind.Relative));
        }

        private void MaskImage_MouseEnter(object sender, MouseEventArgs e)
        {
            MaskImage.Source = new BitmapImage(new Uri(@"Images\button3on.png", UriKind.Relative));
        }

        private void MaskImage_MouseLeave(object sender, MouseEventArgs e)
        {
            MaskImage.Source = new BitmapImage(new Uri(@"Images\button3.png", UriKind.Relative));
        }

        private void ReboundImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ReboundImage.Source = new BitmapImage(new Uri(@"Images\button4on.png", UriKind.Relative));
        }

        private void ReboundImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ReboundImage.Source = new BitmapImage(new Uri(@"Images\button4.png", UriKind.Relative));
        }

        private void ForceImage_MouseEnter(object sender, MouseEventArgs e)
        {
            ForceImage.Source = new BitmapImage(new Uri(@"Images\button5on.png", UriKind.Relative));
        }

        private void ForceImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ForceImage.Source = new BitmapImage(new Uri(@"Images\button5.png", UriKind.Relative));
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TimeScale.Width = ActualWidth - 200;
        }
        #endregion
    }
}
