/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Diagnostics;
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
        private const string ProjectRepositoryUrl = "https://github.com/xysz0824/CrazyStorm2.0";

        #region Window EventHandlers
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToolBar; 
            var overflowGrid = tb.Template.FindName("OverflowGrid", tb) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Hidden;
            }   
        }
        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(ProjectRepositoryUrl)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, (string)FindResource("ErrorTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
