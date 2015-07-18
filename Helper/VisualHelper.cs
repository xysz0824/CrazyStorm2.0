using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
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
                    child = VisualHelper.VisualDownwardSearch(child, name);
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
    }
}
