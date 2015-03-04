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
        void OpenLayerSetting()
        {
            LayerSetting window = new LayerSetting();
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
        private void LayerColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
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
                    OpenLayerSetting();
                    break;
            }
        }
        #endregion
    }
}
