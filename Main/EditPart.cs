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
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Methods
        void UpdateEdit()
        {
            var redoPeek = commandStacks[selectedParticle].RedoPeek();
            var undoPeek = commandStacks[selectedParticle].UndoPeek();
            //Update command buttons
            UndoButton.IsEnabled = redoPeek != null ? true : false;
            RedoButton.IsEnabled = undoPeek != null ? true : false;
            //Update command items
            UndoItem.IsEnabled = UndoButton.IsEnabled;
            RedoItem.IsEnabled = RedoButton.IsEnabled;
            //Update edit buttons
            CutButton.IsEnabled = selectedComponents.Count > 0;
            CopyButton.IsEnabled = CutButton.IsEnabled;
            //Update edit items
            CutItem.IsEnabled = CutButton.IsEnabled;
            CopyItem.IsEnabled = CopyButton.IsEnabled;
            DelItem.IsEnabled = CutItem.IsEnabled;
        }
        void SelectAll()
        {
            var set = new List<Component>();
            foreach (var layer in selectedParticle.Layers)
                if (layer.Visible)
                    set.AddRange(layer.Components);

            SelectComponents(set, false);
        }
        void Undo()
        {
            var stack = commandStacks[selectedParticle];
            var command = stack.RedoPop();
            command.Undo(stack);
            UpdateSelectedStatus();
        }
        void Redo()
        {
            var stack = commandStacks[selectedParticle];
            var command = stack.UndoPop();
            command.Redo(stack);
            UpdateSelectedStatus();
        }
        void Cut()
        {
            //TODO : Cut.
            UpdateSelectedStatus();
        }
        void Copy()
        {
            //TODO : Copy.
            UpdateSelectedStatus();
        }
        void Paste()
        {
            //TODO : Paste.
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
            //Clear selection of components.
            SelectComponents(null, true);
            //Create finder panel.
            item = new TabItem();
            item.Style = (Style)FindResource("CanCloseStyle");
            var panel = new FinderPanel(selectedParticle);
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
        private void FindItem_Click(object sender, RoutedEventArgs e)
        {
            Find();
        }
        #endregion
    }
}
