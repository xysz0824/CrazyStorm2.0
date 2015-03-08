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
            file = new File("Untitled");
            InitializeComponent();
            InitializeSystem();
            status = "已就绪";
        }
        #endregion

        #region Private Methods
        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        void InitializeSystem()
        {
            LoadConfig();
            InitializeFile();
            InitializeScene();
            InitializeLayer();
            InitializeStatus();
        }
        void LoadConfig()
        {

        }
        void InitializeFile()
        {
            Title = AppInfo.AppTitle + " - " + file.FileName;
            ImageList.ItemsSource = file.Images;
            SoundList.ItemsSource = file.Sounds;
            ScriptList.ItemsSource = file.Scripts;
        }
        void InitializeScene()
        {
            selectedBarrage = file.Barrages.First();
            AddNewBarrageTab(selectedBarrage);
            ComponentList.ItemsSource = selectedBarrage.Components;
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
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "PNG图像(*.png)|*.png";
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var image = new Resource(open.SafeFileName, open.FileName);
                file.Images.Add(image);
            }
        }
        private void AddSound_Click(object sender, RoutedEventArgs e)
        {

        }
        private void AddScript_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
