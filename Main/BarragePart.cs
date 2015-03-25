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
    public partial class Main
    {
        #region Private Members
        Barrage selectedBarrage;
        DependencyObject aimBox;
        Component aimComponent;
        DependencyObject selectingBox;
        bool selecting;
        static readonly string[] componentNames = new string[] { "", "Emitter", "Laser", "Mask", "Rebound", "Force" };
        #endregion

        #region Private Methods
        void UpdateScreen()
        {
            var content = (DependencyObject)BarrageTabControl.SelectedContent;
            var canvas = VisualDownwardSearch(content, "ComponentLayer") as Canvas;
            if (canvas != null)
            {
                canvas.Children.Clear();
                var itemTemplate = FindResource("ComponentItem") as DataTemplate;
                foreach (var layer in selectedBarrage.Layers)
                    foreach (var component in layer.Components)
                    {
                        var item = itemTemplate.LoadContent();
                        var frame = VisualDownwardSearch(item, "Frame") as Label;
                        frame.DataContext = layer;
                        var icon = VisualDownwardSearch(item, "Icon") as Image;
                        for (int i = 0; i < componentNames.Length; ++i)
                            if (componentNames[i] == component.GetType().Name)
                            {
                                icon.Source = new BitmapImage(new Uri(@"Images/button" + i + ".png", UriKind.Relative));
                                break;
                            }

                        item.SetValue(Canvas.LeftProperty, (double)component.X);
                        item.SetValue(Canvas.TopProperty, (double)component.Y);
                        canvas.Children.Add(item as UIElement);
                    }
            }
        }
        void CreateNewBarrage()
        {
            var barrage = new Barrage("New Barrage");
            file.Barrages.Add(barrage);
            selectedBarrage = barrage;
            AddNewBarrageTab(barrage);
        }
        void AddNewBarrageTab(Barrage barrage)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = barrage.Name;
            tabItem.Content = BarrageTabControl.ItemTemplate.LoadContent();
            BarrageTabControl.Items.Add(tabItem);
            BarrageTabControl.SelectedItem = tabItem;
        }
        void DeleteSeletedBarrage()
        {
            if (file.Barrages.Count > 1)
            {
                TabItem selected = null;
                foreach (TabItem item in BarrageTabControl.Items)
                    if (selectedBarrage.Name == (string)item.Header)
                    {
                        selected = item;
                        break;
                    }

                file.Barrages.Remove(selectedBarrage);
                BarrageTabControl.Items.Remove(selected);
            }
            else
                MessageBox.Show((string)FindResource("CanNotDeleteAllBarrage"), (string)FindResource("TipTitle"), 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySeletedBarrage()
        {
            //TODO : Copy function.
        }
        void OpenSelectedBarrageSetting()
        {
            BarrageSetting window = new BarrageSetting(file, selectedBarrage, BarrageTabControl);
            window.Owner = this;
            window.ShowDialog();
        }
        #endregion

        #region Window EventHandler
        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(sender as IInputElement);
            int x = (int)point.X;
            int y = (int)point.Y;
            if (x >= 0 && x < config.ScreenWidth && y >= 0 && y < config.ScreenHeight)
            {
                if (aimBox != null)
                {
                    if (config.GridAlignment)
                    {
                        aimBox.SetValue(Canvas.LeftProperty, (double)((x / 32) * 32));
                        aimBox.SetValue(Canvas.TopProperty, (double)((y / 32) * 32));
                    }
                    else if (x <= config.ScreenWidth - 32 && y <= config.ScreenHeight - 32)
                    {
                        aimBox.SetValue(Canvas.LeftProperty, (double)x);
                        aimBox.SetValue(Canvas.TopProperty, (double)y);
                    }
                }
                if (selectingBox != null)
                {
                    var width =  x - (double)selectingBox.GetValue(Canvas.LeftProperty);
                    if (width <= 0)
                        width = 0;
                    var height = y - (double)selectingBox.GetValue(Canvas.TopProperty);
                    if (height <= 0)
                        height = 0;
                    selectingBox.SetValue(WidthProperty, width);
                    selectingBox.SetValue(HeightProperty, height);
                }
            }
        }
        private void Screen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (aimBox != null)
            {
                aimBox.SetValue(OpacityProperty, 0.0d);
                aimBox = null;
            }
        }
        private void Screen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as IInputElement);
            int x = (int)point.X;
            int y = (int)point.Y;
            selecting = true;
            var content = (DependencyObject)BarrageTabControl.SelectedContent;
            selectingBox = VisualDownwardSearch(content, "SelectingBox");
            selectingBox.SetValue(Canvas.LeftProperty, (double)x);
            selectingBox.SetValue(Canvas.TopProperty, (double)y);
            if (aimBox != null)
            {
                aimBox.SetValue(OpacityProperty, 0.0d);
                var boxX = (double)aimBox.GetValue(Canvas.LeftProperty);
                var boxY = (double)aimBox.GetValue(Canvas.TopProperty);
                AddComponent(aimComponent, (int)boxX, (int)boxY);
                aimBox = null;
            }
        }
        private void BarrageTabControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selecting)
                BarrageTabControl_MouseLeftButtonUp(sender, null);
        }
        private void BarrageTabControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selecting = false;
            if (selectingBox != null)
            {
                selectingBox.SetValue(WidthProperty, 0.0d);
                selectingBox.SetValue(HeightProperty, 0.0d);
                selectingBox = null;
            }
        }
        private void ScreenSetting_Click(object sender, RoutedEventArgs e)
        {
            ScreenSetting window = new ScreenSetting(config);
            window.Owner = this;
            window.ShowDialog();
        }
        private void BarrageMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = e.Source as MenuItem;
            switch (item.Name)
            {
                case "AddBarrage":
                    CreateNewBarrage();
                    break;
                case "DeleteBarrage":
                    DeleteSeletedBarrage();
                    break;
                case "CopyBarrage":
                    CopySeletedBarrage();
                    break;
                case "SetBarrage":
                    OpenSelectedBarrageSetting();
                    break;
            }
        }
        private void BarrageTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                UpdateScreen();
                var tabItem = e.AddedItems[0] as TabItem;
                foreach (var item in file.Barrages)
                    if (item.Name == (string)tabItem.Header)
                    {
                        selectedBarrage = item;
                        break;
                    }
            }
        }
        #endregion
    }
}
