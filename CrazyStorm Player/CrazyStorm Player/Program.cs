/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm.Core;
using CrazyStorm.Common;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace CrazyStorm_Player
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            LogHelper.Clear("Player Log.txt", VersionInfo.AppTitle);
            if (args.Length == 0)
                return;

            OutputArgs(args);
            using (Player player = new Player())
            {
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
