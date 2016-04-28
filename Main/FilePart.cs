using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CrazyStorm.Core;
using System.Xml;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Members
        bool saved;
        string filePath;
        string fileName = "Untitled";
        #endregion
        
        #region Private Methods
        bool SaveTip()
        {
            if (!saved)
            {
                switch (MessageBox.Show((string)FindResource("SaveTip"), (string)FindResource("TipTitle"), 
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

            file = new File();
            fileName = "Untitled";
            InitializeSystem();
        }
        void Open()
        {
            if (!SaveTip())
                return;

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
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);
            var root = doc.CreateElement(VersionInfo.AppName.Replace(" ", ""));
            file.StoreAsXml(doc, root);
            doc.AppendChild(root);
            doc.Save(savedPath);
        }
        void SaveTo()
        {
            using (var save = new System.Windows.Forms.SaveFileDialog())
            {
                save.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                save.Filter = (string)FindResource("FileExtension");
                if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = save.FileName;
                    fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    Save(filePath);
                    InitializeFile();
                }
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
        #endregion
    }
}
