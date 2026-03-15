using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExpressionHelper.InitializeTranslationCache();
            FontPropertyAttribute.FontValidator = fontName => FontRegistry.TryGetFontPath(fontName, out _);
            FontRegistry.EnsureInitialized();

            var startupWindow = new StartupWindow();
            MainWindow = startupWindow;
            startupWindow.Show();
        }
    }
}
