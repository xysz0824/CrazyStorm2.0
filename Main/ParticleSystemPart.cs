/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
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
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Methods
        void CreateNewParticleSystem()
        {
            var particleSystem = new ParticleSystem("ParticleSystem" + file.ParticleIndex);
            file.ParticleSystems.Add(particleSystem);
            selectedSystem = particleSystem;
            InitializeCommandStack(particleSystem);
            AddNewParticleTab(particleSystem);
        }
        void AddNewParticleTab(ParticleSystem particle)
        {
            var tabItem = new TabItem();
            tabItem.Header = particle.Name;
            tabItem.Tag = particle;
            tabItem.Content = ParticleTabControl.ItemTemplate.LoadContent() as Canvas;
            ParticleTabControl.Items.Add(tabItem);
            ParticleTabControl.SelectedItem = tabItem;
        }
        void DeleteAllParticle()
        {
            for (int i = 0; i < ParticleTabControl.Items.Count; ++i)
            {
                var item = ParticleTabControl.Items[i] as TabItem;
                var particle = item.Tag as ParticleSystem;
                commandStacks.Remove(particle);
                file.ParticleSystems.Remove(particle);
                ParticleTabControl.Items.Remove(item);
                i--;
            }
        }
        void DeleteSeletedParticleSystem()
        {
            if (file.ParticleSystems.Count > 1)
            {
                TabItem selected = null;
                foreach (TabItem item in ParticleTabControl.Items)
                {
                    if (item.Tag == selectedSystem)
                    {
                        selected = item;
                        break;
                    }
                }
                if (MessageBox.Show((string)FindResource("ConfirmDeleteParticleSystemStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    commandStacks.Remove(selectedSystem);
                    file.ParticleSystems.Remove(selectedSystem);
                    ParticleTabControl.Items.Remove(selected);
                }
            }
            else
                MessageBox.Show((string)FindResource("CanNotDeleteAllParticleSystemStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySeletedParticleSystem()
        {
            var particle = selectedSystem.Clone() as ParticleSystem;
            var components = new List<Component>();
            foreach (var layer in particle.Layers)
                components.AddRange(layer.Components);

            foreach (var component in components)
                component.RebuildReferenceFromCollection(components);

            RebuildComponentTree(particle);
            file.ParticleSystems.Add(particle);
            selectedSystem = particle;
            InitializeCommandStack(particle);
            AddNewParticleTab(particle);
        }
        void ReloadTypes()
        {
            var particleTypes = new List<ParticleType>();
            particleTypes.AddRange(defaultParticleTypes);
            particleTypes.AddRange(selectedSystem.CustomTypes);
            for (int i = 2; i < LeftTabControl.Items.Count; ++i)
            {
                var panel = ((LeftTabControl.Items[i] as TabItem).Content as ScrollViewer).Content as PropertyPanel;
                panel.LoadTypes(particleTypes);
            }
        }
        void OpenSelectedParticleSystemSetting()
        {
            ParticleSystemSetting window = new ParticleSystemSetting(file, selectedSystem, ParticleTabControl.SelectedItem as TabItem);
            window.ShowDialog();
            window.Close();
            ReloadTypes();
        }
        #endregion

        #region Window EventHandlers
        private void ParticleSystemMenu_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to the corresponding function of ParticleSystem menu.
            var item = e.Source as MenuItem;
            switch (item.Name)
            {
                case "AddParticleSystemItem":
                    CreateNewParticleSystem();
                    break;
                case "DeleteParticleSystemItem":
                    DeleteSeletedParticleSystem();
                    break;
                case "CopyParticleSystemItem":
                    CopySeletedParticleSystem();
                    break;
                case "SetParticleSystemItem":
                    OpenSelectedParticleSystemSetting();
                    break;
            }
            saved = false;
        }
        #endregion
    }
}
