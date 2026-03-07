/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using CrazyStorm_Player;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        EmbeddedPlayer player;
        Label activeParticleCountLabel;
        Label statusTipLabel;
        StackPanel statusPanel;
        DispatcherTimer playTimer;
        #endregion

        #region Private Methods
        void GeneratePlayFile()
        {
            if (string.IsNullOrWhiteSpace(file.ResourceDirectory))
            {
                MessageBox.Show((string)FindResource("NeedSaveFirstStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            file.GeneratePlayFile(fileName);
            MessageBox.Show((string)FindResource("PlayFileSavedStr"), (string)FindResource("TipTitleStr"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        void PlayCurrent()
        {
            int particleSystemIndex = 0;
            for (int i = 0; i < file.ParticleSystems.Count; ++i)
            {
                if (selectedSystem == file.ParticleSystems[i])
                {
                    particleSystemIndex = i;
                    break;
                }
            }
            var screen = ParticleTabControl.SelectedItem as TabItem;
            if (screen != null)
            {
                var content = screen.Content as Canvas;
                var screenContent = VisualHelper.VisualDownwardSearch(content, "ScreenContent") as Canvas;
                activeParticleCountLabel = VisualHelper.VisualDownwardSearch(content, "ActiveParticleCount") as Label;
                statusTipLabel = VisualHelper.VisualDownwardSearch(content, "StatusTip") as Label;
                statusPanel = VisualHelper.VisualDownwardSearch(content, "StatusPanel") as StackPanel;
                foreach (Button button in statusPanel.Children)
                {
                    if ((string)button.Content == "0")
                    {
                        button.Background = new SolidColorBrush(Colors.White);
                        button.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(Colors.Transparent);
                        button.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
                activeParticleCountLabel.Visibility = Visibility.Visible;
                statusTipLabel.Visibility = Visibility.Visible;
                statusPanel.Visibility = Visibility.Visible;
                var config = screen.DataContext as Config;
                player = new EmbeddedPlayer();
                player.Width = config.ScreenWidth;
                player.Height = config.ScreenHeight;
                player.PlayerImpl = new PlayerImpl(config.ScreenWidth, config.ScreenHeight, config.FrameRate,
                    config.ParticleMaximum, config.CurveParticleMaximum);
                player.PlayerImpl.TypeLibraryPath = config.TypeLibraryPath;
                player.PlayerImpl.FrameOrientation = config.FrameOrientation;
                player.PlayerImpl.BackgroundPath = config.BackgroundPath;
                player.PlayerImpl.SelectedParticleSystemIndex = particleSystemIndex;
                player.PlayerImpl.ControllableImagePath = config.SelfImagePath;
                player.PlayerImpl.ControllableSetting = config.SelfSetting;
                player.PlayerImpl.Files = new List<File>();
                var generatedFile = new File();
                generatedFile.LoadPlayFile(file.GeneratePlayFile(), file.ResourceDirectory, CrazyStorm_Player.VersionInfo.BaseVersion);
                player.PlayerImpl.Files.Add(generatedFile);
                player.PlayerImpl.CurrentFrame = selectedFrame;
                screenContent.Background.Opacity = 0;
                (VisualHelper.VisualDownwardSearch(screenContent, "Grid") as Canvas).Visibility = Visibility.Hidden;
                (VisualHelper.VisualDownwardSearch(screenContent, "Center") as Image).Visibility = Visibility.Hidden;
                (VisualHelper.VisualDownwardSearch(screenContent, "ComponentLayer") as Canvas).Visibility = Visibility.Hidden;
                screenContent.Children.Add(player);
                Panel.SetZIndex(player, 1);
            }
        }
        void OpenPlaySetting()
        {
            Window window = new PlaySetting(config);
            window.ShowDialog();
            window.Close();
        }
        void SetPanelEnable(bool enable)
        {
            ComponentPanel.IsEnabled = enable;
            ResourcePanel.IsEnabled = enable;
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                var item = LeftTabControl.Items[i] as TabItem;
                if (item.DataContext is CrazyStorm.Core.Component)
                {
                    var scroll = item.Content as ScrollViewer;
                    var panel = scroll.Content as PropertyPanel;
                    panel.IsEnabled = enable;
                }
            }
            foreach (var window in propertyWindows.Values)
            {
                var scroll = window.Content as ScrollViewer;
                if (scroll == null) continue;

                var panel = scroll.Content as PropertyPanel;
                if (panel != null) panel.IsEnabled = enable;
            }
        }
        void StartPlayTimer()
        {
            StartLayerTimer();
            if (playTimer == null)
            {
                playTimer = new DispatcherTimer(DispatcherPriority.Send, Dispatcher);
                playTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
                playTimer.Tick += PlayTimer_Tick;
            }
            playTimer.Start();
        }
        void StopPlayTimer()
        {
            StopLayerTimer();
            playTimer?.Stop();
            playTimer = null;
        }
        void PausePlayTimer()
        {
            PauseLayerTimer();
            playTimer?.Stop();
        }
        bool TrySkipPlayerFrame(float targetFrame, bool replayFromStart)
        {
            if (player == null) return false;
            try
            {
                player.PlayerImpl.SkipFrame(targetFrame, replayFromStart);
                return true;
            }
            catch (PoolOverflowException ex)
            {
                MessageBox.Show(ex.Message, "!", MessageBoxButton.OK, MessageBoxImage.Error);
                StopItem_Click(null, null);
                return false;
            }
        }
        void OpenJumpToFrame()
        {
            var window = new JumpToFrame(selectedFrame, selectedSystem.TotalFrame);
            window.ShowDialog();
            if (!window.Confirmed) return;
            JumpToFrame(window.TargetFrame);
            if (player != null) TrySkipPlayerFrame(selectedFrame, true);
        }
        #endregion

        #region Window EventHandlers
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (activeParticleCountLabel != null) activeParticleCountLabel.Content = ParticleManager.ActiveParticleCount;
            if (player != null && player.HasError)
            {
                StopItem_Click(null, null);
            }
            else if (player != null && player.Pause)
            {
                var path = VisualHelper.VisualDownwardSearch<Path>(PlayButton) as Path;
                path.Data = (Geometry)FindResource("Play_Icon");
                path.Fill = (Brush)FindResource("PlayIconBrush");
                path.ToolTip = (string)FindResource("PlayStr");
                selectedFrame = (int)player.PlayerImpl.CurrentFrame;
                TimeAxis.IsHitTestVisible = true;
                if (config.CollapseLayerAxis) LayerAxisDefinition.Height = new GridLength(94);
                ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Auto);
                var screen = ParticleTabControl.SelectedItem as TabItem;
                if (screen != null)
                {
                    var content = screen.Content as Canvas;
                    var screenContent = VisualHelper.VisualDownwardSearch(content, "ScreenContent") as Canvas;
                    (VisualHelper.VisualDownwardSearch(screenContent, "Grid") as Canvas).Visibility = Visibility.Visible;
                    (VisualHelper.VisualDownwardSearch(screenContent, "ComponentLayer") as Canvas).Visibility = Visibility.Visible;
                }
                Panel.SetZIndex(player, -1);
                PausePlayTimer();
            }
        }
        private void GeneratePlayFile_Click(object sender, RoutedEventArgs e)
        {
            file.UpdateResource();
            GeneratePlayFile();
        }
        private void PlayItem_Click(object sender, RoutedEventArgs e)
        {
            var path = VisualHelper.VisualDownwardSearch<Path>(PlayButton) as Path;
            if (player == null)
            {
                file.UpdateResource();
                PlayCurrent();
                path.Data = (Geometry)FindResource("Pause_Icon");
                path.Fill = (Brush)FindResource("PauseIconBrush");
                path.ToolTip = (string)FindResource("PauseStr");
                StopButton.Visibility = Visibility.Visible;
                TimeAxis.IsHitTestVisible = false;
                if (config.CollapseLayerAxis) LayerAxisDefinition.Height = new GridLength(30);
                ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Hidden);
                SetPanelEnable(false);
                StartPlayTimer();
            }
            else
            {
                if (player.Pause && !TrySkipPlayerFrame(selectedFrame, true)) return;
                player.Pause = !player.Pause;
                if (!player.Pause)
                {
                    path.Data = (Geometry)FindResource("Pause_Icon");
                    path.Fill = (Brush)FindResource("PauseIconBrush");
                    path.ToolTip = (string)FindResource("PauseStr");
                    TimeAxis.IsHitTestVisible = false;
                    if (config.CollapseLayerAxis) LayerAxisDefinition.Height = new GridLength(30);
                    ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Hidden);
                    var screen = ParticleTabControl.SelectedItem as TabItem;
                    if (screen != null)
                    {
                        var content = screen.Content as Canvas;
                        var screenContent = VisualHelper.VisualDownwardSearch(content, "ScreenContent") as Canvas;
                        (VisualHelper.VisualDownwardSearch(screenContent, "Grid") as Canvas).Visibility = Visibility.Hidden;
                        (VisualHelper.VisualDownwardSearch(screenContent, "ComponentLayer") as Canvas).Visibility = Visibility.Hidden;
                    }
                    Panel.SetZIndex(player, 1);
                    StartPlayTimer();
                }
            }
        }
        private void StopItem_Click(object sender, RoutedEventArgs e)
        {
            if (player == null) return;
            var screen = ParticleTabControl.SelectedItem as TabItem;
            if (screen != null)
            {
                var content = screen.Content as Canvas;
                var screenContent = VisualHelper.VisualDownwardSearch(content, "ScreenContent") as Canvas;
                screenContent.Children.Remove(player);
                screenContent.Background.Opacity = 1;
                (VisualHelper.VisualDownwardSearch(screenContent, "Grid") as Canvas).Visibility = Visibility.Visible;
                (VisualHelper.VisualDownwardSearch(screenContent, "Center") as Image).Visibility = Visibility.Visible;
                (VisualHelper.VisualDownwardSearch(screenContent, "ComponentLayer") as Canvas).Visibility = Visibility.Visible;
            }
            player.Dispose();
            player = null;
            if (activeParticleCountLabel != null)
            {
                activeParticleCountLabel.Visibility = Visibility.Hidden;
                statusTipLabel.Visibility = Visibility.Hidden;
                statusPanel.Visibility = Visibility.Hidden;
            }
            StopButton.Visibility = Visibility.Collapsed;
            var path = VisualHelper.VisualDownwardSearch<Path>(PlayButton) as Path;
            path.Data = (Geometry)FindResource("Play_Icon");
            path.Fill = (Brush)FindResource("PlayIconBrush");
            path.ToolTip = (string)FindResource("PlayStr");
            TimeAxis.IsHitTestVisible = true;
            if (config.CollapseLayerAxis) LayerAxisDefinition.Height = new GridLength(94);
            ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Auto);
            SetPanelEnable(true);
            StopPlayTimer();
        }
        private void PlaySettingItem_Click(object sender, RoutedEventArgs e)
        {
            OpenPlaySetting();
        }
        private void JumpToFrame_Click(object sender, RoutedEventArgs e)
        {
            OpenJumpToFrame();
        }
        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (player != null)
            {
                var status = int.Parse((string)(sender as Button).Content);
                player.PlayerImpl.SetStatus(status);
                foreach (Button button in statusPanel.Children)
                {
                    if ((string)button.Content == status.ToString())
                    {
                        button.Background = new SolidColorBrush(Colors.White);
                        button.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(Colors.Transparent);
                        button.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }
        #endregion
    }
}
