/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
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
        ParticleSystem selectedParticle;
        DependencyObject aimRect;
        Component aimComponent;
        DependencyObject selectionRect;
        int selectionRectX, selectionRectY;
        bool selectingComponent;
        List<Line> bindingLines;
        List<Component> selectedComponents;
        DoubleClickDetector dclickDetector;
        #endregion

        #region Private Methods
        void UpdateScreen()
        {
            //Get component layer.
            Canvas canvas = null;
            foreach (TabItem item in ParticleTabControl.Items)
            {
                var content = item.Content as Canvas;
                if (item.Tag == selectedParticle)
                {
                    canvas = VisualHelper.VisualDownwardSearch(content, "ComponentLayer") as Canvas;
                    break;
                }
            }
            if (canvas != null)
            {
                if (selectedComponents == null)
                    selectedComponents = new List<Component>();
                else
                    selectedComponents.Clear();

                canvas.Children.Clear();
                //Update binding lines
                if (bindingLines != null)
                {
                    foreach (var line in bindingLines)
                        canvas.Children.Add(line);
                }
                //Update components on current screen.
                var itemTemplate = FindResource("ComponentItem") as DataTemplate;
                foreach (var layer in selectedParticle.Layers)
                {
                    if (layer.Visible)
                    {
                        foreach (var component in layer.Components)
                        {
                            var item = itemTemplate.LoadContent() as Canvas;
                            var frame = VisualHelper.VisualDownwardSearch(item, "Frame") as Label;
                            var icon = VisualHelper.VisualDownwardSearch(item, "Icon") as Image;
                            var box = VisualHelper.VisualDownwardSearch(item, "Box") as Image;
                            frame.DataContext = layer;
                            icon.DataContext = component;
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
                                if (!component.Selected)
                                    DrawHelper.DrawLine(canvas, (int)x + config.ScreenWidthOver2, (int)y + config.ScreenHeightOver2,
                                        (int)tx + config.ScreenWidthOver2, (int)ty + config.ScreenHeightOver2, 2, true, Colors.White, 0.5f);
                                else
                                    DrawHelper.DrawLine(canvas, (int)x + config.ScreenWidthOver2, (int)y + config.ScreenHeightOver2,
                                        (int)tx + config.ScreenWidthOver2, (int)ty + config.ScreenHeightOver2, 2, false, Colors.White, 0.5f);
                            }
                            if (component.Selected)
                                selectedComponents.Add(component);
                            else
                            {
                                //Draw component mark.
                                object marker = Assembly.GetExecutingAssembly().CreateInstance("CrazyStorm.ComponentMarker");
                                (marker as IComponentMark).Draw(canvas, component, (int)x + config.ScreenWidthOver2,
                                    (int)y + config.ScreenHeightOver2);
                                //Draw specific mark.
                                if (component is Emitter)
                                    marker = Assembly.GetExecutingAssembly().CreateInstance("CrazyStorm.EmitterMarker");
                                else
                                    marker = Assembly.GetExecutingAssembly().CreateInstance("CrazyStorm." + component.GetType().Name + "Marker");

                                (marker as IComponentMark).Draw(canvas, component, (int)x + config.ScreenWidthOver2,
                                    (int)y + config.ScreenHeightOver2);
                            }
                            icon.Source = new BitmapImage(new Uri(@"Images/button-" + component.GetType().Name + ".png", UriKind.Relative));
                            item.SetValue(Canvas.LeftProperty, (double)x - box.Width / 2 + config.ScreenWidthOver2);
                            item.SetValue(Canvas.TopProperty, (double)y - box.Height / 2 + config.ScreenHeightOver2);
                            canvas.Children.Add(item as UIElement);
                        }
                        foreach (var component in layer.Components)
                        {
                            if (component.Selected)
                            {
                                //If component has a parent, caculate the absolute position.
                                float x = component.X;
                                float y = component.Y;
                                if (component.Parent != null)
                                {
                                    Vector2 parent = component.Parent.GetAbsolutePosition();
                                    x += parent.x;
                                    y += parent.y;
                                }
                                //Draw component mark.
                                object marker = Assembly.GetExecutingAssembly().CreateInstance("CrazyStorm.ComponentMarker");
                                (marker as IComponentMark).Draw(canvas, component, (int)x + config.ScreenWidthOver2,
                                    (int)y + config.ScreenHeightOver2);
                                //Draw specific mark.
                                if (component is Emitter)
                                    marker = Assembly.GetExecutingAssembly().CreateInstance("CrazyStorm.EmitterMarker");
                                else
                                    marker = Assembly.GetExecutingAssembly().CreateInstance("CrazyStorm." + component.GetType().Name + "Marker");

                                (marker as IComponentMark).Draw(canvas, component, (int)x + config.ScreenWidthOver2,
                                    (int)y + config.ScreenHeightOver2);
                            }
                        }
                    }
                }
            }
        }
        void SelectComponents(int x, int y, int width, int height)
        {
            var set = new List<Component>();
            //Select those involved in selection rect. 
            int index = 0;
            foreach (var layer in selectedParticle.Layers)
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
                        var componentRect = new Rect(absoluteX - config.GridWidth / 2 + config.ScreenWidthOver2,
                            absoluteY - config.GridHeight / 2 + config.ScreenHeightOver2, config.GridWidth, config.GridHeight);
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
            SelectComponents(set, width == 0 && height == 0);
        }
        void SelectComponents(List<Component> set, bool canDoubleClick)
        {
            foreach (var layer in selectedParticle.Layers)
                if (layer.Visible)
                    foreach (var component in layer.Components)
                    {
                        if (Keyboard.Modifiers != ModifierKeys.Control)
                            component.Selected = false;
                        
                        if (set != null)
                        {
                            foreach (var target in set)
                            {
                                if (component == target)
                                {
                                    component.Selected = 
                                        Keyboard.Modifiers != ModifierKeys.Control ? 
                                        true :
                                        !component.Selected;
                                    break;
                                }
                            }
                        }
                    }

            UpdateSelectedStatus();
            //Check double click.
            if (!(canDoubleClick && set != null && set.Count > 0 && Keyboard.Modifiers != ModifierKeys.Control))
                return;

            if (dclickDetector == null)
                dclickDetector = new DoubleClickDetector();

            dclickDetector.Start();
            if (set.Count > 0 && dclickDetector.IsDetected())
                CreatePropertyPanel(set.First());
        }
        void CancelAllSelection()
        {
            foreach (var layer in selectedParticle.Layers)
                foreach (var component in layer.Components)
                    component.Selected = false;

            UpdateSelectedStatus();
        }
        #endregion

        #region Window EventHandlers
        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            screenMousePos = e.GetPosition(sender as IInputElement);
            int x = (int)screenMousePos.X;
            int y = (int)screenMousePos.Y;
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
                    selectionRect.SetValue(Canvas.LeftProperty, (double)selectionRectX);
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
                    selectionRect.SetValue(Canvas.TopProperty, (double)selectionRectY);
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
            int x = (int)point.X;
            int y = (int)point.Y;
            selectingComponent = true;
            var content = (DependencyObject)ParticleTabControl.SelectedContent;
            selectionRect = VisualHelper.VisualDownwardSearch(content, "SelectingBox");
            selectionRect.SetValue(Canvas.LeftProperty, (double)x);
            selectionRect.SetValue(Canvas.TopProperty, (double)y);
            selectionRectX = x;
            selectionRectY = y;
            //Add component to the place mouse down with left-button.
            if (aimRect != null)
            {
                aimRect.SetValue(OpacityProperty, 0.0d);
                var boxX = (double)aimRect.GetValue(Canvas.LeftProperty);
                var boxY = (double)aimRect.GetValue(Canvas.TopProperty);
                aimComponent.ID = selectedParticle.GetComponentIndex();
                var index = selectedParticle.GetAndIncreaseComponentIndex(aimComponent.GetType().ToString());
                aimComponent.Name = aimComponent.GetType().Name + index;
                aimComponent.X = (int)(boxX + (double)aimRect.GetValue(Canvas.WidthProperty) / 2 - config.ScreenWidthOver2);
                aimComponent.Y = (int)(boxY + (double)aimRect.GetValue(Canvas.HeightProperty) / 2 - config.ScreenHeightOver2);
                new AddComponentCommand().Do(commandStacks[selectedParticle], selectedParticle, selectedLayer, aimComponent);
                UpdateSelectedStatus();
                aimComponent = null;
                aimRect = null;
            }
            //Binding selected component
            if (bindingLines != null)
            {
                SelectComponents((int)screenMousePos.X, (int)screenMousePos.Y, 1, 1);
                if (selectedComponents.Count == 1)
                    new BindComponentCommand().Do(commandStacks[selectedParticle], bindingLines, selectedComponents.First());

                bindingLines = null;
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
            //Determine selected components.
            selectingComponent = false;
            if (selectionRect != null)
            {
                var x = (double)selectionRect.GetValue(LeftProperty);
                var y = (double)selectionRect.GetValue(TopProperty);
                var width = (double)selectionRect.GetValue(WidthProperty);
                var height = (double)selectionRect.GetValue(HeightProperty);
                SelectComponents((int)x, (int)y, (int)width, (int)height);
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
                        selectedParticle = item;
                        break;
                    }
                }
                InitializeLayerAndComponent();
                UpdateSelectedStatus();
            }
        }
        private void ScreenSettingItem_Click(object sender, RoutedEventArgs e)
        {
            //Open screen setting window.
            ScreenSetting window = new ScreenSetting(config);
            window.OnButtonClick += () => UpdateScreen();
            window.ShowDialog();
        }
        #endregion
    }
}
