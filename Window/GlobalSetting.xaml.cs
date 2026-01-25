/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class GlobalSetting : Window
    {
        #region Private Members
        Config config;
        #endregion

        #region Public Members
        public event Action OnButtonClick;
        #endregion

        #region Constructor
        public GlobalSetting(Config config)
        {
            this.config = config;
            InitializeComponent();
            InitializeSetting();
        }
        #endregion

        #region Private Methods
        void InitializeSetting()
        {
            Setting.DataContext = config;
            if (config.GridAlignment) GridOpen.IsChecked = true;
            else GridClose.IsChecked = true;
            if (config.CenterDisplay) CenterOpen.IsChecked = true;
            else CenterClose.IsChecked = true;
            InitializeTypeLibraryList();
        }
        void InitializeTypeLibraryList()
        {
            var files = Directory.GetFiles("typelibrary", "*.txt");
            foreach (var file in files)
            {
                if (System.IO.File.Exists($"typelibrary\\{System.IO.Path.GetFileNameWithoutExtension(file)}.png"))
                {
                    var fileName = System.IO.Path.GetFileName(file);
                    var item = new ComboBoxItem();
                    item.Content = fileName;
                    TypeLibraryList.Items.Add(item);
                    if (config.TypeLibraryPath == fileName)
                    {
                        TypeLibraryList.SelectedItem = item;
                    }
                }
            }
        }
        #endregion

        #region Window EventHandlers
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                open.Filter = (string)FindResource("BackGroundImageTypeStr");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BackgroundPath.Text = open.FileName;
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int width, height;
            if (Int32.TryParse(ScreenWidth.Text, out width) && Int32.TryParse(ScreenHeight.Text, out height))
            {
                config.ScreenWidth = width;
                config.ScreenHeight = height;
                config.BackgroundPath = BackgroundPath.Text;
                if (GridOpen.IsChecked.HasValue && GridOpen.IsChecked.Value == true)
                {
                    config.GridAlignment = true;
                }
                else
                {
                    config.GridAlignment = false;
                }
                if (CenterOpen.IsChecked.HasValue && CenterOpen.IsChecked.Value == true)
                {
                    config.CenterDisplay = true;
                }
                else
                {
                    config.CenterDisplay = false;
                }
                if (TypeLibraryList.SelectedIndex > 0)
                {
                    config.TypeLibraryPath = (TypeLibraryList.SelectedItem as ComboBoxItem).Content as string;
                }
                else
                {
                    config.TypeLibraryPath = string.Empty;
                }
                ParticleType.DefaultTypes.Clear();
                ParticleType.LoadDefaultTypes(config.TypeLibraryPath);
                if (OnButtonClick != null) OnButtonClick();
                config.Save();
                this.Close();
            }
            else
                MessageBox.Show((string)FindResource("ValueInvalidStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        #endregion
    }
}
