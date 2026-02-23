/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
using System.Windows.Threading;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        Layer selectedLayer;
        ScrollViewer axisScroll;
        ScrollViewer layerScroll;
        int selectedFrame = 1;
        bool timeAxisSelecting;
        bool timeAxisScrolling;
        Point timeAxisScrollPos;
        DispatcherTimer layerTimer;
        #endregion

        #region Private Methods
        void CreateNewLayer()
        {
            new AddLayerCommand((string)FindResource("NewLayerStr")).Do(commandStacks[selectedSystem], selectedSystem);
        }
        void DeleteSelectedLayer()
        {
            if (selectedSystem.Layers.Count > 1)
            {
                new DelLayerCommand().Do(commandStacks[selectedSystem], selectedSystem, selectedLayer);
                UpdateSelectedStatus();
            }
            else
                MessageBox.Show((string)FindResource("CanNotDeleteAllLayerStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySelectedLayer()
        {
            new CopyLayerCommand().Do(commandStacks[selectedSystem], selectedSystem, selectedLayer);
            UpdateScreen();
        }
        void OpenSelectedLayerSetting()
        {
            Window window = new LayerSetting(commandStacks[selectedSystem], selectedLayer);
            window.ShowDialog();
            window.Close();
        }
        void StartLayerTimer()
        {
            if (layerTimer == null)
            {
                layerTimer = new DispatcherTimer(DispatcherPriority.Send, Dispatcher);
                layerTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
                layerTimer.Tick += LayerTimer_Tick;
            }
            layerTimer.Start();
            FitAxisScroll(selectedFrame);
        }
        void FitAxisScroll(int frame)
        {
            TimeScalePointerFrame.Content = frame;
            TimeScalePointerTransform.X = (frame - 1) * 3 - axisScroll.HorizontalOffset;
            if (TimeScalePointerTransform.X >= TimeScale.ActualWidth)
            {
                axisScroll.ScrollToHorizontalOffset(axisScroll.HorizontalOffset + TimeScalePointerTransform.X - TimeScale.ActualWidth + 3);
            }
            else if (TimeScalePointerTransform.X <= 0)
            {
                axisScroll.ScrollToHorizontalOffset(axisScroll.HorizontalOffset + TimeScalePointerTransform.X);
            }
        }
        void StopLayerTimer()
        {
            layerTimer?.Stop();
            layerTimer = null;
            selectedFrame = 1;
            FitAxisScroll(selectedFrame);
        }
        void PauseLayerTimer()
        {
            layerTimer?.Stop();
        }
        void JumpToFrame(int frame)
        {
            selectedFrame = frame;
            FitAxisScroll(selectedFrame);
        }
        #endregion

        #region Window EventHandlers
        private void TimeAxis_Loaded(object sender, RoutedEventArgs e)
        {
            axisScroll = VisualHelper.VisualDownwardSearch<ScrollViewer>(LayerAxis) as ScrollViewer;
            layerScroll = VisualHelper.VisualDownwardSearch<ScrollViewer>(LayerTree) as ScrollViewer;
            axisScroll.ScrollChanged += (object s, ScrollChangedEventArgs args) =>
            {
                //Need this for strange display problem
                axisScroll.ScrollToHorizontalOffset(Math.Max(1, axisScroll.HorizontalOffset));
                var imageBruch = TimeScale.Background as ImageBrush;
                imageBruch.Viewport = new Rect(0 - axisScroll.HorizontalOffset, 0,
                    imageBruch.Viewport.Width, imageBruch.Viewport.Height);
                imageBruch = TimeAxis.Background as ImageBrush;
                imageBruch.Viewport = new Rect(0 - axisScroll.HorizontalOffset, -2 - axisScroll.VerticalOffset,
                    imageBruch.Viewport.Width, imageBruch.Viewport.Height);
                if (layerTimer == null || !layerTimer.IsEnabled)
                {
                    TimeScalePointerTransform.X = (selectedFrame - 1) * 3 - axisScroll.HorizontalOffset;
                }
                layerScroll.ScrollToVerticalOffset(axisScroll.VerticalOffset);
                layerScroll.RenderTransform = new TranslateTransform(0, 0);
                if (layerScroll.ScrollableHeight < axisScroll.VerticalOffset)
                {
                    layerScroll.RenderTransform = new TranslateTransform(0, layerScroll.ScrollableHeight - axisScroll.VerticalOffset);
                }
                //Need manual refresh for strange display problem
                LayerAxis.Items.Refresh();
            };
        }
        private void LayerTimer_Tick(object sender, EventArgs e)
        {
            if (player == null) return;
            FitAxisScroll((int)player.PlayerImpl.CurrentFrame);
        }
        private void TimeScale_MouseMove(object sender, MouseEventArgs e)
        {
            //Display the frame that mouse pointed on tooltip.
            var pos = e.GetPosition(TimeAxis);
            var frame = ((int)(pos.X + axisScroll.HorizontalOffset + 1) / 3) + 1;
            if (frame >= selectedSystem.TotalFrame)
            {
                frame = selectedSystem.TotalFrame;
                pos.X = (frame - 1) * 3;
            }
            else if (frame <= 1)
            {
                frame = 1;
                pos.X = 0;
            }
            var textBlock = axisTip.Content as TextBlock;
            textBlock.Text = frame.ToString();
            axisTip.HorizontalOffset = pos.X + 20;
            axisTip.VerticalOffset = pos.Y + 20;
            if (timeAxisSelecting)
            {
                selectedFrame = frame;
                TimeScalePointerFrame.Content = selectedFrame.ToString();
                TimeScalePointerTransform.X = (selectedFrame - 1) * 3 - axisScroll.HorizontalOffset;
                if (player != null) player.PlayerImpl.CurrentFrame = selectedFrame;
            }
        }
        private void TimeScale_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timeAxisSelecting = true;
            TimeScale.CaptureMouse();
            TimeScale_MouseMove(sender, e);
        }
        private void TimeScale_MouseUp(object sender, MouseButtonEventArgs e)
        {
            timeAxisSelecting = false;
            TimeScale.ReleaseMouseCapture();
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
            //Select layer.
            if (LayerTree.SelectedItem != null)
            {
                selectedLayer = selectedSystem.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
                //Press Ctrl key to select all components in this layer.
                if (Keyboard.Modifiers == ModifierKeys.Control && selectedLayer.Visible)
                {
                    var set = new List<Component>();
                    foreach (var component in selectedLayer.Components)
                        set.Add(component);

                    SelectComponents(set, false);
                }
            }
        }
        private void LayerVisible_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Set the visibllity of layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedSystem.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
                return;

            var visible = sender as Grid;
            selectedLayer.Visible = visible.Opacity == 0;
            UpdateSelectedStatus();
        }
        private void LayerDown_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Move down selected layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedSystem.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
                return;

            int index = selectedSystem.Layers.IndexOf(selectedLayer);
            if (index != selectedSystem.Layers.Count - 1)
            {
                var temp = selectedSystem.Layers[index];
                selectedSystem.Layers[index] = selectedSystem.Layers[index + 1];
                selectedSystem.Layers[index + 1] = temp;
            }
        }
        private void LayerUp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Move up selected layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedSystem.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
                return;

            int index = selectedSystem.Layers.IndexOf(selectedLayer);
            if (index != 0)
            {
                var temp = selectedSystem.Layers[index];
                selectedSystem.Layers[index] = selectedSystem.Layers[index - 1];
                selectedSystem.Layers[index - 1] = temp;
            }
        }
        private void LayerColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Set the color of layer.
            if (LayerTree.SelectedItem != null)
                selectedLayer = selectedSystem.Layers[LayerTree.Items.IndexOf(LayerTree.SelectedItem)];
            else
                return;

            if (Enum.IsDefined(typeof(LayerColor), selectedLayer.Color + 1))
            {
                new SetLayerCommand().Do(commandStacks[selectedSystem], selectedLayer,
                    (LayerColor)selectedLayer.Color + 1,
                    selectedLayer.BeginFrame,
                    selectedLayer.TotalFrame,
                    selectedLayer.Name);
            }
            else
            {
                selectedLayer.Color = LayerColor.Blue;
                new SetLayerCommand().Do(commandStacks[selectedSystem], selectedLayer,
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
        private void LayerAxis_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = -e.Delta;
            axisScroll.ScrollToVerticalOffset(axisScroll.VerticalOffset + delta);
            e.Handled = true;
        }
        private void LayerAxis_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border) return;
            timeAxisScrolling = true;
            timeAxisScrollPos = e.GetPosition(LayerAxis);
            LayerAxis.Cursor = Cursors.SizeWE;
            LayerAxis.CaptureMouse();
            e.Handled = true;
        }
        private void LayerAxis_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!timeAxisScrolling) return;
            var pos = e.GetPosition(LayerAxis);
            var dx = pos.X - timeAxisScrollPos.X;
            axisScroll.ScrollToHorizontalOffset(axisScroll.HorizontalOffset - dx);
            timeAxisScrollPos = pos;
            e.Handled = true;
        }
        private void LayerAxis_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!timeAxisScrolling) return;
            timeAxisScrolling = false;
            LayerAxis.ReleaseMouseCapture();
            LayerAxis.Cursor = Cursors.Arrow;
            e.Handled = true;
        }
        #endregion
    }
}
