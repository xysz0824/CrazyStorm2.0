/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Methods
        void GeneratePlayFile()
        {
            if (string.IsNullOrWhiteSpace(Core.File.CurrentDirectory))
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
            if (!System.IO.File.Exists(config.PlayerPath))
            {
                MessageBox.Show((string)FindResource("PlayerNotFoundStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string genPath = Path.GetDirectoryName(config.PlayerPath);
            if (string.IsNullOrEmpty(genPath)) genPath = Environment.CurrentDirectory;

            genPath += "\\Temp\\Temp.bg";
            if (!System.IO.Directory.Exists(Path.GetDirectoryName(genPath)))
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(genPath));
            }
            file.GeneratePlayFile(genPath, "Temp");
            int particleSystemIndex = 0;
            for (int i = 0; i < file.ParticleSystems.Count; ++i)
            {
                if (selectedSystem == file.ParticleSystems[i])
                {
                    particleSystemIndex = i;
                    break;
                }
            }
            ProcessStartInfo ps = new ProcessStartInfo(config.PlayerPath);
            ps.Arguments = "\"" + genPath + "\" \"" + config.BackgroundPath + "\" " + particleSystemIndex + " ";
            ps.Arguments += config.ScreenWidth + " " + config.ScreenHeight + " ";
            ps.Arguments += config.ParticleMaximum + " " + config.CurveParticleMaximum + " ";
            ps.Arguments += config.Windowed + " ";
            ps.Arguments += config.CenterX + " " + config.CenterY + " ";
            ps.Arguments += "\"" + config.SelfImagePath + "\" \"" + config.SelfSetting + "\"";
            ps.WindowStyle = ProcessWindowStyle.Normal;
            Process p = new Process();
            p.StartInfo = ps;
            p.Start();
            p.WaitForInputIdle();
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
            file.UpdateResource();
            PlayCurrent();
        }
        private void PlaySettingItem_Click(object sender, RoutedEventArgs e)
        {
            OpenPlaySetting();
        }
        #endregion
    }
}
