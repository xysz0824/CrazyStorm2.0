/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
using System.ComponentModel;
using CrazyStorm.Core;
using System.Windows.Resources;
using System.Threading.Tasks;
using System.Threading;

namespace CrazyStorm
{
    public partial class Main : Window
    {
        #region Private Members
        Config config;
        File file;
        Dictionary<ParticleSystem, CommandStack> commandStacks;
        List<Core.Component> clipBoard;
        #endregion

        #region Constructor
        public Main()
        {
            commandStacks = new Dictionary<ParticleSystem, CommandStack>();
            clipBoard = new List<Core.Component>();
            InitializeComponent();
        }
        #endregion

        #region Private Methods
        void InitializeConfig()
        {
            config = new Config("Config.ini");
            ParticleTabControl.DataContext = config;
        }
        void InitializeSystem()
        {
            InitializeFile();
            InitializeParticle();
            InitializeEdit();
            InitializeScreen();
        }
        void InitializeFile()
        {
            Title = VersionInfo.AppTitle + " - " + fileName;
            ImageList.ItemsSource = file.Images;
            DeleteImageButton.IsEnabled = file.Images.Count > 0;
            SoundList.ItemsSource = file.Sounds;
            DeleteSoundButton.IsEnabled = file.Sounds.Count > 0;
            VariableGrid.ItemsSource = file.Globals;
            DeleteVariable.IsEnabled = file.Globals.Count > 0;
        }
        void InitializeParticle()
        {
            DeleteAllParticle();
            selectedSystem = file.ParticleSystems.First();
            foreach (var item in file.ParticleSystems)
            {
                InitializeCommandStack(item);
                AddNewParticleTab(item);
            }
        }
        void InitializeCommandStack(ParticleSystem particle)
        {
            commandStacks[particle] = new CommandStack();
            commandStacks[particle].StackChanged += () =>
            {
                UpdateCommandStackStatus();
                saved = false;
            };
        }
        void InitializeEdit()
        {
            CutItem.IsEnabled = false;
            CopyItem.IsEnabled = false;
            PasteItem.IsEnabled = false;
            UndoItem.IsEnabled = false;
            RedoItem.IsEnabled = false;
            DelItem.IsEnabled = false;
            CutButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            PasteButton.IsEnabled = false;
            UndoButton.IsEnabled = false;
            RedoButton.IsEnabled = false;
            BindButton.IsEnabled = false;
            UnbindButton.IsEnabled = false;
        }
        void InitializeLayerAndComponent()
        {
            selectedLayer = selectedSystem.Layers.First();
            LayerTree.ItemsSource = selectedSystem.Layers;            
            LayerAxis.ItemsSource = selectedSystem.Layers;
            ComponentTree.ItemsSource = selectedSystem.ComponentTree;
            BindComponentItem.IsEnabled = false;
            UnbindComponentItem.IsEnabled = false;
            BindButton.IsEnabled = false;
            UnbindButton.IsEnabled = false;
            LeftTabControl.SelectedIndex = 0;
        }
        void InitializeScreen()
        {
            aimRect = null;
            aimComponent = null;
        }
        void UpdateSelectedStatus()
        {
            //Be careful that UpdateScreen() needs to update first,
            //because it will refresh selectedComponents set.
            UpdateScreen();
            UpdateComponentPanels();
            UpdateSelectedGroup();
            UpdateComponentMenu();
            UpdateEditStatus();
        }
        #endregion

        #region Public Methods
        public void Initailize()
        {
            InitializeConfig();
            ParticleType.LoadDefaultTypes(config.TypeLibraryPath);
            ChangeTheme(config.Theme);
        }
        public void StartNewFile()
        {
            saved = true;
            New();
        }
        public void OpenFile(string openPath)
        {
            Open(openPath);
        }
        #endregion

        #region Window EventHandlers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Highlight selected layer.
            TreeViewItem item = (TreeViewItem)LayerTree.ItemContainerGenerator.ContainerFromItem(selectedLayer);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }
        #endregion
    }
}
