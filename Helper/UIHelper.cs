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
using System.Windows.Input;
using System.Windows.Media;

namespace CrazyStorm
{
    public static class UIHelper
    {
        static Style intellisenseListViewItemStyle;

        class IntellisenseContext
        {
            public Popup Popup;
            public UIElement Target;
            public TextBox TextBox;
            public ListView ListView;
            public List<string> EnumItems = new List<string>();
            public List<string> ExpressionItems = new List<string>();
            public TextChangedEventHandler TextChangedHandler;
            public KeyEventHandler PreviewKeyDownHandler;
        }

        public static readonly Color ErrorColor = Color.FromRgb(255, 190, 190);
        public static void ShowIntellisense(Popup popup, Type type, UIElement target)
        {
            ShowIntellisense(popup, type, target, null);
        }
        public static void ShowIntellisense(Popup popup, Type type, UIElement target, IEnumerable<string> expressionItems)
        {
            if (popup == null) return;
            HideIntellisense(popup);
            popup.PlacementTarget = target;
            popup.Placement = PlacementMode.Bottom;
            popup.PopupAnimation = PopupAnimation.Fade;
            var listView = new ListView();
            listView.SelectionMode = SelectionMode.Single;
            listView.Focusable = false;
            listView.ItemContainerStyle = GetIntellisenseListViewItemStyle();
            var context = new IntellisenseContext()
            {
                Popup = popup,
                Target = target,
                TextBox = ResolveTextBox(target),
                ListView = listView,
            };
            if (type == typeof(bool))
            {
                context.EnumItems.Add((string)popup.FindResource($"{true}Str"));
                context.EnumItems.Add((string)popup.FindResource($"{false}Str"));
            }
            else if (type != null && (type.IsEnum || type.IsSubclassOf(typeof(Enum))))
            {
                Array array = Enum.GetValues(type);
                foreach (var item in array)
                {
                    context.EnumItems.Add((string)popup.FindResource($"{item}Str"));
                }
            }

            if (expressionItems != null)
            {
                foreach (var item in expressionItems)
                {
                    if (!string.IsNullOrEmpty(item) && !context.ExpressionItems.Contains(item))
                    {
                        context.ExpressionItems.Add(item);
                    }
                }
            }

            if (context.TextBox != null)
            {
                context.TextChangedHandler = (sender, args) => RefreshIntellisense(context);
                context.PreviewKeyDownHandler = (sender, args) => HandleIntellisenseKeyDown(context, args);
                context.TextBox.TextChanged += context.TextChangedHandler;
                context.TextBox.PreviewKeyDown += context.PreviewKeyDownHandler;
            }

            listView.PreviewMouseLeftButtonDown += (sender, args) =>
            {
                args.Handled = true;
                var item = VisualHelper.VisualUpwardSearch<ListViewItem>(args.OriginalSource as DependencyObject) as ListViewItem;
                if (item == null) return;
                context.ListView.SelectedItem = item.Content;
                if (context.TextBox == null) return;
                if (args.ClickCount >= 2)
                {
                    ApplySuggestion(context, item.Content as string);
                    CloseIntellisensePopup(context);
                }
            };

            popup.Tag = context;
            popup.Child = listView;
            RefreshIntellisense(context);
        }
        public static void HideIntellisense(Popup popup)
        {
            if (popup == null) return;
            var context = popup.Tag as IntellisenseContext;
            if (context != null && context.TextBox != null)
            {
                if (context.TextChangedHandler != null)
                {
                    context.TextBox.TextChanged -= context.TextChangedHandler;
                }
                if (context.PreviewKeyDownHandler != null)
                {
                    context.TextBox.PreviewKeyDown -= context.PreviewKeyDownHandler;
                }
            }
            popup.Tag = null;
            popup.Child = null;
            popup.IsOpen = false;
        }
        public static IList<string> BuildExpressionIntellisenseItems(Expression.Environment environment)
        {
            var items = new List<string>();
            var itemSet = new HashSet<string>();
            if (environment == null) return items;

            foreach (var property in environment.Properties.Keys)
            {
                var display = ExpressionHelper.TranslateProperty(property);
                if (string.IsNullOrEmpty(display)) display = property;
                if (itemSet.Add(display)) items.Add(display);
            }
            foreach (var local in environment.Locals.Keys)
            {
                if (!string.IsNullOrEmpty(local) && itemSet.Add(local)) items.Add(local);
            }
            foreach (var global in environment.Globals.Keys)
            {
                if (!string.IsNullOrEmpty(global) && itemSet.Add(global)) items.Add(global);
            }
            return items;
        }
        static Style GetIntellisenseListViewItemStyle()
        {
            if (intellisenseListViewItemStyle != null) return intellisenseListViewItemStyle;

            var itemStyle = new Style(typeof(ListViewItem));
            var app = Application.Current;
            if (app != null)
            {
                var defaultItemStyle = app.TryFindResource(typeof(ListViewItem)) as Style;
                if (defaultItemStyle != null)
                {
                    itemStyle.BasedOn = defaultItemStyle;
                }
            }
            itemStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4, 2, 4, 2)));
            intellisenseListViewItemStyle = itemStyle;
            return intellisenseListViewItemStyle;
        }
        static TextBox ResolveTextBox(UIElement target)
        {
            var textBox = target as TextBox;
            if (textBox != null) return textBox;
            var contentControl = target as ContentControl;
            if (contentControl != null) return contentControl.Content as TextBox;
            return null;
        }
        static void RefreshIntellisense(IntellisenseContext context)
        {
            if (context == null || context.Popup == null || context.ListView == null) return;
            context.ListView.Items.Clear();
            var prefix = string.Empty;
            bool hasPrefix = false;
            if (context.TextBox != null)
            {
                hasPrefix = TryGetIdentifierPrefix(context.TextBox, out prefix);
            }

            if (hasPrefix)
            {
                foreach (var item in context.EnumItems)
                {
                    if (item.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.ListView.Items.Add(item);
                    }
                }
                foreach (var item in context.ExpressionItems)
                {
                    if (item.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                    {
                        context.ListView.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (var item in context.EnumItems)
                {
                    context.ListView.Items.Add(item);
                }
            }

            if (context.ListView.Items.Count > 0)
            {
                context.ListView.SelectedIndex = -1;
                context.Popup.IsOpen = true;
            }
            else
            {
                CloseIntellisensePopup(context);
            }
        }
        static bool TryGetIdentifierPrefix(TextBox textBox, out string prefix)
        {
            prefix = string.Empty;
            if (textBox == null) return false;
            int start;
            int end;
            int caret;
            GetIdentifierRange(textBox, out start, out end, out caret);
            if (caret <= start) return false;
            prefix = textBox.Text.Substring(start, caret - start);
            return !string.IsNullOrEmpty(prefix);
        }
        static void GetIdentifierRange(TextBox textBox, out int start, out int end, out int caret)
        {
            caret = textBox.CaretIndex;
            var text = textBox.Text ?? string.Empty;
            if (caret < 0) caret = 0;
            if (caret > text.Length) caret = text.Length;
            start = caret;
            while (start > 0 && IsIdentifierChar(text[start - 1]))
            {
                start--;
            }
            end = caret;
            while (end < text.Length && IsIdentifierChar(text[end]))
            {
                end++;
            }
        }
        static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }
        static void HandleIntellisenseKeyDown(IntellisenseContext context, KeyEventArgs e)
        {
            if (context == null || context.ListView == null || context.TextBox == null) return;
            if (!context.Popup.IsOpen || context.ListView.Items.Count == 0) return;

            if (e.Key == Key.Down)
            {
                var next = context.ListView.SelectedIndex < 0 ? 0 : context.ListView.SelectedIndex + 1;
                if (next >= context.ListView.Items.Count) next = context.ListView.Items.Count - 1;
                context.ListView.SelectedIndex = next;
                context.ListView.ScrollIntoView(context.ListView.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                var next = context.ListView.SelectedIndex < 0 ? context.ListView.Items.Count - 1 : context.ListView.SelectedIndex - 1;
                if (next < 0) next = 0;
                context.ListView.SelectedIndex = next;
                context.ListView.ScrollIntoView(context.ListView.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (context.ListView.SelectedIndex < 0) return;
                var selected = context.ListView.SelectedItem as string;
                if (!string.IsNullOrEmpty(selected))
                {
                    ApplySuggestion(context, selected);
                    CloseIntellisensePopup(context);
                    e.Handled = true;
                }
            }
        }
        static void CloseIntellisensePopup(IntellisenseContext context)
        {
            if (context == null || context.Popup == null) return;
            if (context.ListView != null)
            {
                context.ListView.SelectedIndex = -1;
            }
            context.Popup.IsOpen = false;
        }
        static void ApplySuggestion(IntellisenseContext context, string suggestion)
        {
            if (context == null || context.TextBox == null || string.IsNullOrEmpty(suggestion)) return;

            int start;
            int end;
            int caret;
            GetIdentifierRange(context.TextBox, out start, out end, out caret);
            var text = context.TextBox.Text ?? string.Empty;
            context.TextBox.Text = text.Substring(0, start) + suggestion + text.Substring(end);
            context.TextBox.CaretIndex = start + suggestion.Length;
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
