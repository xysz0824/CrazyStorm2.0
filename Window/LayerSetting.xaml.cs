/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
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
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class LayerSetting : Window
    {
        #region Private Members
        CommandStack commandStack;
        Layer layer;
        int selectedColor;
        #endregion

        #region Constructor
        public LayerSetting(CommandStack commandStack, Layer layer)
        {
            this.commandStack = commandStack;
            this.layer = layer;
            InitializeComponent();
            InitializeSetting();
        }
        #endregion

        #region Private Methods
        void InitializeSetting()
        {
            Setting.DataContext = layer;
            selectedColor = (int)layer.Color;
            var item = ColorPalette.Children[selectedColor] as Label;
            item.BorderThickness = new Thickness(1);
        }
        #endregion

        #region Window EventHandlers
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
                MessageBox.Show((string)FindResource("LayerNameCanNotBeEmptyStr"), (string)FindResource("TipTitleStr"), 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Int32.TryParse(BeginFrame.Text, out beginFrame) && Int32.TryParse(TotalFrame.Text, out totalFrame))
            {
                new SetLayerCommand().Do(commandStack, layer, 
                    (LayerColor)selectedColor, 
                    beginFrame, 
                    totalFrame, 
                    LayerName.Text);
                this.Close();
            }
            else
                MessageBox.Show((string)FindResource("ValueInvalidStr"), (string)FindResource("TipTitleStr"), 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        #endregion
    }
}
