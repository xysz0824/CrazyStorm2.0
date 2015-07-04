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
using System.ComponentModel;
using CrazyStorm.Core;

namespace CrazyStorm
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        Config config;
        File file;
        Dictionary<ParticleSystem, CommandStack> commandStacks;
        ClipBoard clipBoard;
        string status = String.Empty;
        #endregion

        #region Public Members
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }
        #endregion

        #region Constructor
        public Main()
        {
            config = new Config();
            file = new File("Untitled");
            commandStacks = new Dictionary<ParticleSystem, CommandStack>();
            clipBoard = new ClipBoard();
            InitializeComponent();
            InitializeSystem();
        }
        #endregion

        #region Private Methods
        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        static DependencyObject VisualDownwardSearch(DependencyObject source, string name)
        {
            if (source == null)
                return null;

            var count = VisualTreeHelper.GetChildrenCount(source);
            if (count == 0)
                return null;

            for (int i = 0; i < count; ++i)
            {
                var child = VisualTreeHelper.GetChild(source, i);
                if ((string)child.GetValue(NameProperty) == name)
                    return child;
                else
                {
                    child = VisualDownwardSearch(child, name);
                    if (child != null)
                        return child;
                }
            }
            return null;
        }
        void InitializeSystem()
        {
            LoadConfig();
            InitializeFile();
            InitializeParticle();
            InitializeLayer();
            InitializeEdit();
            InitializeStatus();
            UpdateComponent();
            status = (string)FindResource("Ready");
        }
        void LoadConfig()
        {
            //TODO : Load config.
        }
        void InitializeFile()
        {
            Title = AppInfo.AppTitle + " - " + file.FileName;
            ImageList.ItemsSource = file.Images;
            SoundList.ItemsSource = file.Sounds;
            ScriptList.ItemsSource = file.Scripts;
        }
        void InitializeParticle()
        {
            selectedParticle = file.Particles.First();
            ParticleTabControl.DataContext = config;
            InitializeCommandStack();
            AddNewParticleTab(selectedParticle);
            ComponentList.ItemsSource = selectedParticle.Components;
            DeleteComponentItem.IsEnabled = false;
            BindComponentItem.IsEnabled = false;
            UnbindComponentItem.IsEnabled = false;
        }
        void InitializeLayer()
        {
            selectedLayer = selectedParticle.Layers.First();
            LayerTree.ItemsSource = selectedParticle.Layers;
            LayerAxis.ItemsSource = selectedParticle.Layers;
            CopyLayerItem.IsEnabled = false;
            DeleteLayerItem.IsEnabled = false;
            SetLayerItem.IsEnabled = false;
        }
        void InitializeCommandStack()
        {
            commandStacks[selectedParticle] = new CommandStack();
            commandStacks[selectedParticle].StackChanged += () =>
            {
                UpdateEdit();
            };
        }
        void InitializeEdit()
        {
            CutItem.IsEnabled = false;
            CopyItem.IsEnabled = false;
            PasteItem.IsEnabled = false;
            UndoItem.IsEnabled = false;
            RedoItem.IsEnabled = false;
            CutButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            PasteButton.IsEnabled = false;
            UndoButton.IsEnabled = false;
            RedoButton.IsEnabled = false;
        }
        void InitializeStatus()
        {
            StatusText.DataContext = this;
        }
        void UpdateParticle()
        {
            ComponentList.ItemsSource = selectedParticle.Components;
            DeleteComponentItem.IsEnabled = false;
            BindComponentItem.IsEnabled = false;
            UnbindComponentItem.IsEnabled = false;
        }
        void UpdateLayer()
        {
            InitializeLayer();
        }
        void UpdateComponent()
        {
            UpdatePropertyPanel();
            UpdateScreen();
            UpdateSelectedGroup();
            UpdateEdit();
        }
        #endregion
    }
}
