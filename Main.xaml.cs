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
        Barrage selectedBarrage;
        Layer selectedLayer;
        string status = String.Empty;
        #endregion

        #region Constructor
        public Main()
        {
            InitializeComponent();
            InitializeSystem();
            status = "已就绪";
        }
        #endregion

        #region Private Methods
        void InitializeSystem()
        {
            LoadConfig();
            CreateNewFile();
            InitializeScene();
            InitializeLayer();
            InitializeStatus();
        }
        void LoadConfig()
        {

        }
        void CreateNewFile()
        {
            file = new File("Untitled");
            Title = AppInfo.AppTitle + " - " + file.FileName;
        }
        void InitializeScene()
        {
            selectedBarrage = file.Barrages.First();
            TabItem tabItem = new TabItem();
            tabItem.Header = selectedBarrage.Name;
            Canvas canvas = new Canvas()
            {
                Width = Config.ScreenWidth,
                Height = Config.ScreenHeight,
                Background = new SolidColorBrush(Color.FromRgb(0,0,0))
            };
            tabItem.Content = canvas;
            SceneTabControl.Items.Add(tabItem);
            ComponentTree.ItemsSource = selectedBarrage.Components;
        }
        void InitializeLayer()
        {
            LayerTree.ItemsSource = selectedBarrage.Layers;
            LayerAxis.ItemsSource = selectedBarrage.Layers;
            CopyLayer.IsEnabled = false;
            DeleteLayer.IsEnabled = false;
            SetLayer.IsEnabled = false;
        }
        void InitializeStatus()
        {
            var item = StatusBar.Items[0] as StatusBarItem;
            item.DataContext = this;
        }
        #endregion

        #region Window EventHandler
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
        #endregion
    }
}
