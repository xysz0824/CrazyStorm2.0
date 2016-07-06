/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Methods
        void UpdateCommandStackStatus()
        {
            var redoPeek = commandStacks[selectedSystem].RedoPeek();
            var undoPeek = commandStacks[selectedSystem].UndoPeek();
            UndoButton.IsEnabled = redoPeek != null ? true : false;
            RedoButton.IsEnabled = undoPeek != null ? true : false;
            UndoItem.IsEnabled = UndoButton.IsEnabled;
            RedoItem.IsEnabled = RedoButton.IsEnabled;
        }
        void UpdateEditStatus()
        {
            CutButton.IsEnabled = selectedComponents.Count > 0;
            CopyButton.IsEnabled = CutButton.IsEnabled;
            PasteButton.IsEnabled = clipBoard.Count > 0;
            CutItem.IsEnabled = CutButton.IsEnabled;
            CopyItem.IsEnabled = CopyButton.IsEnabled;
            PasteItem.IsEnabled = PasteButton.IsEnabled;
            DelItem.IsEnabled = CutButton.IsEnabled;
        }
        void SelectAll()
        {
            var set = new List<Component>();
            foreach (var layer in selectedSystem.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);

            SelectComponents(set, false);
        }
        void Undo()
        {
            var stack = commandStacks[selectedSystem];
            var command = stack.RedoPop();
            command.Undo(stack);
            UpdateSelectedStatus();
        }
        void Redo()
        {
            var stack = commandStacks[selectedSystem];
            var command = stack.UndoPop();
            command.Redo(stack);
            UpdateSelectedStatus();
        }
        void Cut()
        {
            clipBoard.Clear();
            clipBoard.AddRange(selectedComponents);
            Del();
        }
        void Copy()
        {
            clipBoard.Clear();
            clipBoard.AddRange(selectedComponents);
            UpdateSelectedStatus();
        }
        void Paste()
        {
            CancelAllSelection();
            new PasteComponentCommand().Do(commandStacks[selectedSystem], selectedSystem, selectedLayer, clipBoard);
            UpdateSelectedStatus();
        }
        void Del()
        {
            new DelComponentCommand().Do(commandStacks[selectedSystem], selectedSystem, selectedComponents);
            UpdateSelectedStatus();
        }
        void Find()
        {
            TabItem item;
            //Prevent from repeating tab of finder.  
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                item = LeftTabControl.Items[i] as TabItem;
                if (item.Content is FinderPanel)
                {
                    item.Focus();
                    return;
                }
            }
            //Cancel selection of components.
            CancelAllSelection();
            //Create finder panel.
            item = new TabItem();
            item.Style = (Style)FindResource("CanCloseStyle");
            var panel = new FinderPanel(selectedSystem);
            panel.OnSelectComponent += (Component component) =>
            {
                var set = new List<CrazyStorm.Core.Component>();
                set.Add(component);
                SelectComponents(set, true);
            };
            item.DataContext = panel;
            item.Content = panel;
            LeftTabControl.Items.Add(item);
            item.Focus();
        }
        #endregion

        #region Window EventHandlers
        private void SelectAllItem_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }
        private void UndoItem_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }
        private void RedoItem_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }
        private void CutItem_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }
        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }
        private void PasteItem_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }
        private void DelItem_Click(object sender, RoutedEventArgs e)
        {
            Del();
        }
        private void FindItem_Click(object sender, RoutedEventArgs e)
        {
            Find();
        }
        #endregion
    }
}
