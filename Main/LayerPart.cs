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
using System.Windows.Automation.Peers;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Layer selectedLayer;
        #endregion

        #region Private Methods
        void CreateNewLayer()
        {
            if (selectedParticle.Layers.Count < 6)
                new AddLayerCommand().Do(commandStacks[selectedParticle], selectedParticle);
            else
                MessageBox.Show((string)FindResource("CanNotAddMoreLayer"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void DeleteSelectedLayer()
        {
            if (selectedParticle.Layers.Count > 1)
            {
                new DelLayerCommand().Do(commandStacks[selectedParticle], selectedParticle, selectedLayer);
                Update();
            }
            else
                MessageBox.Show((string)FindResource("CanNotDeleteAllLayer"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySelectedLayer()
        {
            //TODO : Copy function.
        }
        void OpenSelectedLayerSetting()
        {
            Window window = new LayerSetting(commandStacks[selectedParticle], selectedLayer);
            window.ShowDialog();
        }
        #endregion

        #region Window EventHandler
        private void TimeAxis_MouseMove(object sender, MouseEventArgs e)
        {
            var scrollviewer = VisualHelper.GetVisualChild<ScrollViewer>(TimeAxis);
            //Display the frame that mouse pointed on tooltip.
            var pos = e.GetPosition(TimeAxis);
            var textBlock = axisTip.Content as TextBlock;
            textBlock.Text = ((int)(pos.X + scrollviewer.HorizontalOffset + 1) / 3).ToString();
            axisTip.HorizontalOffset = pos.X + 20;
            axisTip.VerticalOffset = pos.Y + 20;
        }
        private void NewLayer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CreateNewLayer();
        }
        private void LayerTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && LayerTree.SelectedItem != null)
                OpenSelectedLayerSetting();
        }
        private void LayerTree_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Maintain selected layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedParticle.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
                selectedLayer = null;

            CopyLayerItem.IsEnabled = selectedLayer != null ? true : false;
            DeleteLayerItem.IsEnabled = CopyLayerItem.IsEnabled;
            SetLayerItem.IsEnabled = CopyLayerItem.IsEnabled;
        }
        private void LayerVisible_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Set the visibllity of layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedParticle.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
            {
                selectedLayer = null;
                return;
            }
            var visible = sender as Label;
            selectedLayer.Visible = visible.Opacity == 0 ? true : false;
            Update();
        }
        private void LayerDown_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Move down selected layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedParticle.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
            {
                selectedLayer = null;
                return;
            }
            int index = selectedParticle.Layers.IndexOf(selectedLayer);
            if (index != selectedParticle.Layers.Count - 1)
            {
                var temp = selectedParticle.Layers[index];
                selectedParticle.Layers[index] = selectedParticle.Layers[index + 1];
                selectedParticle.Layers[index + 1] = temp;
            }
        }
        private void LayerUp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Move up selected layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedParticle.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
            {
                selectedLayer = null;
                return;
            }
            int index = selectedParticle.Layers.IndexOf(selectedLayer);
            if (index != 0)
            {
                var temp = selectedParticle.Layers[index];
                selectedParticle.Layers[index] = selectedParticle.Layers[index - 1];
                selectedParticle.Layers[index - 1] = temp;
            }
        }
        private void LayerColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Set the color of layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedParticle.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
            {
                selectedLayer = null;
                return;
            }
            if (Enum.IsDefined(typeof(LayerColor), selectedLayer.Color + 1))
            {
                new SetLayerCommand().Do(commandStacks[selectedParticle], selectedLayer,
                    (LayerColor)selectedLayer.Color + 1,
                    selectedLayer.BeginFrame,
                    selectedLayer.TotalFrame,
                    selectedLayer.Name);
            }
            else
            {
                selectedLayer.Color = LayerColor.Blue;
                new SetLayerCommand().Do(commandStacks[selectedParticle], selectedLayer,
                    LayerColor.Blue,
                    selectedLayer.BeginFrame,
                    selectedLayer.TotalFrame,
                    selectedLayer.Name);
            }
        }
        private void LayerMenu_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to the corresponding function of layer menu.
            var item = e.Source as MenuItem;
            switch (item.Name)
            {
                case "AddLayerItem":
                    CreateNewLayer();
                    break;
                case "DeleteLayerItem":
                    DeleteSelectedLayer();
                    break;
                case "CopyLayerItem":
                    CopySelectedLayer();
                    break;
                case "SetLayerItem":
                    OpenSelectedLayerSetting();
                    break;
            }
        }
        private void LayerShortSetting_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedLayerSetting();
        }
        private void LayerShortCopy_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedLayer();
        }
        private void LayerShortDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedLayer();
        }
        private void LayerElement_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualHelper.FocusItem<TreeViewItem>(e);
        }
        #endregion
    }
}
