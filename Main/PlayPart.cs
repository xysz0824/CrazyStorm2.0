using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CrazyStorm.Core;
using System.IO;

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
            GeneratePlayFile(filePath);
        }
        void GeneratePlayFile(string genPath)
        {
            genPath = Path.GetDirectoryName(genPath) + "\\" + fileName + ".bg";
            using (FileStream stream = new FileStream(genPath, FileMode.Create))
            {
                var writer = new BinaryWriter(stream);
                //Play file use UTF-8 encoding
                //Write file header
                writer.Write(PlayDataHelper.GetStringBytes("BG"));
                //Write file version
                writer.Write(PlayDataHelper.GetStringBytes(VersionInfo.Version));
                //Write file data
                writer.Write(file.GeneratePlayData().ToArray());
            }
            MessageBox.Show((string)FindResource("PlayFileSavedStr"), (string)FindResource("TipTitleStr"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Window EventHandlers
        private void GeneratePlayFile_Click(object sender, RoutedEventArgs e)
        {
            GeneratePlayFile();
        }
        #endregion
    }
}
