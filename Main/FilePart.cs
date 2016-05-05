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

            file = new File();
            fileName = "Untitled";
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
                var doc = new XmlDocument();
                doc.Load(openPath);
                XmlElement root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
                if (root == null)
                    throw new XmlException();
                else
                {
                    if (!root.HasAttribute("version"))
                        throw new System.IO.FileLoadException("FileDataError");

                    string version = root.GetAttribute("version");
                    if (VersionInfo.Version != version &&
                        MessageBox.Show((string)FindResource("DifferentVersionStr"), (string)FindResource("TipTitleStr"),
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                        return false;
                    else
                    {
                        file = new File();
                        file.BuildFromXml(root);
                        RebuildObjectReference();
                        RebuildComponentTree();
                        filePath = openPath;
                        fileName = System.IO.Path.GetFileNameWithoutExtension(openPath);
                        File.CurrentDirectory = System.IO.Path.GetDirectoryName(openPath) + '\\';
                        InitializeSystem();
                        saved = true;
                        return true;
                    }
                }
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
        void RebuildComponentTree(ParticleSystem particleSystem)
        {
            for (int i = 0; i < particleSystem.Layers.Count; ++i)
            {
                particleSystem.AddLayer(particleSystem.Layers[0]);
                particleSystem.Layers.RemoveAt(0);
            }   
        }
        void RebuildComponentTree()
        {
            foreach (var particleSystem in file.ParticleSystems)
                RebuildComponentTree(particleSystem);
        }
        void RebuildObjectReference()
        {
            foreach (var particleSystem in file.ParticleSystems)
            {
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                    customType.RebuildReferenceFromCollection(file.Images);
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                particleTypes.AddRange(defaultParticleTypes);
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Collect all components
                var components = new List<Core.Component>();
                foreach (var layer in particleSystem.Layers)
                    components.AddRange(layer.Components);
                //Rebuild components reference
                foreach (var component in components)
                {
                    component.RebuildReferenceFromCollection(components);
                    //Rebuild particles reference
                    if (component is Emitter)
                        (component as Emitter).Particle.RebuildReferenceFromCollection(particleTypes);
                }
            }
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
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);
            var root = doc.CreateElement(VersionInfo.AppName.Replace(" ", ""));
            var version = doc.CreateAttribute("version");
            version.Value = VersionInfo.Version;
            root.Attributes.Append(version);
            file.StoreAsXml(doc, root);
            doc.AppendChild(root);
            doc.Save(savedPath);
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
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !SaveTip();
        }
        #endregion
    }
}
