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
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Methods
        void CreateNewLayer()
        {
            selectedBarrage.Layers.Add(new Layer("New Layer"));
        }
        void DeleteSelectedLayer()
        {
            if (selectedBarrage.Layers.Count > 1)
                selectedBarrage.Layers.Remove(selectedLayer);
            else
                MessageBox.Show("不能删除所有图层", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySelectedLayer()
        {

        }
        void OpenSelectedLayerSetting()
        {
            LayerSetting window = new LayerSetting(selectedLayer);
            window.Owner = this;
            window.ShowDialog();
        }
        #endregion

        #region Window EventHandler
        private void TimeAxis_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(TimeAxis);
            var textBlock = axisTip.Content as TextBlock;
            textBlock.Text = ((int)(pos.X + 1) / 3).ToString();
            axisTip.HorizontalOffset = pos.X + 20;
            axisTip.VerticalOffset = pos.Y + 20;
        }
        private void NewLayer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CreateNewLayer();
        }
        private void LayerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedBarrage.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
                selectedLayer = null;
            CopyLayer.IsEnabled = selectedLayer != null ? true : false;
            DeleteLayer.IsEnabled = CopyLayer.IsEnabled;
            SetLayer.IsEnabled = CopyLayer.IsEnabled;
        }
        private void LayerVisible_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var visible = sender as Label;
            selectedLayer.Visible = visible.Opacity == 0 ? true : false;
        }
        private void LayerDown_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedLayer == null)
                return;
            int index = selectedBarrage.Layers.IndexOf(selectedLayer);
            if (index != selectedBarrage.Layers.Count - 1)
            {
                var temp = selectedBarrage.Layers[index];
                selectedBarrage.Layers[index] = selectedBarrage.Layers[index + 1];
                selectedBarrage.Layers[index + 1] = temp;
            }
        }
        private void LayerUp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedLayer == null)
                return;
            int index = selectedBarrage.Layers.IndexOf(selectedLayer);
            if (index != 0)
            {
                var temp = selectedBarrage.Layers[index];
                selectedBarrage.Layers[index] = selectedBarrage.Layers[index - 1];
                selectedBarrage.Layers[index - 1] = temp;
            }
        }
        private void LayerColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedLayer == null)
                return;
            if (Enum.IsDefined(typeof(LayerColor), selectedLayer.Color + 1))
                selectedLayer.Color += 1;
            else
                selectedLayer.Color = LayerColor.BLUE;
        }
        private void LayerMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = e.Source as MenuItem;
            switch ((string)item.Header)
            {
                case "添加图层":
                    CreateNewLayer();
                    break;
                case "删除图层":
                    DeleteSelectedLayer();
                    break;
                case "复制图层":
                    CopySelectedLayer();
                    break;
                case "图层设置":
                    OpenSelectedLayerSetting();
                    break;
            }
        }
        private void LayerScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double offset = e.VerticalOffset;
            ListViewAutomationPeer lvap = new ListViewAutomationPeer(LayerAxis);
            var svap = lvap.GetPattern(PatternInterface.Scroll) as ScrollViewerAutomationPeer;
            var scroll = svap.Owner as ScrollViewer;
            scroll.ScrollToVerticalOffset((int)(offset / 16));
        }
        private void LayerShortSetting_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLayer != null)
            OpenSelectedLayerSetting();
        }
        private void LayerShortCopy_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLayer != null)
                CopySelectedLayer();
        }
        private void LayerShortDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLayer != null)
            DeleteSelectedLayer();
        }
        #endregion
    }
}
