/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Point screenMousePos;
        ParticleSystem selectedSystem;
        DependencyObject aimRect;
        Component aimComponent;
        DependencyObject selectionRect;
        double selectionRectX, selectionRectY;
        bool selectingComponent;
        List<Line> bindingLines;
        bool binded;
        List<Component> selectedComponents;
        #endregion

        #region Private Methods
        void UpdateScreen()
        {
            //Get component layer.
            Canvas canvas = null;
            foreach (TabItem item in ParticleTabControl.Items)
            {
                var content = item.Content as Canvas;
                if (item.Tag == selectedSystem)
                {
                    canvas = VisualHelper.VisualDownwardSearch(content, "ComponentLayer") as Canvas;
                    break;
                }
            }
            if (canvas != null)
            {
                var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
                if (selectedComponents == null) selectedComponents = new List<Component>();
                else selectedComponents.Clear();
                canvas.Children.Clear();
                //Update binding lines
                if (bindingLines != null && !binded) foreach (var line in bindingLines) canvas.Children.Add(line);
                else if (bindingLines != null && binded) foreach (var line in bindingLines) canvas.Children.Remove(line);
                //Update components on current screen.
                var assembly = Assembly.GetExecutingAssembly();
                var itemTemplate = FindResource("ComponentItem") as DataTemplate;
                foreach (var layer in selectedSystem.Layers)
                {
                    if (layer.Visible)
                    {
                        foreach (var component in layer.Components)
                        {
                            var item = itemTemplate.LoadContent() as Canvas;
                            var frame = VisualHelper.VisualDownwardSearch(item, "Frame") as Label;
                            var icon = VisualHelper.VisualDownwardSearch(item, "Icon") as Path;
                            var box = VisualHelper.VisualDownwardSearch(item, "Box") as Border;
                            var id = VisualHelper.VisualDownwardSearch(item, "ID") as Label;
                            id.Content = component.Name;
                            frame.DataContext = layer;
                            box.Opacity = component.Selected ? 1 : 0;
                            //If component has a parent, caculate the absolute position.
                            float x = component.X;
                            float y = component.Y;
                            if (component.Parent != null)
                            {
                                Vector2 parent = component.Parent.GetAbsolutePosition();
                                x += parent.x;
                                y += parent.y;
                            }
                            //Draw binding line.
                            if (component.BindingTarget != null)
                            {
                                float tx = component.BindingTarget.X;
                                float ty = component.BindingTarget.Y;
                                if (component.BindingTarget.Parent != null)
                                {
                                    Vector2 parent = component.BindingTarget.Parent.GetAbsolutePosition();
                                    tx += parent.x;
                                    ty += parent.y;
                                }
                                if (component.Selected)
                                {
                                    var v = new Vector2(tx - x, ty - y);
                                    DrawHelper.DrawArrow(canvas, (int)(x + center.X), (int)(y + center.Y), 
                                        Math.Max(1, (int)v.Length() - 16), 3, MathHelper.GetDegree(v), Colors.White, 0.5f);
                                }
                            }
                            if (component.Selected)
                            {
                                selectedComponents.Add(component);
                                //Draw component mark.
                                var marker = assembly.CreateInstance("CrazyStorm.ComponentMarker") as IComponentMark;
                                marker.Draw(canvas, component, (int)(x + center.X), (int)(y + center.Y));
                                //Draw specific mark.
                                if (component is Emitter) marker = assembly.CreateInstance("CrazyStorm.EmitterMarker") as IComponentMark;
                                else marker = assembly.CreateInstance("CrazyStorm." + component.GetType().Name + "Marker") as IComponentMark;
                                marker?.Draw(canvas, component, (int)(x + center.X), (int)(y + center.Y));
                            }
                            icon.Data = (Geometry)FindResource($"{component.GetType().Name}_Icon");
                            var scale = (double)FindResource($"{component.GetType().Name}_Scale");
                            var transform = new ScaleTransform(scale, scale, icon.ActualWidth / 2, icon.ActualHeight / 2);
                            icon.RenderTransform = transform;
                            item.SetValue(Canvas.LeftProperty, (double)x - box.Width / 2 + center.X);
                            item.SetValue(Canvas.TopProperty, (double)y - box.Height / 2 + center.Y);
                            canvas.Children.Add(item);
                        }
                    }
                }
            }
        }
        void SelectComponents(int x, int y, int width, int height, int clickCount)
        {
            var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
            var set = new List<Component>();
            //Select those involved in selection rect. 
            int index = 0;
            foreach (var layer in selectedSystem.Layers)
            {
                if (layer.Visible)
                {
                    foreach (var component in layer.Components)
                    {
                        //If component has parent, caculate the absolute position.
                        float absoluteX = component.X;
                        float absoluteY = component.Y;
                        if (component.Parent != null)
                        {
                            Vector2 parent = component.Parent.GetAbsolutePosition();
                            absoluteX += parent.x;
                            absoluteY += parent.y;
                        }
                        var selectRect = new Rect(x, y, width, height);
                        var componentRect = new Rect(absoluteX - config.GridWidth / 2 + center.X,
                            absoluteY - config.GridHeight / 2 + center.Y, config.GridWidth, config.GridHeight);
                        if (selectRect.IntersectsWith(componentRect))
                        {
                            //Prevent overlay shade from preceding components.
                            if (width == 0 && height == 0 && set.Count > 0)
                                set[set.Count - 1] = component;
                            else
                                set.Add(component);
                        }
                        index++;
                    }
                }
            }
            SelectComponents(set, width == 0 && height == 0, clickCount);
        }
        void SelectComponents(List<Component> set, bool canDoubleClick)
        {
            SelectComponents(set, canDoubleClick, 0);
        }
        void SelectComponents(List<Component> set, bool canDoubleClick, int clickCount)
        {
            foreach (var layer in selectedSystem.Layers)
            {
                if (!layer.Visible) continue;
                foreach (var component in layer.Components)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Control) component.Selected = false;
                    if (set != null)
                    {
                        foreach (var target in set)
                        {
                            if (component == target)
                            {
                                component.Selected = Keyboard.Modifiers != ModifierKeys.Control ? true : !component.Selected;
                                break;
                            }
                        }
                    }
                }
            }
            UpdateSelectedStatus();
            //If mouse double click
            if (!(canDoubleClick && set != null && set.Count > 0 && Keyboard.Modifiers != ModifierKeys.Control))
                return;

            if (set.Count > 0 && clickCount == 2)
                CreatePropertyPanel(set.First());
        }
        void CancelAllSelection()
        {
            foreach (var layer in selectedSystem.Layers)
                foreach (var component in layer.Components)
                    component.Selected = false;

            UpdateSelectedStatus();
        }
        #endregion

        #region Public Methods
        public void ChangeTheme(string name)
        {
            var dictionaries = App.Current.Resources.MergedDictionaries;
            var original = dictionaries.Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Style\\")).ToList();
            original.ForEach(d => dictionaries.Remove(d));
            switch (name)
            {
                case "StyleDefault":
                    dictionaries.Add(new ResourceDictionary() { Source = new Uri("Style\\Default.xaml", UriKind.Relative) });
                    break;
                case "StyleBlack":
                    dictionaries.Add(new ResourceDictionary() { Source = new Uri("Style\\Black.xaml", UriKind.Relative) });
                    break;
            }
            config.Theme = name;
            config.Save();
        }
        #endregion

        #region Window EventHandlers
        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    e.Handled = true;
                    break;
            }
        }
        private void Screen_LostFocus(object sender, RoutedEventArgs e)
        {
            CancelAllSelection();
        }
        private void Screen_MouseEnter(object sender, MouseEventArgs e)
        {
            MousePosTip.Visibility = Visibility.Visible;
        }
        private void Screen_MouseLeave(object sender, MouseEventArgs e)
        {
            MousePosTip.Visibility = Visibility.Hidden;
        }
        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
            screenMousePos = e.GetPosition(sender as IInputElement);
            int x = (int)screenMousePos.X;
            int y = (int)screenMousePos.Y;
            if (selectedComponents.Count == 1) MousePosTip.Content = $"{selectedComponents[0].X},{selectedComponents[0].Y}";
            else MousePosTip.Content = $"{x - center.X},{y - center.Y}";
            //Display a rect with red edge to mark the location that component will be put on.
            if (aimRect != null)
            {
                if (config.GridAlignment)
                {
                    aimRect.SetValue(Canvas.LeftProperty, (double)((x / ((int)config.GridWidth / 2)) * (config.GridWidth / 2)));
                    aimRect.SetValue(Canvas.TopProperty, (double)((y / ((int)config.GridHeight / 2)) * (config.GridHeight / 2)));
                }
                else if (x <= config.ScreenWidth - config.GridWidth && y <= config.ScreenHeight - config.GridHeight)
                {
                    aimRect.SetValue(Canvas.LeftProperty, (double)x);
                    aimRect.SetValue(Canvas.TopProperty, (double)y);
                }
            }
            //Display a coloured rect to mark the range that is selecting.
            if (selectionRect != null)
            {
                var width = x - selectionRectX;
                if (width >= 0)
                {
                    selectionRect.SetValue(Canvas.LeftProperty, selectionRectX);
                    selectionRect.SetValue(WidthProperty, (double)width);
                }
                else
                {
                    selectionRect.SetValue(Canvas.LeftProperty, (double)x);
                    selectionRect.SetValue(WidthProperty, (double)-width);
                }
                var height = y - selectionRectY;
                if (height >= 0)
                {
                    selectionRect.SetValue(Canvas.TopProperty, selectionRectY);
                    selectionRect.SetValue(HeightProperty, (double)height);
                }
                else
                {
                    selectionRect.SetValue(Canvas.TopProperty, (double)y);
                    selectionRect.SetValue(HeightProperty, (double)-height);
                }
            }
            //Update binding lines
            if (bindingLines != null)
            {
                foreach (var line in bindingLines)
                {
                    line.X2 = (int)screenMousePos.X;
                    line.Y2 = (int)screenMousePos.Y;
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
            //Show selection rect.
            Point point = e.GetPosition(sender as IInputElement);
            double x = point.X;
            double y = point.Y;
            selectingComponent = true;
            var content = (DependencyObject)ParticleTabControl.SelectedContent;
            selectionRect = VisualHelper.VisualDownwardSearch(content, "SelectingBox");
            selectionRect.SetValue(Canvas.LeftProperty, x);
            selectionRect.SetValue(Canvas.TopProperty, y);
            selectionRectX = x;
            selectionRectY = y;
            //Add component to the place mouse down with left-button.
            if (aimRect != null)
            {
                var center = new Point(config.ScreenWidthOver2, config.ScreenHeightOver2);
                aimRect.SetValue(OpacityProperty, 0.0d);
                var boxX = (double)aimRect.GetValue(Canvas.LeftProperty);
                var boxY = (double)aimRect.GetValue(Canvas.TopProperty);
                aimComponent.ID = selectedSystem.GetComponentIndex();
                var index = selectedSystem.GetAndIncreaseComponentIndex(aimComponent.GetType().ToString());
                aimComponent.Name = (string)FindResource($"{aimComponent.GetType().Name}Str") + (index + 1);
                aimComponent.X = (int)(boxX + (double)aimRect.GetValue(Canvas.WidthProperty) / 2 - center.X);
                aimComponent.Y = (int)(boxY + (double)aimRect.GetValue(Canvas.HeightProperty) / 2 - center.Y);
                new AddComponentCommand().Do(commandStacks[selectedSystem], selectedSystem, selectedLayer, aimComponent);
                UpdateSelectedStatus();
                aimComponent = null;
                aimRect = null;
            }
            //Binding to selected emitter
            if (bindingLines != null)
            {
                binded = true;
                SelectComponents((int)screenMousePos.X, (int)screenMousePos.Y, 1, 1, e.ClickCount);
                if (selectedComponents.Count == 1 && selectedComponents[0] is Emitter)
                {
                    new BindComponentCommand().Do(commandStacks[selectedSystem], bindingLines, selectedComponents.First());
                    var components = new List<Component>();
                    foreach (var line in bindingLines)
                    {
                        var component = line.DataContext as Component;
                        components.Add(component);
                    }
                    SelectComponents(components, false);
                }
                bindingLines = null;
            }
            if (e.ClickCount == 2)
            {
                ParticleTabControl_MouseLeftButtonUp(sender, e);
            }
        }
        private void ParticleTabControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //Cancel selection when mouse leaves.
            if (selectingComponent)
                ParticleTabControl_MouseLeftButtonUp(sender, null);
        }
        private void ParticleTabControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectingComponent = false;
            if (binded)
            {
                binded = false;
                selectionRect.SetValue(WidthProperty, 0.0d);
                selectionRect.SetValue(HeightProperty, 0.0d);
                selectionRect = null;
                return;
            }
            //Determine selected components.
            if (selectionRect != null)
            {
                var x = (double)selectionRect.GetValue(LeftProperty);
                var y = (double)selectionRect.GetValue(TopProperty);
                var width = (double)selectionRect.GetValue(WidthProperty);
                var height = (double)selectionRect.GetValue(HeightProperty);
                if (e != null)
                    SelectComponents((int)x, (int)y, (int)width, (int)height, e.ClickCount);
                else
                    SelectComponents((int)x, (int)y, (int)width, (int)height, 0);

                selectionRect.SetValue(WidthProperty, 0.0d);
                selectionRect.SetValue(HeightProperty, 0.0d);
                selectionRect = null;
            }
            else
                CancelAllSelection();
        }
        private void ParticleTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Switch to selected Particle.
            if (e.AddedItems.Count > 0)
            {
                var tabItem = e.AddedItems[0] as TabItem;
                foreach (var item in file.ParticleSystems)
                {
                    if (item == tabItem.Tag)
                    {
                        selectedSystem = item;
                        break;
                    }
                }
                InitializeLayerAndComponent();
                UpdateSelectedStatus();
            }
        }
        private void StyleItem_Loaded(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as MenuItem;
            clickedItem.IsChecked = clickedItem.Name == config.Theme;
        }
        private void ScreenSettingItem_Click(object sender, RoutedEventArgs e)
        {
            //Open screen setting window.
            ScreenSetting window = new ScreenSetting(config);
            window.OnButtonClick += () => UpdateScreen();
            window.ShowDialog();
            window.Close();
        }

        private void StyleItem_Click(object sender, RoutedEventArgs e)
        {
            var clickedItem = sender as MenuItem; 
            var parent = clickedItem.Parent as MenuItem;
            foreach (MenuItem item in parent.Items)
            {
                item.IsChecked = false;
            }
            clickedItem.IsChecked = true;
            ChangeTheme(clickedItem.Name);
        }
        #endregion
    }
}
