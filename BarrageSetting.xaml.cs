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
        List<string> colorList = new List<string>{ "无", "红", "紫", "蓝", "青", "绿", "黄", "橙", "灰" };
        File file;
        Barrage barrage;
        BarrageType selectedType;
        #endregion

        #region Constructor
        public BarrageSetting(File file, Barrage barrage)
        {
            this.file = file;
            this.barrage = barrage;
            InitializeComponent();
            InitializeDataBinding();
        }
        #endregion

        #region Private Methods
        void InitializeDataBinding()
        {
            TypeList.ItemsSource = barrage.CustomType;
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
        private void TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedType = e.AddedItems[0] as BarrageType;
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
            barrage.CustomType.Add(new BarrageType());
        }
        private void ColorPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var color = e.OriginalSource as TextBlock;
            if (color != null && selectedType != null)
            {
                selectedType.Color = (BarrageColor)colorList.FindIndex(s => s == color.Text);
                UpdateColor();
            }
        }
        private void DeleteType_Click(object sender, RoutedEventArgs e)
        {
            barrage.CustomType.Remove(selectedType);
        }
        #endregion
    }
}
