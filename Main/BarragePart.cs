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
        #region Private Methods
        void AddNewBarrageTab(Barrage barrage)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = barrage.Name;
            tabItem.DataContext = config;
            tabItem.Content = BarrageTabControl.ItemTemplate.LoadContent();
            BarrageTabControl.Items.Add(tabItem);
            BarrageTabControl.SelectedItem = tabItem;
        }
        void CreateNewBarrage()
        {
            var barrage = new Barrage("New Barrage");
            file.Barrages.Add(barrage);
            selectedBarrage = barrage;
            AddNewBarrageTab(barrage);
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
            BarrageSetting window = new BarrageSetting(file, selectedBarrage);
            window.Owner = this;
            window.ShowDialog();
        }
        #endregion

        #region Window EventHandler
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
                var tabItem = e.AddedItems[0] as TabItem;
                foreach (var item in file.Barrages)
                    if (item.Name == (string)tabItem.Header)
                    {
                        selectedBarrage = item;
                        break;
                    }
            }
            //TODO : Update components.
        }
        #endregion
    }
}
