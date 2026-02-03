/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;

namespace CrazyStorm
{
    static class VisualHelper
    {
        public static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        public static DependencyObject VisualDownwardSearch(DependencyObject source, string name)
        {
            if (source == null)
                return null;

            var count = VisualTreeHelper.GetChildrenCount(source);
            if (count == 0)
                return null;

            for (int i = 0; i < count; ++i)
            {
                var child = VisualTreeHelper.GetChild(source, i);
                if ((string)child.GetValue(FrameworkElement.NameProperty) == name)
                    return child;
                else
                {
                    child = VisualDownwardSearch(child, name);
                    if (child != null)
                        return child;
                }
            }
            return null;
        }
        public static DependencyObject VisualDownwardSearch<T>(DependencyObject source)
        {
            if (source == null)
                return null;

            var count = VisualTreeHelper.GetChildrenCount(source);
            if (count == 0)
                return null;

            for (int i = 0; i < count; ++i)
            {
                var child = VisualTreeHelper.GetChild(source, i);
                if (child.GetType() == typeof(T))
                    return child;
                else
                {
                    child = VisualHelper.VisualDownwardSearch<T>(child);
                    if (child != null)
                        return child;
                }
            }
            return null;
        }
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T childContent = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                childContent = v as T;
                if (childContent == null)
                {
                    childContent = GetVisualChild<T>(v);
                }
                if (childContent != null)
                {
                    break;
                }
            }
            return childContent;
        }
        public static void FocusItem<T>(MouseButtonEventArgs e)
        {
            //Focus pointed item when mouse right-button down.
            var item = VisualUpwardSearch<T>(e.OriginalSource as DependencyObject) as UIElement;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            T obj1;
            while (true)
            {
                DependencyObject parent1 = LogicalTreeHelper.GetParent(child);
                DependencyObject parent2 = VisualTreeHelper.GetParent(child);
                if (parent1 != null || parent2 != null)
                {
                    T obj2 = parent1 as T;
                    if (obj2 == null) obj2 = parent2 as T;
                    obj1 = obj2;
                    if (obj1 == null) child = parent1 ?? parent2;
                    else goto final;
                }
                else break;
            }
            return default(T);
        final:
            return obj1;
        }
    }
}
