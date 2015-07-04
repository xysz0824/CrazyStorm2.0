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
        static readonly string[] componentImages = new string[] 
        { "EmitterImage", "LaserImage", "MaskImage", "ReboundImage", "ForceImage" };
        #endregion

        #region Private Methods
        void CreatePropertyPanel(Component component)
        {
            TabItem item;
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
            item.Header = component.Name;
            item.Style = (Style)FindResource("CanCloseStyle");
            item.Content = PropertyPanelFactory.Create(component);
            LeftTabControl.Items.Add(item);
            item.Focus();
        }
        void UpdatePropertyPanel()
        {
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
            aimRect = VisualDownwardSearch((DependencyObject)ParticleTabControl.SelectedContent, "AimBox");
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
            var item = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }
        private void ComponentList_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
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
            UpdateComponent();
        }
        private void DeleteComponentItem_Click(object sender, RoutedEventArgs e)
        {
            new DelComponentCommand().Do(commandStacks[selectedParticle], selectedParticle, selectedComponents);
            UpdateComponent();
        }
        private void TabClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = VisualUpwardSearch<TabItem>(sender as DependencyObject) as TabItem;
            var tab = VisualUpwardSearch<TabControl>(item) as TabControl;
            tab.Items.Remove(item);
        }
        #endregion
    }
}
