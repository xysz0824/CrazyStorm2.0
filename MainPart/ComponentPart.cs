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
using System.Reflection;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Point lastMouseDown;
        DependencyObject lastSelectedItem;
        static readonly string[] componentImages = new string[] 
        { "MultiEmitterImage", "CurveEmitterImage", "MaskImage", "ReboundImage", "ForceImage" };
        #endregion

        #region Private Methods
        void UpdateSelectedGroup()
        {
            //Clean removed component.
            for (int i = 0; i < selectedComponents.Count; ++i)
                if (!selectedParticle.Components.Contains(selectedComponents[i]))
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
                    SelectedGroupTip.Text = (string)FindResource("DoubleClickTip");
                    for (int i = 0; i < componentNames.Length; ++i)
                        if (componentNames[i] == component.GetType().Name)
                        {
                            SelectedGroupImage.Source = new BitmapImage(new Uri(@"Images/button" + i + ".png", UriKind.Relative));
                            break;
                        }
                }
                else
                {
                    SelectedGroupImage.Source = new BitmapImage(new Uri(@"Images/group.png", UriKind.Relative));
                    SelectedGroupType.Text = "Group";
                    SelectedGroupName.Text = selectedComponents.Count + (string)FindResource("ComponentUnit");
                    SelectedGroupTip.Text = string.Empty;
                }
            }
            else
                SelectedGroup.Opacity = 0;
        }
        void CreatePropertyPanel(Component component)
        {
            SelectComponents(null, true);
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
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroll.Content = new PropertyPanel(commandStacks[selectedParticle], component, UpdateProperty);
            item.Content = scroll;
            LeftTabControl.Items.Add(item);
            item.Focus();
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
        void UpdatePropertyPanel()
        {
            //Remove the tab which is not belonging to any component.
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                TabItem item = LeftTabControl.Items[i] as TabItem;
                if (!selectedParticle.Components.Contains(item.DataContext))
                {
                    LeftTabControl.Items.RemoveAt(i);
                    i--;
                }
            }
        }
        #endregion

        #region Window EventHandler
        private void Component_Click(object sender, RoutedEventArgs e)
        {
            //Create corresponding component according to different button.
            var button = sender as Button;
            aimRect = VisualHelper.VisualDownwardSearch((DependencyObject)ParticleTabControl.SelectedContent, "AimBox");
            aimRect.SetValue(OpacityProperty, 1.0d);
            aimComponent = ComponentFactory.Create(button.Name);
        }
        private void Component_MouseEnter(object sender, MouseEventArgs e)
        {
            //Light up button when mouse enter.
            var image = sender as Image;
            for (int i = 1; i <= componentImages.Length; ++i)
                if (image.Name == componentImages[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + "on.png", UriKind.Relative));
                    break;
                }
        }
        private void Component_MouseLeave(object sender, MouseEventArgs e)
        {
            //Reset button when mouse leave.
            var image = sender as Image;
            for (int i = 1; i <= componentImages.Length; ++i)
                if (image.Name == componentImages[i - 1])
                {
                    image.Source = new BitmapImage(new Uri(@"Images\button" + i + ".png", UriKind.Relative));
                    break;
                }
        }
        private void ComponentList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Focus pointed item when mouse right-button down.
            var item = VisualHelper.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }
        private void ComponentList_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            lastMouseDown = e.GetPosition(ComponentList);
            lastSelectedItem = sender as DependencyObject;
            //Select pointed component when mouse left-button down. 
            var textBlock = sender as TextBlock;
            var set = new List<CrazyStorm.Core.Component>();
            foreach (var component in selectedParticle.Components)
                if (component == textBlock.DataContext)
                {
                    set.Add(component);
                    SelectComponents(set, true);
                    break;
                }
        }
        private void ComponentList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(ComponentList);
                if ((Math.Abs(currentPosition.X - lastMouseDown.X) > 2.0) || 
                    (Math.Abs(currentPosition.Y - lastMouseDown.Y) > 2.0))
                {
                    if (lastSelectedItem != null)
                    {
                        DragDropEffects finalDropEffect = DragDrop.DoDragDrop(lastSelectedItem, sender, DragDropEffects.Move);
                    }
                }
            }
        }
        private void ComponentList_CheckDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
        private void ComponentList_Drop(object sender, DragEventArgs e)
        {
            var sourceComponent = ((TextBlock)lastSelectedItem).DataContext as Component;
            if (!(e.OriginalSource is TextBlock))
                return;

            var targetIComponent = ((TextBlock)e.OriginalSource).DataContext as Component;
            if (sourceComponent == targetIComponent)
                return;

            if (targetIComponent.Children.Contains(sourceComponent))
            {
                //A way to change the node from parenthood to brotherhood. 
                var tree = new Component();
                foreach (Component item in selectedParticle.Components)
                    tree.Children.Add(item);
                var parent = tree.FindParent(targetIComponent);
                if (parent != null)
                {
                    if (parent == tree)
                        selectedParticle.Components.Add(sourceComponent);
                    else
                        parent.Children.Add(sourceComponent);

                    targetIComponent.Children.Remove(sourceComponent);
                }
            }
            else
            {
                var tree = new Component();
                foreach (Component item in selectedParticle.Components)
                    tree.Children.Add(item);
                var parent = tree.FindParent(sourceComponent);
                if (parent != null)
                {
                    if (parent == tree)
                        selectedParticle.Components.Remove(sourceComponent);
                    else
                        parent.Children.Remove(sourceComponent);

                    targetIComponent.Children.Add(sourceComponent);
                    return;
                }
            }
        }
        private void CopyComponent_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Copy function.
        }
        private void DeleteComponent_Click(object sender, RoutedEventArgs e)
        {
            var item = ComponentList.SelectedItem as CrazyStorm.Core.Component; 
            var list = new List<CrazyStorm.Core.Component>();
            list.Add(item);
            new DelComponentCommand().Do(commandStacks[selectedParticle], selectedParticle, list);
            Update();
        }
        private void DeleteComponentItem_Click(object sender, RoutedEventArgs e)
        {
            new DelComponentCommand().Do(commandStacks[selectedParticle], selectedParticle, selectedComponents);
            Update();
        }
        private void TabClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = VisualHelper.VisualUpwardSearch<TabItem>(sender as DependencyObject) as TabItem;
            var tab = VisualHelper.VisualUpwardSearch<TabControl>(item) as TabControl;
            if (tab != null)
                tab.Items.Remove(item);
        }
        #endregion
    }
}
