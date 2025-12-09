/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Point lastMouseDown;
        DependencyObject lastSelectedItem;
        #endregion

        #region Private Methods
        void UpdateSelectedGroup()
        {
            //Get all visible components in this particle system.
            var set = new List<Component>();
            foreach (var layer in selectedSystem.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);
            //Clean removed component.
            for (int i = 0; i < selectedComponents.Count; ++i)
                if (!set.Contains(selectedComponents[i]))
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
                    SelectedGroupName.DataContext = component;
                    SelectedGroupName.SetBinding(TextBlock.TextProperty, "Name");
                    SelectedGroupTip.Text = (string)FindResource("DoubleClickTipStr");
                    SelectedGroupIconPath.Data = (Geometry)FindResource($"{component.GetType().Name}_Icon");
                    var scale = (double)FindResource($"{component.GetType().Name}_Scale");
                    var transform = new ScaleTransform(scale, scale);
                    SelectedGroupIconPath.RenderTransform = transform;
                }
                else
                {
                    SelectedGroupIconPath.Data = (Geometry)FindResource($"Group_Icon");
                    var scale = (double)FindResource("Group_Scale");
                    var transform = new ScaleTransform(scale, scale);
                    SelectedGroupIconPath.RenderTransform = transform;
                    SelectedGroupType.Text = "Group";
                    SelectedGroupName.Text = selectedComponents.Count + (string)FindResource("ComponentUnitStr");
                    SelectedGroupTip.Text = string.Empty;
                }
            }
            else
                SelectedGroup.Opacity = 0;
        }
        void CreatePropertyPanel(Component component)
        {
            TabItem item;
            //Prevent from repeating tab of components.  
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                item = LeftTabControl.Items[i] as TabItem;
                if (item.DataContext == component)
                {
                    item.Focus();
                    return;
                }
            }
            item = new TabItem();
            item.DataContext = component;
            item.Style = (Style)FindResource("CanCloseStyle");
            var scroll = new ScrollViewer();
            var baseScrollBarStyle = (Style)Application.Current.FindResource("FlatScrollBarStyle");
            var implicitScrollBarStyle = new Style(typeof(ScrollBar), baseScrollBarStyle);
            scroll.Resources.Add(typeof(ScrollBar), implicitScrollBarStyle);
            var trackBrush = (Brush)Application.Current.FindResource("ScrollBarTrackBrush");
            scroll.Resources.Add(SystemColors.ControlBrushKey, trackBrush);
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            var particleTypes = new List<ParticleType>();
            particleTypes.AddRange(ParticleType.DefaultTypes);
            particleTypes.AddRange(selectedSystem.CustomTypes);
            var panel = new PropertyPanel(commandStacks[selectedSystem], file, 
                particleTypes, component, UpdateProperty);
            scroll.Content = panel;
            panel.OnBeginEditing += () =>
            {
                editingProperties = true;
            };
            panel.OnEndEditing += () =>
            {
                editingProperties = false;
            };
            item.Content = scroll;
            LeftTabControl.Items.Add(item);
            item.Focus();
            saved = false;
        }
        void UpdateProperty()
        {
            foreach (TabItem item in LeftTabControl.Items)
            {
                var scroll = item.Content as ScrollViewer;
                if (scroll != null)
                {
                    var content = scroll.Content as PropertyPanel;
                    if (content != null)
                        content.UpdateProperty();
                }
            }
            UpdateScreen();
        }
        void UpdateComponentMenu()
        {
            //Enable binding if selected one or more.
            BindComponentItem.IsEnabled = selectedComponents != null && selectedComponents.Count > 0;
            UnbindComponentItem.IsEnabled = BindComponentItem.IsEnabled;
        }
        void UpdateComponentPanels()
        {
            //Get all visible components in this particle system.
            var set = new List<Component>();
            foreach (var layer in selectedSystem.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                TabItem item = LeftTabControl.Items[i] as TabItem;
                //Remove the property panel which is not belonging to any component.
                if (item.DataContext is Component && !set.Contains(item.DataContext))
                {
                    LeftTabControl.Items.RemoveAt(i);
                    i--;
                }
                //Update finder panel
                if (item.Content is FinderPanel)
                    (item.Content as FinderPanel).Update(selectedSystem);
            }
        }
        void BindComponent()
        {
            bindingLines = new List<Line>();
            foreach (var component in selectedComponents)
            {
                //If component has parent, caculate the absolute position.
                float x = component.X;
                float y = component.Y;
                if (component.Parent != null)
                {
                    Vector2 parent = component.Parent.GetAbsolutePosition();
                    x += parent.x;
                    y += parent.y;
                }
                var line = DrawHelper.GetLine((int)x + config.ScreenWidthOver2, (int)y + config.ScreenHeightOver2,
                    (int)screenMousePos.X, (int)screenMousePos.Y, 2, true, Colors.White, 0.5f);
                line.DataContext = component;
                bindingLines.Add(line);
            }
            UpdateScreen();
        }
        void UnbindComponent()
        {
            if (selectedComponents.Count > 0)
            {
                foreach (var component in selectedComponents)
                {
                    if (component.BindingTarget != null)
                    {
                        new UnbindComponentCommand().Do(commandStacks[selectedSystem], selectedComponents);
                        UpdateScreen();
                        break;
                    }
                }
            }
        }
        #endregion

        #region Window EventHandlers
        private void ComponentButton_Click(object sender, RoutedEventArgs e)
        {
            //Create corresponding component according to different button.
            var button = sender as Button;
            aimRect = VisualHelper.VisualDownwardSearch((DependencyObject)ParticleTabControl.SelectedContent, "AimBox");
            aimRect.SetValue(OpacityProperty, 1.0d);
            aimComponent = ComponentFactory.Create(button.Name);
            var emitter = aimComponent as Emitter;
            if (emitter != null)
            {
                emitter.Particle.Type = ParticleType.DefaultTypes.First();
            }
        }
        private void ComponentTree_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            lastMouseDown = e.GetPosition(ComponentTree);
            lastSelectedItem = sender as DependencyObject;
            //Select pointed component when mouse left-button down. 
            var textBlock = sender as TextBlock;
            var set = new List<CrazyStorm.Core.Component>();
            set.Add((Component)textBlock.DataContext);
            SelectComponents(set, true);
        }
        private void ComponentTree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(ComponentTree);
                if ((Math.Abs(currentPosition.X - lastMouseDown.X) > 2.0) || 
                    (Math.Abs(currentPosition.Y - lastMouseDown.Y) > 2.0))
                {
                    if (lastSelectedItem != null)
                        DragDrop.DoDragDrop(lastSelectedItem, sender, DragDropEffects.Move);
                }
            }
            else
            {
                lastSelectedItem = null;
            }
        }
        private void ComponentTree_CheckDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
        private void ComponentTree_Drop(object sender, DragEventArgs e)
        {
            var sourceComponent = ((TextBlock)lastSelectedItem).DataContext as Component;
            if (!(e.OriginalSource is TextBlock))
                return;

            var targetComponent = ((TextBlock)e.OriginalSource).DataContext as Component;
            if (sourceComponent == targetComponent)
                return;

            if (sourceComponent.Children.Contains(targetComponent))
                return;

            if (targetComponent.Children.Contains(sourceComponent))
            {
                //A way to change the node from parenthood to brotherhood. 
                var tree = new Component();
                foreach (Component item in selectedSystem.ComponentTree)
                    tree.Children.Add(item);

                var parent = tree.FindParent(targetComponent);
                if (parent != null)
                {
                    if (parent == tree)
                    {
                        selectedSystem.ComponentTree.Add(sourceComponent);
                        sourceComponent.TransPositiontoAbsolute();
                        sourceComponent.Parent = null;
                    }
                    else
                    {
                        parent.Children.Add(sourceComponent);
                        sourceComponent.TransPositiontoAbsolute();
                        sourceComponent.Parent = parent;
                        sourceComponent.TransPositiontoRelative();
                    }
                    targetComponent.Children.Remove(sourceComponent);
                }
            }
            else
            {
                //Add source component to target component as child
                var tree = new Component();
                foreach (Component item in selectedSystem.ComponentTree)
                    tree.Children.Add(item);

                var parent = tree.FindParent(sourceComponent);
                if (parent != null)
                {
                    if (parent == tree)
                        selectedSystem.ComponentTree.Remove(sourceComponent);
                    else
                        parent.Children.Remove(sourceComponent);

                    targetComponent.Children.Add(sourceComponent);
                    sourceComponent.TransPositiontoAbsolute();
                    sourceComponent.Parent = targetComponent;
                    sourceComponent.TransPositiontoRelative();
                }
            }
            UpdateProperty();
        }
        private void ComponentTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && ComponentTree.SelectedItem != null)
            {
                CreatePropertyPanel(ComponentTree.SelectedItem as Component);
            }
        }
        private void TabClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tabItem = VisualHelper.VisualUpwardSearch<TabItem>(sender as DependencyObject) as TabItem;
            LeftTabControl.Items.Remove(tabItem);
            var hasComponentPanel = false;
            foreach (var item in LeftTabControl.Items)
            {
                tabItem = item as TabItem;
                if (tabItem != null && tabItem.DataContext != null && tabItem.DataContext is Component)
                {
                    hasComponentPanel = true;
                    LeftTabControl.SelectedItem = tabItem;
                }
            }
            if (!hasComponentPanel) LeftTabControl.SelectedIndex = 0;
        }
        private void BindComponentItem_Click(object sender, RoutedEventArgs e)
        {
            BindComponent();
        }
        private void UnbindComponentItem_Click(object sender, RoutedEventArgs e)
        {
            UnbindComponent();
        }
        #endregion
    }
}
