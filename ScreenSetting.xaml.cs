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
        #region Private Members
        Config config;
        #endregion

        #region Constructor
        public ScreenSetting(Config config)
        {
            this.config = config;
            InitializeComponent();
            InitializeSetting();
        }
        #endregion

        #region Private Methods
        void InitializeSetting()
        {
            Setting.DataContext = config;
            if (config.GridAlignment)
                GridOpen.IsChecked = true;
            else
                GridClose.IsChecked = true;

            if (config.CenterDisplay)
                CenterOpen.IsChecked = true;
            else
                CenterClose.IsChecked = true;
        }
        #endregion

        #region Window EventHandlers
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.Filter = (string)FindResource("BackGroundImageType");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    ImagePath.Text = open.FileName;

            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int width, height;
            if (Int32.TryParse(Width.Text, out width) && Int32.TryParse(Height.Text, out height))
            {
                config.ScreenWidth = width;
                config.ScreenHeight = height;
                config.ImagePath = ImagePath.Text;
                if (GridOpen.IsChecked.HasValue && GridOpen.IsChecked.Value == true)
                    config.GridAlignment = true;
                else
                    config.GridAlignment = false;

                if (CenterOpen.IsChecked.HasValue && CenterOpen.IsChecked.Value == true)
                    config.CenterDisplay = true;
                else
                    config.CenterDisplay = false;

                this.Close();
            }
            else
                MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        #endregion
    }
}
