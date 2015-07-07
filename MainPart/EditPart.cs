/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
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
        }
        void SelectAll()
        {
            SelectComponents(selectedParticle.Components.ToList(), false);
        }
        void Undo()
        {
            var stack = commandStacks[selectedParticle];
            var command = stack.RedoPop();
            command.Undo(stack);
            Update();
        }
        void Redo()
        {
            var stack = commandStacks[selectedParticle];
            var command = stack.UndoPop();
            command.Redo(stack);
            Update();
        }
        void Cut()
        {
            //TODO : Cut.
            Update();
        }
        void Copy()
        {
            //TODO : Copy.
            Update();
        }
        void Paste()
        {
            //TODO : Paste;
            Update();
        }
        #endregion

        #region Window EventHandler
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
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }
        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }
        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }
        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }
        #endregion
    }
}
