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
            var particle = new ParticleSystem("Particle" + file.ParticleIndex);
            file.Particles.Add(particle);
            selectedParticle = particle;
            InitializeCommandStacks();
            AddNewParticleTab(particle);
        }
        void AddNewParticleTab(ParticleSystem particle)
        {
            var tabItem = new TabItem();
            tabItem.Header = particle.Name;
            tabItem.Content = ParticleTabControl.ItemTemplate.LoadContent() as Canvas;
            ParticleTabControl.Items.Add(tabItem);
            ParticleTabControl.SelectedItem = tabItem;
        }
        void DeleteSeletedParticle()
        {
            if (file.Particles.Count > 1)
            {
                TabItem selected = null;
                foreach (TabItem item in ParticleTabControl.Items)
                    if (selectedParticle.Name == (string)item.Header)
                    {
                        selected = item;
                        break;
                    }
                if (MessageBox.Show((string)FindResource("ConfirmDeleteParticle"), (string)FindResource("TipTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    commandStacks.Remove(selectedParticle);
                    file.Particles.Remove(selectedParticle);
                    ParticleTabControl.Items.Remove(selected);
                }
            }
            else
                MessageBox.Show((string)FindResource("CanNotDeleteAllParticle"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CopySeletedParticle()
        {
            //TODO : Copy function.
        }
        void OpenSelectedParticleSetting()
        {
            ParticleSetting window = new ParticleSetting(file, selectedParticle, ParticleTabControl.SelectedItem as TabItem);
            window.ShowDialog();
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
        }
        #endregion
    }
}
