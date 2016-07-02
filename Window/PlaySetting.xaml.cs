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
    /// Interaction logic for PlaySetting.xaml
    /// </summary>
    public partial class PlaySetting : Window
    {
        #region Private Members
        Config config;
        TextBox[] selfSettingBoxes;
        #endregion

        #region Constructor
        public PlaySetting(Config config)
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
            selfSettingBoxes = new TextBox[] { SelfStartX, SelfStartY, SelfWidth, SelfHeight, SelfCenterX, SelfCenterY,
                SelfFrames, SelfDelay, SelfRadius};
            if (config.ScreenCenter)
                ScreenCenter.IsChecked = true;
            else
                CustomCenter.IsChecked = true;

            if (!string.IsNullOrWhiteSpace(config.SelfSetting))
            {
                string[] split = config.SelfSetting.Split(',');
                if (split.Length == selfSettingBoxes.Length)
                {
                    for (int i = 0;i < split.Length;++i)
                    {
                        int test;
                        if (Int32.TryParse(split[i], out test) && test >= 0)
                            selfSettingBoxes[i].Text = split[i];
                    }
                }
            }
        }
        #endregion

        #region Window EventHandlers
        private void PlayerBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                open.Filter = (string)FindResource("PlayerTypeStr");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    PlayerPath.Text = open.FileName;
            }
        }
        private void SelfImageBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                open.Filter = (string)FindResource("ImageTypeStr");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SelfImagePath.Text = open.FileName;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int particleMaximum, curveParticleMaximum;
            int centerX, centerY;
            if (Int32.TryParse(ParticleMaximum.Text, out particleMaximum) && 
                Int32.TryParse(CurveParticleMaximum.Text, out curveParticleMaximum) && 
                Int32.TryParse(CenterX.Text, out centerX) && Int32.TryParse(CenterY.Text, out centerY))
            {
                config.PlayerPath = PlayerPath.Text;
                config.ParticleMaximum = particleMaximum;
                config.CurveParticleMaximum = curveParticleMaximum;
                config.ScreenCenter = ScreenCenter.IsChecked == true;
                config.CenterX = centerX;
                config.CenterY = centerY;
                config.SelfImagePath = SelfImagePath.Text;
            }
            else
            {
                MessageBox.Show((string)FindResource("ValueInvalidStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            config.SelfSetting = string.Empty;
            for (int i = 0; i < selfSettingBoxes.Length;++i)
            {
                if (string.IsNullOrWhiteSpace(selfSettingBoxes[i].Text))
                {
                    config.SelfSetting += "0,";
                    continue;
                }
                int value;
                if (Int32.TryParse(selfSettingBoxes[i].Text, out value) && value >= 0)
                {
                    config.SelfSetting += selfSettingBoxes[i].Text + ",";
                }
                else
                {
                    MessageBox.Show((string)FindResource("ValueInvalidStr"), (string)FindResource("TipTitleStr"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            config.SelfSetting = config.SelfSetting.TrimEnd(',');
            config.Save();
            this.Close();
        }
        #endregion
    }
}
