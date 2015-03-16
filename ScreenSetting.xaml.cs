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
using System.Windows.Shapes;

namespace CrazyStorm
{
    /// <summary>
    /// ScreenSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenSetting : Window
    {
        #region Constructor
        public ScreenSetting()
        {
            InitializeComponent();
        }
        #endregion

        #region Window EventHandlers
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.Filter = (string)FindResource("BackGroundImageType");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
