/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
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
using System.ComponentModel;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Window EventHandler
        private void ImageList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Focus pointed item when mouse right-button down.
            var item = VisualHelper.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }
        private void AddImageItem_Click(object sender, RoutedEventArgs e)
        {
            //Open file dialog to add a new image.
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.Filter = (string)FindResource("ImageType");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var image = new FileResource(open.SafeFileName, open.FileName);
                    file.Images.Add(image);
                }
            }
        }
        private void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            //Delete selected image.
            var item = ImageList.SelectedItem as Resource;
            file.Images.Remove(item);
        }
        private void AddSound_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Add sound.
        }
        private void AddGlobal_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Add global.
        }
        #endregion
    }
}
