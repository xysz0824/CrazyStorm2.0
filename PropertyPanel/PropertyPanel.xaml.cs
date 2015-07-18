/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrazyStorm.Core;
using System.Collections.ObjectModel;

namespace CrazyStorm
{
    /// <summary>
    /// PropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyPanel : UserControl
    {
        CommandStack commandStack;
        ParticleSystem particle;
        Component component;
        Action updateFunc;
        bool initializeType;
        bool invalidVariable;
        public bool InvalidVariable { get { return invalidVariable; } }
        public PropertyPanel(CommandStack commandStack, ParticleSystem particle, Component component, Action updateFunc)
        {
            this.commandStack = commandStack;
            this.particle = particle;
            this.component = component;
            this.updateFunc = updateFunc;
            InitializeComponent();
           //Load component properties.
            var componentProperties = new ObservableCollection<PropertyPanelItem>();
            var componentList = component.InitializeProperties(typeof(Component));
            foreach (var item in componentList)
            {
                var property = new PropertyPanelItem() {
                    Info = item,
                    Name = item.Name,
                    Value = component.Properties[item].Value
                };
                componentProperties.Add(property);
            }
            ComponentGrid.DataContext = componentProperties;
            //Load specific properties.
            var specificProperties = new ObservableCollection<PropertyPanelItem>();
            IList<PropertyInfo> specificList;
            if (component is Emitter)
                specificList = component.InitializeProperties(typeof(Emitter));
            else
                specificList = component.InitializeProperties(component.GetType());

            foreach (var item in specificList)
            {
                var property = new PropertyPanelItem()
                {
                    Info = item,
                    Name = item.Name,
                    Value = component.Properties[item].Value
                };
                specificProperties.Add(property);
            }
            SpecificGrid.DataContext = specificProperties;
            //Load particle properties.
            if (component is MultiEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                var particleProperties = new ObservableCollection<PropertyPanelItem>();
                var particleList = (component as MultiEmitter).Particle.InitializeProperties(typeof(Particle));
                foreach (var item in particleList)
                {
                    var property = new PropertyPanelItem()
                    {
                        Info = item,
                        Name = item.Name,
                        Value = (component as MultiEmitter).Particle.Properties[item].Value
                    };
                    particleProperties.Add(property);
                }
                ParticleGrid.DataContext = particleProperties;
            }
            else if (component is CurveEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                var particleProperties = new ObservableCollection<PropertyPanelItem>();
                var particleList = (component as CurveEmitter).CurveParticle.InitializeProperties(typeof(CurveParticle));
                foreach (var item in particleList)
                {
                    var property = new PropertyPanelItem()
                    {
                        Info = item,
                        Name = item.Name,
                        Value = (component as CurveEmitter).CurveParticle.Properties[item].Value
                    };
                    particleProperties.Add(property);
                }
                ParticleGrid.DataContext = particleProperties;
            }
            //Load particle types.
            var types = new List<ParticleType>();
            foreach (var item in particle.CustomType)
            {
                bool exist = false;
                for (int i = 0;i < types.Count; ++i)
                {
                    if (item.Name == types[i].Name)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                    types.Add(item);
            }
            TypeCombo.ItemsSource = types;
            ParticleType type = null;
            if (component is MultiEmitter)
                type = (component as MultiEmitter).Particle.Type;
            else if (component is CurveEmitter)
                type = (component as CurveEmitter).CurveParticle.Type;
            if (type != null)
            {
                //Select specific type.
                foreach (var item in types)
                    if (item.Name == type.Name)
                    {
                        TypeCombo.SelectedItem = item;
                        break;
                    }
                //Avoid activing ColorCombo_SelectionChanged()
                //otherwise it would initialize again after selecting specific type.
                initializeType = true;
                //Select specific color.
                InitializeColorCombo();
                for (int i = 0; i < ColorCombo.Items.Count; ++i)
                    if (type.Color.ToString() == (string)((ColorCombo.Items[i] as ComboBoxItem).Content))
                    {
                        ColorCombo.SelectedIndex = i;
                        break;
                    }
            }
            //Load variables.
            VariableGrid.ItemsSource = component.Variables;
            DeleteVariable.IsEnabled = component.Variables.Count > 0 ? true : false;
        }

        void SetProperty(PropertyContainer container, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var property = e.Row.Item as PropertyPanelItem;
                var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
                var newValue = (e.EditingElement as TextBox).Text;
                if (property.Value == newValue)
                    return;
                var attribute = property.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                new SetPropertyCommand().Do(commandStack, container, property, cell, newValue, attribute, updateFunc);
            }
        }

        public void UpdateProperty()
        {
            //Update component property.
            var componentProperties = ComponentGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            foreach (var item in componentProperties)
            {
                var result = item.Info.GetGetMethod().Invoke(component, null).ToString();
                component.Properties[item.Info].Value = result;
                item.Value = component.Properties[item.Info].Value;
            }
            //Update specific property.
            var specificProperties = SpecificGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            foreach (var item in specificProperties)
            {
                var result = item.Info.GetGetMethod().Invoke(component, null).ToString();
                component.Properties[item.Info].Value = result;
                item.Value = component.Properties[item.Info].Value;
            }
            //Update particle property.
            var particleProperties = ParticleGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            if (component is MultiEmitter)
            {
                var particle = (component as MultiEmitter).Particle;
                foreach (var item in particleProperties)
                {
                    var result = item.Info.GetGetMethod().Invoke(particle, null).ToString();
                    particle.Properties[item.Info].Value = result;
                    item.Value = particle.Properties[item.Info].Value;
                }
            }
            else if (component is CurveEmitter)
            {
                var curveParticle = (component as CurveEmitter).CurveParticle;
                foreach (var item in particleProperties)
                {
                    var result = item.Info.GetGetMethod().Invoke(curveParticle, null).ToString();
                    curveParticle.Properties[item.Info].Value = result;
                    item.Value = curveParticle.Properties[item.Info].Value;
                }
            }
        }

        private void ComponentGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetProperty(component, e);
        }

        private void SpecificGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetProperty(component, e);
        }

        private void ParticleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (component is MultiEmitter)
                SetProperty((component as MultiEmitter).Particle, e);
            else if (component is CurveEmitter)
                SetProperty((component as CurveEmitter).CurveParticle, e);
        }

        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            var label = "Untitled_";
            for (int i = 0;; ++i)
            {
                //To avoid repeating name, use number.
                var name = label + i;
                bool ok = true;
                for (int k = 0;k < component.Variables.Count;++k)
                    if (component.Variables[k].Label == name)
                    {
                        ok = false;
                        break;
                    }

                if (ok)
                {
                    var newVar = new VariableResource(name);
                    component.Variables.Add(newVar);
                    DeleteVariable.IsEnabled = true;
                    return;
                }
            }
        }

        private void DeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (VariableGrid.SelectedItem != null)
            {
                component.Variables.Remove(VariableGrid.SelectedItem as VariableResource);
                DeleteVariable.IsEnabled = component.Variables.Count > 0 ? true : false;
            }
        }

        private void VariableGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editItem = e.EditingElement.DataContext as VariableResource;
                var newValue = (e.EditingElement as TextBox).Text;
                if (e.Column.SortMemberPath == "Label")
                {
                    //Check the commit to avoid repeating name.
                    newValue = newValue.Trim();
                    foreach (var item in component.Variables)
                        if (item != editItem && item.Label == newValue)
                        {
                            MessageBox.Show((string)FindResource("NameRepeating"), (string)FindResource("TipTitle"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            invalidVariable = true;
                            return;
                        }
                }
                else if (e.Column.SortMemberPath == "Value")
                {
                    //Check the commit to avoid invalid value.
                    float value;
                    bool result = float.TryParse(newValue, out value);
                    if (!result)
                    {
                        MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        invalidVariable = true;
                        return;
                    }
                }
                invalidVariable = false;
            }
        }

        void InitializeColorCombo()
        {
            ColorCombo.Items.Clear();
            var selectedItem = TypeCombo.SelectedItem as ParticleType;
            foreach (var item in particle.CustomType)
            {
                if (item.Name == selectedItem.Name)
                {
                    var color = new ComboBoxItem();
                    color.Content = item.Color.ToString();
                    ColorCombo.Items.Add(color);
                }
            }
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeCombo.SelectedItem != null && !initializeType)
            {
                //Refresh color combobox.
                InitializeColorCombo();
            }
            initializeType = false;
        }

        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorCombo.SelectedItem != null)
            {
                var selectedItem = TypeCombo.SelectedItem as ParticleType;
                var color = ColorCombo.SelectedItem as ComboBoxItem;
                foreach (var item in particle.CustomType)
                {
                    if (item.Name == selectedItem.Name && item.Color.ToString() == (string)color.Content)
                    {
                        if (component is MultiEmitter)
                            (component as MultiEmitter).Particle.Type = item;
                        else if (component is CurveEmitter)
                            (component as CurveEmitter).CurveParticle.Type = item;
                        break;
                    }
                }
            }
        }
    }
}
