/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CrazyStorm.Core;

namespace CrazyStorm
{
    partial class Main
    {
        #region Private Members
        bool editingProperties;
        #endregion

        #region Window EventHandlers
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.ContinueRouting = true;
            e.CanExecute = true;
            //Check if properties are being edited.
            if (editingProperties)
            {
                e.CanExecute = false;
                return;
            }
            //If the name of command end with "Component", it need to check if it didn't have components selected.
            var command = (RoutedUICommand)e.Command;
            if (command.Text.EndsWith("Component"))
                e.CanExecute = selectedComponents.Count > 0;
            //Determine if these can execute according to the status of relevant buttons. 
            switch (command.Text)
            {
                case "Undo":
                    e.CanExecute = UndoButton.IsEnabled;
                    break;
                case "Redo":
                    e.CanExecute = RedoButton.IsEnabled;
                    break;
                case "Paste":
                    e.CanExecute = PasteButton.IsEnabled;
                    break;
            }
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var gridSize = config.GridSize;
            var command = (RoutedUICommand)e.Command;
            var stack = commandStacks[selectedSystem];
            switch (command.Text)
            {
                case "DelComponent":
                    Del();
                    break;
                case "UpComponent":
                    new MoveComponentCommand(MoveStatus.Up, gridSize, config.GridAlignment).Do(stack, selectedComponents, 
                        new Action(UpdateProperty));
                    break;
                case "DownComponent":
                    new MoveComponentCommand(MoveStatus.Down, gridSize, config.GridAlignment).Do(stack, selectedComponents,
                        new Action(UpdateProperty));
                    break;
                case "LeftComponent":
                    new MoveComponentCommand(MoveStatus.Left, gridSize, config.GridAlignment).Do(stack, selectedComponents,
                        new Action(UpdateProperty));
                    break;
                case "RightComponent":
                    new MoveComponentCommand(MoveStatus.Right, gridSize, config.GridAlignment).Do(stack, selectedComponents,
                        new Action(UpdateProperty));
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
                case "BindComponent":
                    BindComponent();
                    break;
                case "UnbindComponent":
                    UnbindComponent();
                    break;
                case "Find":
                    Find();
                    break;
                case "CutComponent":
                    Cut();
                    break;
                case "CopyComponent":
                    Copy();
                    break;
                case "Paste":
                    Paste();
                    break;
                case "New":
                    New();
                    break;
                case "Open":
                    Open();
                    break;
                case "Save":
                    Save();
                    break;
                case "SaveTo":
                    SaveTo();
                    break;
                case "PlayCurrent":
                    PlayCurrent();
                    break;
            }
        }
        #endregion
    }
}
