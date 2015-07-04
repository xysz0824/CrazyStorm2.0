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
using CrazyStorm.Core;

namespace CrazyStorm
{
    /// <summary>
    /// BrrageSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ParticleSetting : Window
    {
        #region Private Members
        List<string> colorList = new List<string>{ "无", "红", "紫", "蓝", "青", "绿", "黄", "橙", "灰" };
        File file;
        ParticleSystem selectedParticle;
        ParticleType selectedType;
        TabControl particleTabControl;
        #endregion

        #region Constructor
        public ParticleSetting(File file, ParticleSystem particle, TabControl particleTabControl)
        {
            this.file = file;
            this.selectedParticle = particle;
            this.particleTabControl = particleTabControl;
            InitializeComponent();
            InitializeDataBinding();
        }
        #endregion

        #region Private Methods
        void InitializeDataBinding()
        {
            ParticleName.DataContext = selectedParticle;
            TypeList.ItemsSource = selectedParticle.CustomType;
            file.UpdateResource();
            ImageCombo.ItemsSource = file.Images;
        }
        void UpdateColor()
        {
            foreach (Label item in ColorPanel.Children)
            {
                if (selectedType != null && colorList[(int)selectedType.Color] == (string)item.Content)
                    item.BorderThickness = new Thickness(1);
                else
                    item.BorderThickness = new Thickness(0);
            }
        }
        #endregion

        #region Window EventHandlers
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ParticleName.Text))
            {
                MessageBox.Show((string)FindResource("ParticleNameCanNotBeEmpty"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            selectedParticle.Name = ParticleName.Text;
            var item = particleTabControl.SelectedItem as TabItem;
            item.Header = ParticleName.Text;
            MessageBox.Show((string)FindResource("ParticleNameSuccessfullyChanged"), (string)FindResource("TipTitle"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedType = e.AddedItems[0] as ParticleType;
                Setting.IsEnabled = true;
                Setting.DataContext = selectedType;
            }
            else
            {
                selectedType = null;
                Setting.IsEnabled = false;
                Setting.DataContext = null;
            }
            UpdateColor();
        }
        private void AddNewType_Click(object sender, RoutedEventArgs e)
        {
            selectedParticle.CustomType.Add(new ParticleType());
        }
        private void ColorPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var color = e.OriginalSource as TextBlock;
            if (color != null && selectedType != null)
            {
                selectedType.Color = (ParticleColor)colorList.FindIndex(s => s == color.Text);
                UpdateColor();
            }
        }
        private void DeleteType_Click(object sender, RoutedEventArgs e)
        {
            selectedParticle.CustomType.Remove(selectedType);
        }
        #endregion
    }
}
