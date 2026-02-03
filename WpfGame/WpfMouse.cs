using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MonoGame.Framework.WpfInterop.Input
{
    /// <summary>
    /// Helper class that converts WPF mouse input to the XNA/MonoGame <see cref="F:MonoGame.Framework.WpfInterop.Input.WpfMouse._mouseState" />.
    /// Required for any WPF hosted control.
    /// </summary>
    public class WpfMouse
    {
        private bool _captureMouseWithin = true;
        private readonly WpfGame _focusElement;
        private MouseState _mouseState;

        /// <summary>Creates a new instance of the mouse helper.</summary>
        /// <param name="focusElement">The element that will be used as the focus point. Provide your implementation of <see cref="T:MonoGame.Framework.WpfInterop.WpfGame" /> here.</param>
        public WpfMouse(WpfGame focusElement)
        {
            if (focusElement == null) throw new ArgumentNullException(nameof(focusElement));
            _focusElement = focusElement;
            _focusElement.MouseWheel += new MouseWheelEventHandler(HandleMouse);
            _focusElement.MouseMove += new MouseEventHandler(HandleMouse);
            _focusElement.MouseEnter += new MouseEventHandler(HandleMouse);
            _focusElement.MouseLeave += new MouseEventHandler(HandleMouse);
            _focusElement.MouseLeftButtonDown += new MouseButtonEventHandler(HandleMouse);
            _focusElement.MouseLeftButtonUp += new MouseButtonEventHandler(HandleMouse);
            _focusElement.MouseRightButtonDown += new MouseButtonEventHandler(HandleMouse);
            _focusElement.MouseRightButtonUp += new MouseButtonEventHandler(HandleMouse);
        }

        /// <summary>
        /// Gets or sets the mouse capture behaviour.
        /// If true, the mouse will be captured within the control. This means that the control will still capture mouse events when the user drags the mouse outside the control.
        /// E.g. mouse down on game window, mouse drag to outside of the window, mouse release -&gt; the game will still register the mouse release. The downside is that overlayed elements (textbox, etc.) will never be able to receive focus.
        /// If false, mouse events outside the game window are never registered. E.g. mouse down on game window, mouse drag to outside of the window, mouse release -&gt; the game will still thing the mouse is pressed until the cursor enters the window again.
        /// The upside is that overlayed controls (e.g. textboxes) can receive focus and input.
        /// Defaults to true.
        /// </summary>
        public bool CaptureMouseWithin
        {
            get
            {
                return _captureMouseWithin;
            }
            set
            {
                if (!value && _focusElement.IsMouseCaptured) _focusElement.ReleaseMouseCapture();
                _captureMouseWithin = value;
            }
        }

        public MouseState GetState()
        {
            return _mouseState;
        }

        private void HandleMouse(object sender, MouseEventArgs e)
        {
            if (e.Handled) return;
            Point point = e.GetPosition(_focusElement);
            if (!CaptureMouseWithin)
            {
                point = new Point(Clamp(point.X, 0, _focusElement.ActualWidth), Clamp(point.Y, 0, _focusElement.ActualHeight));
            }
            if (_focusElement.IsMouseDirectlyOver && System.Windows.Input.Keyboard.FocusedElement != _focusElement && WindowHelper.IsControlOnActiveWindow(_focusElement))
            {
                if (_focusElement.FocusOnMouseOver) _focusElement.Focus();
                else if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || 
                    (e.MiddleButton == MouseButtonState.Pressed || e.XButton1 == MouseButtonState.Pressed) || e.XButton2 == MouseButtonState.Pressed)
                {
                    _focusElement.Focus();
                }
            }
            if ((!_focusElement.IsMouseDirectlyOver || _focusElement.IsMouseCaptured) && CaptureMouseWithin)
            {
                if (!_focusElement.IsMouseCaptured) return;
                WpfGame focusElement = _focusElement;
                bool hit = false;
                HitTestResultCallback resultCallback = (target =>
                {
                    if (target.VisualHit == _focusElement) hit = true;
                    return HitTestResultBehavior.Continue;
                });
                PointHitTestParameters hitTestParameters = new PointHitTestParameters(point);
                VisualTreeHelper.HitTest(focusElement, (filterTarget => HitTestFilterBehavior.Continue), resultCallback, hitTestParameters);
                if (!hit)
                {
                    _mouseState = new MouseState(_mouseState.X, _mouseState.Y, _mouseState.ScrollWheelValue, 
                        (ButtonState)e.LeftButton, (ButtonState)e.MiddleButton, (ButtonState)e.RightButton, (ButtonState)e.XButton1, (ButtonState)e.XButton2);
                    if (e.LeftButton == MouseButtonState.Released) _focusElement.ReleaseMouseCapture();
                    e.Handled = true;
                    return;
                }
            }
            if (CaptureMouseWithin)
            {
                if (!_focusElement.IsMouseCaptured)
                {
                    if (!WindowHelper.IsControlOnActiveWindow(_focusElement)) return;
                    _focusElement.CaptureMouse();
                }
            }
            else if (_focusElement.IsFocused && !WindowHelper.IsControlOnActiveWindow(_focusElement)) return;
            e.Handled = true;
            MouseState mouseState = _mouseState;
            MouseWheelEventArgs mouseWheelEventArgs = e as MouseWheelEventArgs;
            _mouseState = new MouseState((int)point.X, (int)point.Y, mouseState.ScrollWheelValue + 
                (mouseWheelEventArgs != null ? mouseWheelEventArgs.Delta : 0), 
                (ButtonState)e.LeftButton, (ButtonState)e.MiddleButton, (ButtonState)e.RightButton, (ButtonState)e.XButton1, (ButtonState)e.XButton2);
        }

        private static double Clamp(double v, int min, double max)
        {
            if (v < min) return min;
            if (v <= max) return v;
            return max;
        }

        /// <summary>
        /// Sets the cursor to the specific coordinates within the attached game.
        /// This is required as the monogame Mouse.SetPosition function relies on the underlying Winforms implementation and will not work with WPF.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetCursor(int x, int y)
        {
            Point screen = _focusElement.PointToScreen(new Point(x, y));
            SetCursorPos((int)screen.X, (int)screen.Y);
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}
