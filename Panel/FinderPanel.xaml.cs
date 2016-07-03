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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrazyStorm.Core;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace CrazyStorm
{
    public partial class FinderPanel : UserControl
    {
        #region Private Members
        ParticleSystem selectedParticle;
        bool selecting;
        #endregion

        #region Public Members
        public event Action<Component> OnSelectComponent;
        #endregion

        #region Constructor
        public FinderPanel(ParticleSystem selectedParticle)
        {
            InitializeComponent();
            InitializeTypes();
            Update(selectedParticle);
        }
        #endregion

        #region Private Methods
        private void InitializeTypes()
        {
            Type[] type = { typeof(MultiEmitter), typeof(CurveEmitter), typeof(EventField), typeof(Rebounder), typeof(ForceField) };
            for (int i = 0;i < type.Length;++i)
            {
                var item = new ComboBoxItem();
                item.DataContext = type[i];
                item.Content = (string)FindResource(type[i].Name + "Str");
                TypeComboBox.Items.Add(item);
            }
        }
        private void InitializeLayers()
        {
            LayerComboBox.ItemsSource = selectedParticle.Layers;
            LayerComboBox.SelectedIndex = 0;
        }
        private List<Component> Search()
        {
            List<Component> results = null;
            if (LayerCheckBox.IsChecked.Value)
                results = SearchLayer();
            else
                results = SearchTree(selectedParticle.ComponentTree);

            ResultCount.Text = string.Format((string)FindResource("ResultCountStr"), results.Count.ToString());
            return results;
        }
        private List<Component> SearchLayer()
        {
            var results = new List<Component>();
            var layer = LayerComboBox.SelectedItem as Layer;
            if (layer == null)
                return results;

            foreach (var component in layer.Components)
            {
                if (string.IsNullOrWhiteSpace(InputName.Text) || 
                    Regex.IsMatch(component.Name,InputName.Text, RegexOptions.IgnoreCase))
                {
                    if (TypeCheckBox.IsChecked.Value)
                    {
                        var type = (TypeComboBox.SelectedItem as ComboBoxItem).DataContext as Type;
                        if (component.GetType() != type)
                            continue;
                    }
                    results.Add(component);
                }
            }
            return results;
        }
        private List<Component> SearchTree(IList<Component> components)
        {
            var results = new List<Component>();
            if (components != null)
            {
                foreach (var component in components)
                {
                    if (string.IsNullOrWhiteSpace(InputName.Text) || 
                        Regex.IsMatch(component.Name, InputName.Text, RegexOptions.IgnoreCase))
                    {
                        if (TypeCheckBox.IsChecked.Value)
                        {
                            var type = (TypeComboBox.SelectedItem as ComboBoxItem).DataContext as Type;
                            if (component.GetType() != type)
                                continue;
                        }
                        results.Add(component);
                    }
                    results.AddRange(SearchTree(component.Children));
                }
            }
            return results;
        }
        #endregion

        #region Public Methods
        public void Update(ParticleSystem selectedParticle)
        {
            if (!selecting)
            {
                this.selectedParticle = selectedParticle;
                InitializeLayers();
                ComponentTree.ItemsSource = Search();
            }
        }
        #endregion

        #region Window EventHandlers
        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selecting = true;
            if (OnSelectComponent != null)
                OnSelectComponent((sender as TextBlock).DataContext as Component);
            selecting = false;
        }
        private void InputName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!selecting && selectedParticle != null)
                ComponentTree.ItemsSource = Search();
        }
        private void LayerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            InputName_TextChanged(null, null);
        }
        private void TypeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            InputName_TextChanged(null, null);
        }
        private void LayerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InputName_TextChanged(null, null);
        }
        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InputName_TextChanged(null, null);
        }
        #endregion
    }
}
