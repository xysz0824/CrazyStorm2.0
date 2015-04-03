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
        //Implement control and interaction with UI.
        #region Window EventHandler
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            var command = (RoutedUICommand)e.Command;
            switch (command.Text)
            {
                case "DelComponent":
                    if (selectedComponents.Count == 0)
                        e.CanExecute = false;
                    break;
            }
        }
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var command = (RoutedUICommand)e.Command;
            switch (command.Text)
            {
                case "DelComponent":
                    new DelComponentCommand().Do(commandStacks[selectedBarrage], selectedBarrage, selectedComponents);
                    break;
            }
            UpdateComponent();
        }
        #endregion
    }
}
