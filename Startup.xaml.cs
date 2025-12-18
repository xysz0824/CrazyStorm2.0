/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrazyStorm
{
    public partial class StartupWindow : Window
    {
        #region Private Members
        Main mainWindow;
        DispatcherTimer dTimer;
        short frame = 0;
        #endregion

        #region Constructor
        public StartupWindow()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            AppDomain.CurrentDomain.UnhandledException += 
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            InitializeComponent();

            dTimer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher);
            dTimer.Tick += dTimer_Tick;
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            dTimer.Start();
        }
        #endregion

        #region Private Methods
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string exceptionMessage = e.ExceptionObject.ToString();
            LogHelper.Error(exceptionMessage);
            MessageBox.Show(exceptionMessage, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void dTimer_Tick(object sender, EventArgs e)
        {
            //A frame approximately equals to 16ms(60 frames equal to one second)
            frame++;
            if (Opacity >= 1f)
            {
                string[] args = Environment.GetCommandLineArgs();
                LogHelper.Clear("Log.txt", VersionInfo.AppTitle);
                Environment.CurrentDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                mainWindow = new Main();
                mainWindow.Initailize();
                if (args.Length >= 2)
                    mainWindow.OpenFile(args[1]);
                else
                    mainWindow.StartNewFile();

                mainWindow.Show();
                this.Close();
                dTimer.Stop();
            }
            Opacity = Math.Min(1f, Opacity + 0.1f);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null)
                return;

            Matrix m = source.CompositionTarget.TransformToDevice;

            double dpiScaleX = m.M11;
            double dpiScaleY = m.M22;
            Left += (1.0d - 1.0d / dpiScaleX) * 0.5d * Width;
            Top += (1.0d - 1.0d / dpiScaleY) * 0.5d * Height;
            Width *= 1.0d / dpiScaleX;
            Height *= 1.0d / dpiScaleY;
        }
        #endregion
    }
}
