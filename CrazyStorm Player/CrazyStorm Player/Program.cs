/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
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
            if (args.Length == 0)
                return;

            LogHelper.Clear(Environment.CurrentDirectory + "\\Player Log.txt", VersionInfo.AppTitle);
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            using (Player player = new Player())
            {
                player.Run();
            }
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
