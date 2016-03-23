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
using System.Windows.Input;
using CrazyStorm.Core;

namespace CrazyStorm
{
    partial class Main
    {
        #region Window EventHandler
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.ContinueRouting = true;
            e.CanExecute = true;
            //Check if it didn't select component the relevant command will be prohibited.
            var command = (RoutedUICommand)e.Command;
            if (command.Text.EndsWith("Component"))
                if (selectedComponents.Count == 0)
                    e.CanExecute = false;
            //Determine if these can execute by the status of relevant buttons. 
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
            var gridSize = config.GridSize;
            var command = (RoutedUICommand)e.Command;
            var stack = commandStacks[selectedParticle];
            switch (command.Text)
            {
                case "DelComponent":
                    new DelComponentCommand().Do(stack, selectedParticle, selectedComponents);
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
            }
            Update();
        }
        #endregion
    }
}
