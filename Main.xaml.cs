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
using System.ComponentModel;

namespace CrazyStorm
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        File file;
        Barrage selectedBarrage;
        Layer selectedLayer;
        string status = String.Empty;
        #endregion

        #region Public Members
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }
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
        private void Component_MouseEnter(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            string[] condition = new string[] { "EmitterImage", "LaserImage", "MaskImage", "ReboundImage", "ForceImage" };
            for (int i = 1; i <= condition.Length; ++i)
                if (image.Name == condition[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + "on.png", UriKind.Relative));
                    break;
                }
        }
        private void Component_MouseLeave(object sender, MouseEventArgs e)
        {
            var image = sender as Image;
            string[] condition = new string[] { "EmitterImage", "LaserImage", "MaskImage", "ReboundImage", "ForceImage" };
            for (int i = 1; i <= condition.Length; ++i)
                if (image.Name == condition[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + ".png", UriKind.Relative));
                    break;
                }
        }
        #endregion
    }
}
