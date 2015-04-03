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
using CrazyStorm.CoreLibrary;

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
        Dictionary<Barrage, CommandStack> commandStacks;
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
            file = new File("Untitled");
            commandStacks = new Dictionary<Barrage, CommandStack>();
            InitializeComponent();
            InitializeSystem();
            status = (string)FindResource("Ready");
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
            InitializeBarrage();
            InitializeLayer();
            InitializeEdit();
            InitializeStatus();
            UpdateComponent();
        }
        void LoadConfig()
        {
            config = new Config();
            //TODO : Load config.
        }
        void InitializeFile()
        {
            Title = AppInfo.AppTitle + " - " + file.FileName;
            ImageList.ItemsSource = file.Images;
            SoundList.ItemsSource = file.Sounds;
            ScriptList.ItemsSource = file.Scripts;
        }
        void InitializeBarrage()
        {
            selectedBarrage = file.Barrages.First();
            commandStacks[selectedBarrage] = new CommandStack();
            BarrageTabControl.DataContext = config;
            AddNewBarrageTab(selectedBarrage);
            ComponentList.ItemsSource = selectedBarrage.Components;
            DeleteComponent.IsEnabled = false;
            BindComponent.IsEnabled = false;
            UnbindComponent.IsEnabled = false;
        }
        void InitializeLayer()
        {
            selectedLayer = selectedBarrage.Layers.First();
            LayerTree.ItemsSource = selectedBarrage.Layers;
            LayerAxis.ItemsSource = selectedBarrage.Layers;
            CopyLayer.IsEnabled = false;
            DeleteLayer.IsEnabled = false;
            SetLayer.IsEnabled = false;
        }
        void InitializeEdit()
        {
            Cut.IsEnabled = false;
            Copy.IsEnabled = false;
            Paste.IsEnabled = false;
            Undo.IsEnabled = false;
            Redo.IsEnabled = false;
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
        void UpdateBarrage()
        {
            ComponentList.ItemsSource = selectedBarrage.Components;
            DeleteComponent.IsEnabled = false;
            BindComponent.IsEnabled = false;
            UnbindComponent.IsEnabled = false;
        }
        void UpdateLayer()
        {
            InitializeLayer();
        }
        void UpdateComponent()
        {
            UpdateScreen();
            UpdateSelectedGroup();
        }
        #endregion
    }
}
