/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
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
        void CreateNewParticle()
        {
            var particleSystem = new ParticleSystem("Particle" + file.ParticleIndex);
            file.ParticleSystems.Add(particleSystem);
            selectedParticle = particleSystem;
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
        void DeleteSeletedParticle()
        {
            if (file.ParticleSystems.Count > 1)
            {
                TabItem selected = null;
                foreach (TabItem item in ParticleTabControl.Items)
                {
                    if (item.Tag == selectedParticle)
                    {
                        selected = item;
                        break;
                    }
                }
                if (MessageBox.Show((string)FindResource("ConfirmDeleteParticleStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    commandStacks.Remove(selectedParticle);
                    file.ParticleSystems.Remove(selectedParticle);
                    ParticleTabControl.Items.Remove(selected);
                }
            }
            else
                MessageBox.Show((string)FindResource("CanNotDeleteAllParticleStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySeletedParticle()
        {
            var particle = selectedParticle.Clone() as ParticleSystem;
            var components = new List<Component>();
            foreach (var layer in particle.Layers)
                components.AddRange(layer.Components);

            foreach (var component in components)
                component.RebuildReferenceFromCollection(components);

            RebuildComponentTree(particle);
            file.ParticleSystems.Add(particle);
            selectedParticle = particle;
            InitializeCommandStack(particle);
            AddNewParticleTab(particle);
        }
        void OpenSelectedParticleSetting()
        {
            ParticleSetting window = new ParticleSetting(file, selectedParticle, ParticleTabControl.SelectedItem as TabItem);
            window.ShowDialog();
            window.Close();
        }
        #endregion

        #region Window EventHandlers
        private void ParticleMenu_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to the corresponding function of Particle menu.
            var item = e.Source as MenuItem;
            switch (item.Name)
            {
                case "AddParticleItem":
                    CreateNewParticle();
                    break;
                case "DeleteParticleItem":
                    DeleteSeletedParticle();
                    break;
                case "CopyParticleItem":
                    CopySeletedParticle();
                    break;
                case "SetParticleItem":
                    OpenSelectedParticleSetting();
                    break;
            }
            saved = false;
        }
        #endregion
    }
}
