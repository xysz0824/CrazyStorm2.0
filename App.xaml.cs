using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CrazyStorm
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExpressionHelper.InitializeTranslationCache();

            var startupWindow = new StartupWindow();
            MainWindow = startupWindow;
            startupWindow.Show();
        }
    }
}
