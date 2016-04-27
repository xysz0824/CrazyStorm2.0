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
    public enum UpdateType
    {
        Add,
        Delete,
        Modify
    }
    public partial class Main
    {
        #region Private Methods
        void UpdateGlobals(UpdateType type, VariableResource variable, string newName = "")
        {
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                var panel = ((LeftTabControl.Items[i] as TabItem).Content as ScrollViewer).Content as PropertyPanel;
                panel.UpdateGlobals(type, variable, newName);
            }
        }
        #endregion

        #region Window EventHandlers
        private void ImageList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualHelper.FocusItem<TreeViewItem>(e);
        }
        private void SoundList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualHelper.FocusItem<TreeViewItem>(e);
        }
        private void AddImageItem_Click(object sender, RoutedEventArgs e)
        {
            (LeftTabControl.Items[1] as TabItem).Focus();
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
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
            var item = ImageList.SelectedItem as Resource;
            file.Images.Remove(item);
        }
        private void AddSound_Click(object sender, RoutedEventArgs e)
        {
            (LeftTabControl.Items[1] as TabItem).Focus();
            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                open.Filter = (string)FindResource("SoundType");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var sound = new FileResource(open.SafeFileName, open.FileName);
                    file.Sounds.Add(sound);
                }
            }
        }
        private void DeleteSound_Click(object sender, RoutedEventArgs e)
        {
            var item = SoundList.SelectedItem as Resource;
            file.Sounds.Remove(item);
        }
        private void AddGlobalItem_Click(object sender, RoutedEventArgs e)
        {
            (LeftTabControl.Items[1] as TabItem).Focus();
            AddVariable_Click(null, null);
        }
        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            var label = "Global_";
            for (int i = 0; ; ++i)
            {
                //To avoid repeating name, use number.
                var name = label + i;
                bool ok = true;
                for (int k = 0; k < file.Globals.Count; ++k)
                    if (file.Globals[k].Label == name)
                    {
                        ok = false;
                        break;
                    }

                if (ok)
                {
                    var newVar = new VariableResource(name);
                    file.Globals.Add(newVar);
                    UpdateGlobals(UpdateType.Add, newVar);
                    DeleteVariable.IsEnabled = true;
                    return;
                }
            }
        }
        private void DeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (VariableGrid.SelectedItem != null)
            {
                var item = VariableGrid.SelectedItem as VariableResource;
                file.Globals.Remove(item);
                UpdateGlobals(UpdateType.Delete, item);
                DeleteVariable.IsEnabled = file.Globals.Count > 0 ? true : false;
            }
        }
        private void VariableGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editItem = e.EditingElement.DataContext as VariableResource;
                var newValue = (e.EditingElement as TextBox).Text;
                if (e.Column.SortMemberPath == "Label")
                {
                    //Check the commit to avoid repeating name.
                    newValue = newValue.Trim();
                    foreach (var item in file.Globals)
                        if (item != editItem && item.Label == newValue)
                        {
                            MessageBox.Show((string)FindResource("NameRepeating"), (string)FindResource("TipTitle"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Cancel = true;
                            (e.EditingElement as TextBox).Text = editItem.Label;
                            return;
                        }
                    UpdateGlobals(UpdateType.Modify, editItem, newValue);
                }
                else if (e.Column.SortMemberPath == "Value")
                {
                    //Check the commit to avoid invalid value.
                    float value;
                    bool result = float.TryParse(newValue, out value);
                    if (!result)
                    {
                        MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                        (e.EditingElement as TextBox).Text = editItem.Value.ToString();
                        return;
                    }
                }
            }
        }
        #endregion
    }
}
