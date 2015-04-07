/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CrazyStorm
{
    partial class Main
    {
        #region Window EventHandler
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            var command = (RoutedUICommand)e.Command;
            if (command.Text.EndsWith("Component"))
                if (selectedComponents.Count == 0)
                    e.CanExecute = false;
            switch (command.Text)
            {
                case "Undo":
                    e.CanExecute = UndoButton.IsEnabled;
                    break;
                case "Redo":
                    e.CanExecute = RedoButton.IsEnabled;
                    break;
            }
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var command = (RoutedUICommand)e.Command;
            var stack = commandStacks[selectedBarrage];
            switch (command.Text)
            {
                case "DelComponent":
                    new DelComponentCommand().Do(stack, selectedBarrage, selectedComponents);
                    break;
                case "UpComponent":
                    new MoveComponentCommand().Do(stack, selectedComponents, MoveStatus.Up, config.GridAlignment);
                    break;
                case "DownComponent":
                    new MoveComponentCommand().Do(stack, selectedComponents, MoveStatus.Down, config.GridAlignment);
                    break;
                case "LeftComponent":
                    new MoveComponentCommand().Do(stack, selectedComponents, MoveStatus.Left, config.GridAlignment);
                    break;
                case "RightComponent":
                    new MoveComponentCommand().Do(stack, selectedComponents, MoveStatus.Right, config.GridAlignment);
                    break;
                case "SelectAll":
                    SelectAll();
                    break;
                case "Undo":
                    Undo();
                    break;
                case "Redo":
                    Redo();
                    break;
            }
            UpdateComponent();
        }
        #endregion
    }
}
