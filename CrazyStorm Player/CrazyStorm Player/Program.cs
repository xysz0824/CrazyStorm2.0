/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace CrazyStorm_Player
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            LogHelper.Clear("Player Log.txt", VersionInfo.AppTitle);
            if (args.Length == 0)
                return;

            OutputArgs(args);
            using (var player = new StandalonePlayer())
            {
                player.InactiveSleepTime = new TimeSpan(0, 0, 0);
                player.Run();
            }
        }
        static void OutputArgs(string[] args)
        {
            StringBuilder argsInfo = new StringBuilder();
            for (int i = 0; i < args.Length; ++i)
            {
                argsInfo.Append(args[i]);
                argsInfo.Append(",");
            }
            LogHelper.Info(argsInfo.ToString());
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string exceptionMessage = e.ExceptionObject.ToString();
            LogHelper.Error(exceptionMessage);
            MessageBox.Show(exceptionMessage, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}
