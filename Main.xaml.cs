/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
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
        List<ParticleType> defaultParticleTypes;
        Dictionary<ParticleSystem, CommandStack> commandStacks;
        List<Core.Component> clipBoard;
        #endregion

        #region Constructor
        public Main()
        {
            defaultParticleTypes = new List<ParticleType>();
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
        void LoadDefaultParticleTypes()
        {
            StreamResourceInfo info = Application.GetResourceStream(new Uri("set.txt", UriKind.Relative));
            using (System.IO.StreamReader reader = new System.IO.StreamReader(info.Stream))
            {
                ParticleType.LoadDefaultTypes(reader, defaultParticleTypes);
            }
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
            SoundList.ItemsSource = file.Sounds;
            VariableGrid.ItemsSource = file.Globals;
            DeleteVariable.IsEnabled = file.Globals.Count > 0 ? true : false;
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
        }
        void InitializeLayerAndComponent()
        {
            selectedLayer = selectedSystem.Layers.First();
            LayerTree.ItemsSource = selectedSystem.Layers;            
            LayerAxis.ItemsSource = selectedSystem.Layers;
            ComponentTree.ItemsSource = selectedSystem.ComponentTree;
            BindComponentItem.IsEnabled = false;
            UnbindComponentItem.IsEnabled = false;
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
            LoadDefaultParticleTypes();
            InitializeConfig();
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
            item.IsSelected = true;
        }
        #endregion
    }
}
