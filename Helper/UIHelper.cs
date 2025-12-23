/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CrazyStorm
{
    public static class UIHelper
    {
        public static readonly Color ErrorColor = Color.FromRgb(255, 190, 190);
        public static void ShowIntellisense(Popup popup, Type type, UIElement target)
        {
            if (popup == null) return;
            popup.PlacementTarget = target;
            popup.Placement = PlacementMode.Bottom;
            popup.PopupAnimation = PopupAnimation.Fade;
            var listView = new ListView();
            if (type == typeof(bool))
            {
                listView.Items.Add((string)popup.FindResource($"{true}Str"));
                listView.Items.Add((string)popup.FindResource($"{false}Str"));
            }
            else if (type.IsSubclassOf(typeof(Enum)))
            {
                Array array = Enum.GetValues(type);
                foreach (var item in array)
                    listView.Items.Add((string)popup.FindResource($"{item}Str"));
            }
            if (!listView.Items.IsEmpty)
            {
                listView.PreviewMouseLeftButtonDown += (sender, args) =>
                {
                    args.Handled = true;
                    if (!(args.OriginalSource is TextBlock)) return;
                    var textBox = target as TextBox;
                    if (textBox == null && target is ContentControl) textBox = (target as ContentControl).Content as TextBox;
                    if (textBox == null) return;
                    textBox.Text = (args.OriginalSource as TextBlock).Text;
                };
                popup.Child = listView;
                popup.IsOpen = true;
            }
        }
        public static void HideIntellisense(Popup popup)
        {
            if (popup == null) return;
            popup.Child = null;
            popup.IsOpen = false;
        }
        public static void SetErrorToolTip(Control source, ExpressionException error)
        {
            if (error != null)
            {
                var tip = new ToolTip();
                var tipText = new TextBlock();
                tipText.Text = (string)source.FindResource(error.Message + "Str");
                tip.Content = tipText;
                source.ToolTip = tip;
                source.Background = new SolidColorBrush(ErrorColor);
            }
            else
            {
                source.ToolTip = null;
                source.Background = new SolidColorBrush(Colors.White);
            }
        }
        public static bool HasError(Control source)
        {
            var brush = source.Background as SolidColorBrush;
            return source.ToolTip != null && brush != null && brush.Color == ErrorColor;
        }
    }
}
