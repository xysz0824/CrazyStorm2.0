/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CrazyStorm.Core;

namespace CrazyStorm
{
    partial class Main
    {
        private const string ProjectRepositoryUrl = "https://github.com/xysz0824/CrazyStorm2.0";
        private const string TutorialDirectoryRelativePath = @"Docs";
        private const string TutorialRootRelativePath = @"Docs\index.html";
        [DllImport("kernel32.dll")]
        private static extern ushort GetUserDefaultUILanguage();

        #region Private Methods
        private void OpenTutorial()
        {
            try
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var candidates = GetTutorialCandidates(baseDirectory);
                foreach (var tutorialPath in candidates)
                {
                    if (!System.IO.File.Exists(tutorialPath)) continue;
                    Process.Start(new ProcessStartInfo(tutorialPath)
                    {
                        UseShellExecute = true
                    });
                    return;
                }

                var message = string.Format((string)FindResource("TutorialNotFoundStr"),
                    string.Join(Environment.NewLine, candidates));
                MessageBox.Show(message, (string)FindResource("ErrorTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                var message = string.Format((string)FindResource("TutorialOpenFailedStr"), ex.Message);
                MessageBox.Show(message, (string)FindResource("ErrorTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private List<string> GetTutorialCandidates(string baseDirectory)
        {
            var candidates = new List<string>();
            var docsDirectory = Path.Combine(baseDirectory, TutorialDirectoryRelativePath);
            var languageIndexes = GetLanguageIndexFiles(docsDirectory);
            var systemCulture = GetSystemCulture();
            var preferred = FindPreferredLanguageIndex(languageIndexes, systemCulture);
            AddTutorialCandidate(candidates, preferred);
            foreach (var indexFile in languageIndexes)
            {
                AddTutorialCandidate(candidates, indexFile);
            }
            AddTutorialCandidate(candidates, Path.Combine(baseDirectory, TutorialRootRelativePath));
            return candidates;
        }
        private CultureInfo GetSystemCulture()
        {
            var languageId = GetUserDefaultUILanguage();
            if (languageId == 0) return CultureInfo.CurrentUICulture;
            return CultureInfo.GetCultureInfo(languageId);
        }
        private List<string> GetLanguageIndexFiles(string docsDirectory)
        {
            if (!Directory.Exists(docsDirectory)) return new List<string>();
            var indexes = new List<string>();
            foreach (var directory in Directory.GetDirectories(docsDirectory))
            {
                var indexFile = Path.Combine(directory, "index.html");
                if (!System.IO.File.Exists(indexFile)) continue;
                indexes.Add(indexFile);
            }

            indexes.Sort(StringComparer.OrdinalIgnoreCase);
            return indexes;
        }
        private string FindPreferredLanguageIndex(List<string> indexFiles, CultureInfo culture)
        {
            if (culture == null || indexFiles == null || indexFiles.Count == 0) return null;
            var exactName = culture.Name;
            var twoLetterName = culture.TwoLetterISOLanguageName;
            string startsWith = null;
            if (!string.IsNullOrEmpty(twoLetterName)) startsWith = twoLetterName + "-";

            foreach (var file in indexFiles)
            {
                var directoryName = Path.GetFileName(Path.GetDirectoryName(file));
                if (string.IsNullOrEmpty(directoryName)) continue;
                if (string.Equals(directoryName, exactName, StringComparison.OrdinalIgnoreCase))
                    return file;
            }

            foreach (var file in indexFiles)
            {
                var directoryName = Path.GetFileName(Path.GetDirectoryName(file));
                if (string.IsNullOrEmpty(directoryName)) continue;
                if (string.Equals(directoryName, twoLetterName, StringComparison.OrdinalIgnoreCase))
                    return file;
                if (startsWith != null && directoryName.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                    return file;
            }

            return null;
        }
        private void AddTutorialCandidate(List<string> candidates, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (candidates.Contains(path, StringComparer.OrdinalIgnoreCase)) return;
            candidates.Add(path);
        }
        #endregion

        #region Window EventHandlers
        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToolBar; 
            var overflowGrid = tb.Template.FindName("OverflowGrid", tb) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Hidden;
            }   
        }
        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(ProjectRepositoryUrl)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, (string)FindResource("ErrorTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TutorialItem_Click(object sender, RoutedEventArgs e)
        {
            OpenTutorial();
        }
        private void OpenTutorialCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void OpenTutorialCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenTutorial();
        }
        #endregion
    }
}
