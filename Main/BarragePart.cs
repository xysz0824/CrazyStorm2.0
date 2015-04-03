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
        static readonly string[] componentNames = new string[] 
        { "", "Emitter", "Laser", "Mask", "Rebound", "Force" };
        Barrage selectedBarrage;
        DependencyObject aimRect;
        Component aimComponent;
        DependencyObject selectionRect;
        readonly List<Component> selectedComponents = new List<Component>();
        bool selecting;
        #endregion
        //Mainly implement operation to data.
        #region Private Methods
        void UpdateScreen()
        {
            //Update everything in current screen.
            Canvas canvas = null;
            foreach (TabItem item in BarrageTabControl.Items)
            {
                var content = item.Content as Canvas;
                if ((string)item.Header == selectedBarrage.Name)
                {
                    canvas = VisualDownwardSearch(content, "ComponentLayer") as Canvas;
                    break;
                }
            }
            if (canvas != null)
            {
                canvas.Children.Clear();
                var itemTemplate = FindResource("ComponentItem") as DataTemplate;
                foreach (var layer in selectedBarrage.Layers)
                    if (layer.Visible)
                        foreach (var component in layer.Components)
                        {
                            var item = itemTemplate.LoadContent() as Canvas;
                            var frame = VisualDownwardSearch(item, "Frame") as Label;
                            frame.DataContext = layer;
                            var icon = VisualDownwardSearch(item, "Icon") as Image;
                            icon.DataContext = component;
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
        void SelectComponent(int x, int y, int width, int height)
        {
            //Select those involved in selection rect. 
            selectedComponents.Clear();
            var content = (DependencyObject)BarrageTabControl.SelectedContent;
            var canvas = VisualDownwardSearch(content, "ComponentLayer") as Canvas;
            int index = 0;
            if (canvas != null)
            {
                foreach (var layer in selectedBarrage.Layers)
                    if (layer.Visible)
                        foreach (var component in layer.Components)
                        {
                            var box = VisualDownwardSearch(canvas.Children[index], "Box") as Image;
                            box.Opacity = 0;
                            var selectRect = new Rect(x, y, width, height);
                            var componentRect = new Rect(component.X, component.Y, 32, 32);
                            if (selectRect.IntersectsWith(componentRect))
                            {
                                box.Opacity = 1;
                                //Prevent overlay shade from other preceding components.
                                if (width == 0 && height == 0 && selectedComponents.Count > 0)
                                    selectedComponents[selectedComponents.Count - 1] = component;
                                else
                                    selectedComponents.Add(component);
                            }
                            index++;
                        }
            }
            //Enable delection if selected one or more.
            DeleteComponent.IsEnabled = selectedComponents.Count > 0;
            //Enable binding if selected one.
            BindComponent.IsEnabled = selectedComponents.Count == 1;
            UnbindComponent.IsEnabled = BindComponent.IsEnabled;
            //Update.
            UpdateSelectedGroup();
        }
        void SelectComponent(Component target)
        {
            selectedComponents.Clear();
            var content = (DependencyObject)BarrageTabControl.SelectedContent;
            var canvas = VisualDownwardSearch(content, "ComponentLayer") as Canvas;
            int index = 0;
            if (canvas != null && target != null)
            {
                foreach (var layer in selectedBarrage.Layers)
                    if (layer.Visible)
                        foreach (var component in layer.Components)
                        {
                            var box = VisualDownwardSearch(canvas.Children[index], "Box") as Image;
                            box.Opacity = 0;
                            if (component == target)
                            {
                                box.Opacity = 1;
                                selectedComponents.Add(component);
                            }
                            index++;
                        }
            }
            //Enable delection if selected one or more.
            DeleteComponent.IsEnabled = selectedComponents.Count > 0;
            //Enable binding if selected one.
            BindComponent.IsEnabled = selectedComponents.Count == 1;
            UnbindComponent.IsEnabled = BindComponent.IsEnabled;
            //Update.
            UpdateSelectedGroup();
        }
        void UpdateSelectedGroup()
        {
            //Clean abandoned component.
            for (int i = 0; i < selectedComponents.Count; ++i )
                if (!selectedBarrage.Components.Contains(selectedComponents[i]))
                {
                    selectedComponents.RemoveAt(i);
                    i--;
                }
            //Update selected group.
            if (selectedComponents.Count > 0)
            {
                SelectedGroup.Opacity = 1;
                if (selectedComponents.Count == 1)
                {
                    var component = selectedComponents.First();
                    SelectedGroupType.Text = component.GetType().Name;
                    SelectedGroupName.Text = component.Name;
                    SelectedGroupTip.Text = (string)FindResource("DoubleClickTip");
                }
                else
                {
                    SelectedGroupType.Text = "Group";
                    SelectedGroupName.Text = selectedComponents.Count + (string)FindResource("ComponentUnit");
                    SelectedGroupTip.Text = string.Empty;
                }
            }
            else
                SelectedGroup.Opacity = 0;
        }
        void CreateNewBarrage()
        {
            var barrage = new Barrage("New Barrage");
            file.Barrages.Add(barrage);
            selectedBarrage = barrage;
            commandStacks[selectedBarrage] = new CommandStack();
            AddNewBarrageTab(barrage);
        }
        void AddNewBarrageTab(Barrage barrage)
        {
            var tabItem = new TabItem();
            tabItem.Header = barrage.Name;
            tabItem.Content = BarrageTabControl.ItemTemplate.LoadContent() as Canvas;
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

                commandStacks.Remove(selectedBarrage);
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
        //Implement control and interaction with UI.
        #region Window EventHandler
        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(sender as IInputElement);
            int x = (int)point.X;
            int y = (int)point.Y;
            if (x >= 0 && x < config.ScreenWidth && y >= 0 && y < config.ScreenHeight)
            {
                //Display a rect with red edge to mark the location that component will be put on.
                if (aimRect != null)
                {
                    if (config.GridAlignment)
                    {
                        aimRect.SetValue(Canvas.LeftProperty, (double)((x / 32) * 32));
                        aimRect.SetValue(Canvas.TopProperty, (double)((y / 32) * 32));
                    }
                    else if (x <= config.ScreenWidth - 32 && y <= config.ScreenHeight - 32)
                    {
                        aimRect.SetValue(Canvas.LeftProperty, (double)x);
                        aimRect.SetValue(Canvas.TopProperty, (double)y);
                    }
                }
                //Display a coloured  rect to mark the range that is selecting.
                if (selectionRect != null)
                {
                    var width =  x - (double)selectionRect.GetValue(Canvas.LeftProperty);
                    if (width <= 0)
                        width = 0;
                    var height = y - (double)selectionRect.GetValue(Canvas.TopProperty);
                    if (height <= 0)
                        height = 0;
                    selectionRect.SetValue(WidthProperty, width);
                    selectionRect.SetValue(HeightProperty, height);
                }
            }
        }
        private void Screen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Take away the rect.
            if (aimRect != null)
            {
                aimRect.SetValue(OpacityProperty, 0.0d);
                aimRect = null;
            }
        }
        private void Screen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(sender as IInputElement);
            int x = (int)point.X;
            int y = (int)point.Y;
            selecting = true;
            var content = (DependencyObject)BarrageTabControl.SelectedContent;
            selectionRect = VisualDownwardSearch(content, "SelectingBox");
            selectionRect.SetValue(Canvas.LeftProperty, (double)x);
            selectionRect.SetValue(Canvas.TopProperty, (double)y);
            //Add component to the place where mouse down with left-button.
            if (aimRect != null)
            {
                aimRect.SetValue(OpacityProperty, 0.0d);
                var boxX = (double)aimRect.GetValue(Canvas.LeftProperty);
                var boxY = (double)aimRect.GetValue(Canvas.TopProperty);
                aimComponent.X = (int)boxX;
                aimComponent.Y = (int)boxY;
                selectedBarrage.AddComponentToLayer(selectedLayer, aimComponent);
                UpdateComponent();
                aimRect = null;
            }
        }
        private void BarrageTabControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (selecting)
                BarrageTabControl_MouseLeftButtonUp(sender, null);
        }
        private void BarrageTabControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Determine selection of components.
            selecting = false;
            if (selectionRect != null)
            {
                var x = (double)selectionRect.GetValue(LeftProperty);
                var y = (double)selectionRect.GetValue(TopProperty);
                var width = (double)selectionRect.GetValue(WidthProperty);
                var height = (double)selectionRect.GetValue(HeightProperty);
                SelectComponent((int)x, (int)y, (int)width, (int)height);
                selectionRect.SetValue(WidthProperty, 0.0d);
                selectionRect.SetValue(HeightProperty, 0.0d);
                selectionRect = null;
            }
            else
                SelectComponent(null);
        }
        private void BarrageTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Maintain selected barrage.
            if (e.AddedItems.Count > 0)
            {
                var tabItem = e.AddedItems[0] as TabItem;
                foreach (var item in file.Barrages)
                    if (item.Name == (string)tabItem.Header)
                    {
                        selectedBarrage = item;
                        break;
                    }
                UpdateBarrage();
                UpdateLayer();
                UpdateComponent();
            }
        }
        private void ScreenSetting_Click(object sender, RoutedEventArgs e)
        {
            //Open screen setting window.
            ScreenSetting window = new ScreenSetting(config);
            window.Owner = this;
            window.ShowDialog();
        }
        private void BarrageMenu_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to the corresponding function of barrage menu.
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
        private void DeleteComponent_Click(object sender, RoutedEventArgs e)
        {
            new DelComponentCommand().Do(commandStacks[selectedBarrage], selectedBarrage, selectedComponents);
            UpdateComponent();
        }
        #endregion
    }
}
