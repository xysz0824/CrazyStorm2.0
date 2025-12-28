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
        DispatcherTimer playTimer;
        #endregion
        #region Private Methods
        void GeneratePlayFile()
        {
            if (string.IsNullOrWhiteSpace(File.CurrentDirectory))
            {
                MessageBox.Show((string)FindResource("NeedSaveFirstStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            file.GeneratePlayFile(filePath, fileName);
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
                if (activeParticleCountLabel == null)
                {
                    activeParticleCountLabel = VisualHelper.VisualDownwardSearch(content, "ActiveParticleCount") as Label;
                }
                activeParticleCountLabel.Visibility = Visibility.Visible;
                player = new EmbeddedPlayer();
                var config = screen.DataContext as Config;
                player.Width = config.ScreenWidth;
                player.Height = config.ScreenHeight;
                player.PlayerImpl = new PlayerImpl(config.ScreenWidth, config.ScreenHeight,
                    config.ParticleMaximum, config.CurveParticleMaximum);
                if (string.IsNullOrWhiteSpace(File.CurrentDirectory))
                {
                    player.PlayerImpl.ResourceDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                }
                else
                {
                    player.PlayerImpl.ResourceDirectory = File.CurrentDirectory;
                }
                player.PlayerImpl.File = new File();
                player.PlayerImpl.File.LoadPlayFile(file.GeneratePlayFile(), CrazyStorm_Player.VersionInfo.BaseVersion);
                player.PlayerImpl.BackgroundPath = config.BackgroundPath;
                player.PlayerImpl.SelectedParticleSystemIndex = particleSystemIndex;
                player.PlayerImpl.CustomCenter = new Microsoft.Xna.Framework.Vector2(0, 0);
                player.PlayerImpl.ControllableImagePath = config.SelfImagePath;
                player.PlayerImpl.ControllableSetting = config.SelfSetting;
                player.PlayerImpl.CurrentFrame = selectedFrame - 1;
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
        }
        void StartPlayTimer()
        {
            StartLayerTimer();
            if (playTimer == null)
            {
                playTimer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher);
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
        void OpenJumpToFrame()
        {
            var window = new JumpToFrame(selectedFrame, selectedSystem.TotalFrame);
            window.ShowDialog();
            if (window.Confirmed) JumpToFrame(window.TargetFrame);
        }
        #endregion

        #region Window EventHandlers
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (activeParticleCountLabel != null) activeParticleCountLabel.Content = ParticleManager.ActiveParticleCount;
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
                ScrollViewer.SetHorizontalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Hidden);
                ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Hidden);
                SetPanelEnable(false);
                StartPlayTimer();
            }
            else
            {
                player.Pause = !player.Pause;
                if (player.Pause)
                {
                    path.Data = (Geometry)FindResource("Play_Icon");
                    path.Fill = (Brush)FindResource("PlayIconBrush");
                    path.ToolTip = (string)FindResource("PlayStr");
                    selectedFrame = player.PlayerImpl.CurrentFrame + 1;
                    TimeAxis.IsHitTestVisible = true;
                    ScrollViewer.SetHorizontalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Auto);
                    ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Auto);
                    Panel.SetZIndex(player, -1);
                    PausePlayTimer();
                }
                else
                {
                    path.Data = (Geometry)FindResource("Pause_Icon");
                    path.Fill = (Brush)FindResource("PauseIconBrush");
                    path.ToolTip = (string)FindResource("PauseStr");
                    player.PlayerImpl.CurrentFrame = selectedFrame - 1;
                    TimeAxis.IsHitTestVisible = false;
                    ScrollViewer.SetHorizontalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Hidden);
                    ScrollViewer.SetVerticalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Hidden);
                    Panel.SetZIndex(player, 1);
                    StartPlayTimer();
                }
            }
        }
        private void StopItem_Click(object sender, RoutedEventArgs e)
        {
            if (player == null) return;
            player.Dispose();
            player = null;
            if (activeParticleCountLabel != null) activeParticleCountLabel.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Collapsed;
            var path = VisualHelper.VisualDownwardSearch<Path>(PlayButton) as Path;
            path.Data = (Geometry)FindResource("Play_Icon");
            path.Fill = (Brush)FindResource("PlayIconBrush");
            path.ToolTip = (string)FindResource("PlayStr");
            TimeAxis.IsHitTestVisible = true;
            ScrollViewer.SetHorizontalScrollBarVisibility(LayerAxis, ScrollBarVisibility.Auto);
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
        #endregion
    }
}
