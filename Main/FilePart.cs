/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CrazyStorm.Core;
using System.Xml;
using System.ComponentModel;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        bool saved;
        string filePath;
        string fileName;
        #endregion
        
        #region Private Methods
        bool SaveTip()
        {
            if (!saved)
            {
                switch (MessageBox.Show((string)FindResource("SaveTipStr"), (string)FindResource("TipTitleStr"), 
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }
        void New()
        {
            if (!SaveTip())
                return;

            file = new File(true);
            fileName = "Untitled";
            filePath = string.Empty;
            InitializeSystem();
            saved = true;
        }
        void Open()
        {
            if (!SaveTip())
                return;

            using (var open = new System.Windows.Forms.OpenFileDialog())
            {
                open.InitialDirectory = File.CurrentDirectory;
                open.Filter = (string)FindResource("ProjectFileExtensionStr");
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Open(open.FileName);
            }
        }
        bool Open(string openPath)
        {
            try
            {
                if (!File.CheckVersion(openPath) &&
                    MessageBox.Show((string)FindResource("DifferentVersionStr"), (string)FindResource("TipTitleStr"),
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return false;
                }
                file = new File(false);
                file.Load(openPath);
                filePath = openPath;
                fileName = System.IO.Path.GetFileNameWithoutExtension(openPath);
                File.CurrentDirectory = System.IO.Path.GetDirectoryName(openPath) + '\\';
                InitializeSystem();
                saved = true;
                return true;
            }
            catch (XmlException)
            {
                MessageBox.Show((string)FindResource("FileTypeErrorStr"), (string)FindResource("ErrorTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.IO.FileLoadException ex)
            {
                MessageBox.Show((string)FindResource(ex.Message + "Str"), (string)FindResource("ErrorTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
        void Save()
        {
            if (string.IsNullOrWhiteSpace(filePath))
                SaveTo();
            else
                Save(filePath);
        }
        void Save(string savedPath)
        {
            filePath = savedPath;
            fileName = System.IO.Path.GetFileNameWithoutExtension(savedPath);
            File.CurrentDirectory = System.IO.Path.GetDirectoryName(savedPath) + '\\';
            file.UpdateResource();
            file.Save(savedPath);
            InitializeFile();
            saved = true;
        }
        void SaveTo()
        {
            using (var save = new System.Windows.Forms.SaveFileDialog())
            {
                save.InitialDirectory = File.CurrentDirectory;
                save.Filter = (string)FindResource("ProjectFileExtensionStr");
                if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Save(save.FileName);
            }
        }
        #endregion

        #region Window EventHandlers
        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            New();
        }
        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }
        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        private void SaveToItem_Click(object sender, RoutedEventArgs e)
        {
            SaveTo();
        }
        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }   
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !SaveTip();
        }
        #endregion
    }
}
