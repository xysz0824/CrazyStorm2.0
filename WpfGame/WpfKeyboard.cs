using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace MonoGame.Framework.WpfInterop.Input
{
    /// <summary>
    /// Helper class that accesses a native API to get the current keystate.
    /// Required for any WPF hosted control.
    /// </summary>
    public class WpfKeyboard
    {
        private readonly WpfGame _focusElement;

        /// <summary>Creates a new instance of the keyboard helper.</summary>
        /// <param name="focusElement">The element that will be used as the focus point. Provide your implementation of <see cref="T:MonoGame.Framework.WpfInterop.WpfGame" /> here.</param>
        public WpfKeyboard(WpfGame focusElement)
        {
            if (focusElement == null) throw new ArgumentNullException(nameof(focusElement));
            _focusElement = focusElement;
        }

        /// <summary>Gets the active keyboardstate.</summary>
        /// <returns></returns>
        public KeyboardState GetState()
        {
            if (_focusElement.IsMouseDirectlyOver && System.Windows.Input.Keyboard.FocusedElement != _focusElement &&
                (WindowHelper.IsControlOnActiveWindow(_focusElement) && _focusElement.FocusOnMouseOver))
            {
                _focusElement.Focus();
            }
            return new KeyboardState(GetKeys(_focusElement));
        }

        private static Keys[] GetKeys(IInputElement focusElement)
        {
            byte[] keyStates = new byte[256];
            if (!NativeGetKeyboardState(keyStates)) throw new Win32Exception(Marshal.GetLastWin32Error());
            List<Keys> keysList = new List<Keys>();
            if (focusElement.IsKeyboardFocused)
            {
                for (int index = 8; index < keyStates.Length; ++index)
                {
                    byte num = keyStates[index];
                    if ((num & 128) != 0 && num != 0)
                        keysList.Add((Keys)index);
                }
            }
            return keysList.ToArray();
        }

        [DllImport("user32.dll", EntryPoint = "GetKeyboardState", SetLastError = true)]
        private static extern bool NativeGetKeyboardState([Out] byte[] keyStates);
    }
}
