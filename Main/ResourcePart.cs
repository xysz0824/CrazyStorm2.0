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
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    public partial class Main
    {
        //Implement control and interaction with UI.
        #region Window EventHandler
        private void ImageList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Focus pointed item when mouse right-button down.
            var item = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            //Open file dialog to add a new image.
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.Filter = (string)FindResource("ImageType");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var image = new Resource(open.SafeFileName, open.FileName);
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

        }
        private void AddScript_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
