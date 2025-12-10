/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using CrazyStorm.Core;
using CrazyStorm_Player;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        EmbeddedPlayer player;
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
                player.PlayerImpl.File = new File(false);
                player.PlayerImpl.File.LoadPlayFile(file.GeneratePlayFile(), CrazyStorm_Player.VersionInfo.BaseVersion);
                player.PlayerImpl.BackgroundPath = config.BackgroundPath;
                player.PlayerImpl.SelectedParticleSystemIndex = particleSystemIndex;
                player.PlayerImpl.CustomCenter = new Microsoft.Xna.Framework.Vector2(config.CenterX, config.CenterY);
                player.PlayerImpl.ControllableImagePath = config.SelfImagePath;
                player.PlayerImpl.ControllableSetting = config.SelfSetting;
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
        #endregion

        #region Window EventHandlers
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
            }
            else
            {
                player.Pause = !player.Pause;
                if (player.Pause)
                {
                    path.Data = (Geometry)FindResource("Play_Icon");
                    path.Fill = (Brush)FindResource("PlayIconBrush");
                    path.ToolTip = (string)FindResource("PlayStr");
                    Panel.SetZIndex(player, -1);
                }
                else
                {
                    path.Data = (Geometry)FindResource("Pause_Icon");
                    path.Fill = (Brush)FindResource("PauseIconBrush");
                    path.ToolTip = (string)FindResource("PauseStr");
                    Panel.SetZIndex(player, 1);
                }
            }
        }
        private void StopItem_Click(object sender, RoutedEventArgs e)
        {
            if (player == null) return;
            player.Dispose();
            player = null;
            StopButton.Visibility = Visibility.Collapsed;
            var path = VisualHelper.VisualDownwardSearch<Path>(PlayButton) as Path;
            path.Data = (Geometry)FindResource("Play_Icon");
            path.Fill = (Brush)FindResource("PlayIconBrush");
            path.ToolTip = (string)FindResource("PlayStr");
        }
        private void PlaySettingItem_Click(object sender, RoutedEventArgs e)
        {
            OpenPlaySetting();
        }
        #endregion
    }
}
