using System;
using System.Linq;
using System.Windows;

namespace MonoGame.Framework.WpfInterop.Input
{
    internal static class WindowHelper
    {
        /// <summary>
        /// Returns the window of the given control or null if unable to find a window.
        /// If null, the default implementation is used
        /// </summary>
        /// <returns></returns>
        public static Func<IInputElement, Window> FindWindow = null;

        public static bool IsControlOnActiveWindow(IInputElement element)
        {
            Window window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            return GetWindowFrom(element) == window;
        }

        private static Window GetWindowFrom(IInputElement focusElement)
        {
            Func<IInputElement, Window> findWindow = WindowHelper.FindWindow;
            if (findWindow != null) return findWindow(focusElement);
            FrameworkElement frameworkElement = focusElement as FrameworkElement;
            if (frameworkElement == null)
            {
                throw new NotSupportedException("Only FrameworkElement is currently supported.");
            }
            return Window.GetWindow(frameworkElement);
        }
    }
}
