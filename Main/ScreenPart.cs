using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Window EventHandler
        private void ScreenSetting_Click(object sender, RoutedEventArgs e)
        {
            ScreenSetting window = new ScreenSetting();
            window.Owner = this;
            window.ShowDialog();
        }
        #endregion
    }
}
