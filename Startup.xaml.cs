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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrazyStorm
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members
        Main mainWindow;
        DispatcherTimer dTimer;
        short frame = 0;
        #endregion

        #region Constructor
        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += 
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            LogHelper.Clear();
            InitializeComponent();

            dTimer = new DispatcherTimer();
            dTimer.Tick += dTimer_Tick;
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            dTimer.Start();

            mainWindow = new Main();
        }
        #endregion

        #region Private Methods
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string exceptionMessage = e.ExceptionObject.ToString();
            LogHelper.Error(exceptionMessage);
            MessageBox.Show(exceptionMessage, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void dTimer_Tick(object sender, EventArgs e)
        {
            //A frame approximately equal to 16ms(60 frames equal to one second)
            frame++;
            if (Opacity < 1.0f) 
                Opacity += 0.1f;

            if (frame >= 60)
            {
                dTimer.Stop();
                //Enter main ui
                mainWindow.Show();
                this.Close();
            }
        }
        #endregion
    }
}
