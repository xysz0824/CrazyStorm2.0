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
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    /// <summary>
    /// LayerSetting.xaml 的交互逻辑
    /// </summary>
    public partial class LayerSetting : Window
    {
        #region Private Members
        Layer layer;
        int selectedColor;
        #endregion

        #region Constructor
        public LayerSetting(Layer layer)
        {
            this.layer = layer;
            InitializeComponent();
            InitializeSetting();
        }
        #endregion

        #region Private Methods
        void InitializeSetting()
        {
            Setting.DataContext = layer;
            var item = ColorPalette.Children[(int)layer.Color] as Label;
            item.BorderThickness = new Thickness(1);
        }
        #endregion

        #region Window EventHandler
        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectedColor = ColorPalette.Children.IndexOf(sender as UIElement);
            foreach (Label item in ColorPalette.Children)
                    item.BorderThickness = new Thickness(Convert.ToInt32(item == sender));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int beginFrame, totalFrame;
            if (string.IsNullOrWhiteSpace(LayerName.Text))
            {
                MessageBox.Show("图层名称不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Int32.TryParse(BeginFrame.Text, out beginFrame) && Int32.TryParse(EndFrame.Text, out totalFrame))
            {
                layer.Color = (LayerColor)selectedColor;
                layer.BeginFrame = beginFrame;
                layer.TotalFrame = totalFrame;
                layer.Name = LayerName.Text;
                this.Close();
            }
            else
                MessageBox.Show("数值填入有误，请重试", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        #endregion
    }
}
