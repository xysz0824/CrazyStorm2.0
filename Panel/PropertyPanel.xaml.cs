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
        #region Private Members
        Script.Environment environment;
        IList<Resource> globals;
        CommandStack commandStack;
        ParticleSystem particle;
        Component component;
        Action updateFunc;
        bool initializeType;
        #endregion

        #region Constructor
        public PropertyPanel(CommandStack commandStack, 
            IList<Resource> globals, ParticleSystem particle, Component component, Action updateFunc)
        {
            this.commandStack = commandStack;
            this.globals = globals;
            this.particle = particle;
            this.component = component;
            this.updateFunc = updateFunc;
            InitializeComponent();
            InitializerEnvironment();
            LoadContent();
        }
        #endregion

        #region Private Methods
        void InitializerEnvironment()
        {
            environment = new Script.Environment();
            //Add globals.
            foreach (VariableResource item in globals)
                environment.PutGlobal(item.Label, item.Value);
            //Add system structs.
            Script.Struct vector2 = new Script.Struct();
            vector2.PutField("x", 0.0f);
            vector2.PutField("y", 0.0f);
            environment.PutStruct(typeof(Vector2).ToString(), vector2);
            Script.Struct vector3 = new Script.Struct();
            vector3.PutField("r", 0.0f);
            vector3.PutField("g", 0.0f);
            vector3.PutField("b", 0.0f);
            environment.PutStruct(typeof(RGB).ToString(), vector3);
            //Add system functions.
            Script.Function rand = new Script.Function(2);
            environment.PutFunction("rand", rand);
            Script.Function sin = new Script.Function(1);
            environment.PutFunction("sin", sin);
            Script.Function cos = new Script.Function(1);
            environment.PutFunction("cos", cos);
            Script.Function tan = new Script.Function(1);
            environment.PutFunction("tan", tan);
        }
        void LoadContent()
        {
            //Load component properties.
            var componentList = component.InitializeProperties(typeof(Component));
            LoadProperties(ComponentGrid, component, componentList);
            //Load specific properties.
            IList<PropertyInfo> specificList;
            if (component is Emitter)
                specificList = component.InitializeProperties(typeof(Emitter));
            else
                specificList = component.InitializeProperties(component.GetType());

            LoadProperties(SpecificGrid, component, specificList);
            //Load particle properties.
            if (component is MultiEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                var particleList = (component as MultiEmitter).Particle.InitializeProperties(typeof(Particle));
                LoadProperties(ParticleGrid, (component as MultiEmitter).Particle, particleList);
            }
            else if (component is CurveEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                var particleList = (component as CurveEmitter).CurveParticle.InitializeProperties(typeof(CurveParticle));
                LoadProperties(ParticleGrid, (component as CurveEmitter).CurveParticle, particleList);
            }
            //Load particle types.
            var types = new List<ParticleType>();
            foreach (var item in particle.CustomType)
            {
                bool exist = false;
                for (int i = 0; i < types.Count; ++i)
                    if (item.Name == types[i].Name)
                    {
                        exist = true;
                        break;
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
                //Prevent doing ColorCombo_SelectionChanged(),
                //or it will initialize again after selecting some type.
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
            //Put locals into environment.
            foreach (var item in component.Variables)
                environment.PutLocal(item.Label, item.Value);
            //Load component events.
            ComponentEventList.ItemsSource = component.ComponentEventGroups;
            //Load specific events.
            if (component is Emitter)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("ParticleEventList");
                SpecificEventList.ItemsSource = (component as Emitter).ParticleEventGroups;
            }
            else if (component is Mask)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("MaskEventList");
                SpecificEventList.ItemsSource = (component as Mask).MaskEventGroups;
            }
            else if (component is Rebound)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("ReboundEventList");
                SpecificEventList.ItemsSource = (component as Rebound).ReboundEventGroups;
            }   
        }
        void LoadProperties(FrameworkElement element, PropertyContainer container, IList<PropertyInfo> infos)
        {
            var propertyItems = new ObservableCollection<PropertyPanelItem>();
            foreach (var item in infos)
            {
                var property = new PropertyPanelItem()
                {
                    Info = item,
                    Name = item.Name,
                    Value = container.Properties[item].Value
                };
                propertyItems.Add(property);
                //Put the property into environment.
                environment.PutLocal(item.Name, item.GetGetMethod().Invoke(container, null));
            }
            element.DataContext = propertyItems;
        }
        void SetProperty(PropertyContainer container, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var property = e.Row.Item as PropertyPanelItem;
                var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
                var newValue = (e.EditingElement as TextBox).Text;
                var attribute = property.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                new SetPropertyCommand().Do(commandStack, environment, container, property, cell, newValue, attribute, updateFunc);
            }
        }
        void UpdateProperty(PropertyContainer container, ObservableCollection<PropertyPanelItem> properties)
        {
            foreach (var item in properties)
            {
                if (!container.Properties[item.Info].Expression)
                {
                    var result = item.Info.GetGetMethod().Invoke(container, null).ToString();
                    container.Properties[item.Info].Value = result;
                }
                item.Value = container.Properties[item.Info].Value;
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
        void OpenEventSetting(EventGroup eventGroup, Script.Environment environment)
        {
            Window window = new EventSetting(eventGroup, environment);
            window.ShowDialog();
        }
        #endregion

        #region Public Methods
        public void UpdateProperty()
        {
            //Update component property.
            var componentProperties = ComponentGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            UpdateProperty(component, componentProperties);
            //Update specific property.
            var specificProperties = SpecificGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            UpdateProperty(component, specificProperties);
            //Update particle property.
            var particleProperties = ParticleGrid.DataContext as ObservableCollection<PropertyPanelItem>;
            if (component is MultiEmitter)
            {
                var particle = (component as MultiEmitter).Particle;
                UpdateProperty(particle, particleProperties);
            }
            else if (component is CurveEmitter)
            {
                var curveParticle = (component as CurveEmitter).CurveParticle;
                UpdateProperty(curveParticle, particleProperties);
            }
        }
        #endregion

        #region Windows EventHandlers
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
            var label = "Local_";
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
                    //Put this into environment.
                    environment.PutLocal(newVar.Label, newVar.Value);
                    return;
                }
            }
        }
        private void DeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (VariableGrid.SelectedItem != null)
            {
                var item = VariableGrid.SelectedItem as VariableResource;
                component.Variables.Remove(item);
                DeleteVariable.IsEnabled = component.Variables.Count > 0 ? true : false;
                //Remove this from environment.
                environment.RemoveLocal(item.Label);
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
                            e.Cancel = true;
                            (e.EditingElement as TextBox).Text = editItem.Label;
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
                        e.Cancel = true;
                        (e.EditingElement as TextBox).Text = editItem.Value.ToString();
                        return;
                    }
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
                var selectedType = TypeCombo.SelectedItem as ParticleType;
                var selectedColor = ColorCombo.SelectedItem as ComboBoxItem;
                foreach (var item in particle.CustomType)
                {
                    if (item.Name == selectedType.Name && item.Color.ToString() == (string)selectedColor.Content)
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
        private void AddComponentEvent_Click(object sender, RoutedEventArgs e)
        {
            component.ComponentEventGroups.Add(new EventGroup());
        }
        private void AddSpecificEvent_Click(object sender, RoutedEventArgs e)
        {
            if (component is Emitter)
                (component as Emitter).ParticleEventGroups.Add(new EventGroup());
            else if (component is Mask)
                (component as Mask).MaskEventGroups.Add(new EventGroup());
            else if (component is Rebound)
                (component as Rebound).ReboundEventGroups.Add(new EventGroup());
        }
        private void DelComponentEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = ComponentEventList.SelectedItem as EventGroup;
            component.ComponentEventGroups.Remove(item);
        }
        private void DelSpecificEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = SpecificEventList.SelectedItem as EventGroup;
            if (component is Emitter)
                (component as Emitter).ParticleEventGroups.Remove(item);
            else if (component is Mask)
                (component as Mask).MaskEventGroups.Remove(item);
            else if (component is Rebound)
                (component as Rebound).ReboundEventGroups.Remove(item);
        }
        private void ComponentEventList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualHelper.FocusItem<TreeViewItem>(e);
        }
        private void SpecificEventList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VisualHelper.FocusItem<TreeViewItem>(e);
        }
        private void ComponentEventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && ComponentEventList.SelectedItem != null)
            {
                Script.Environment environment = new Script.Environment(this.environment);
                //Remove string property
                environment.RemoveLocal("Name");
                OpenEventSetting(ComponentEventList.SelectedItem as EventGroup, environment);
            }
        }
        private void SpecificEventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && SpecificEventList.SelectedItem != null)
            {
                if (component is Emitter)
                {
                    Script.Environment environment = new Script.Environment(this.environment);
                    //Remove emitter properties
                    var componentItems = ComponentGrid.DataContext as IList<PropertyPanelItem>;
                    var emitterItems = SpecificGrid.DataContext as IList<PropertyPanelItem>;
                    foreach (var item in componentItems)
                    {
                        environment.RemoveLocal(item.Name);
                    }
                    foreach (var item in emitterItems)
                    {
                        environment.RemoveLocal(item.Name);
                    }
                    OpenEventSetting(SpecificEventList.SelectedItem as EventGroup, environment);
                }
                else if (component is Mask || component is Rebound)
                {
                    Script.Environment environment = new Script.Environment();
                    //Put particle properties
                    environment.PutLocal("MaxLife", 0);
                    environment.PutLocal("Type", new ParticleType());
                    environment.PutLocal("RGB", RGB.Zero);
                    environment.PutLocal("Opacity", 0.0f);
                    environment.PutLocal("PSpeed", 0.0f);
                    environment.PutLocal("PSpeedAngle", 0.0f);
                    environment.PutLocal("PAcspeed", 0.0f);
                    environment.PutLocal("PAcspeedAngle", 0.0f);
                    environment.PutLocal("BlendType", BlendType.AlphaBlend);
                    environment.PutLocal("KillOutside", true);
                    environment.PutLocal("Collision", true);
                    OpenEventSetting(SpecificEventList.SelectedItem as EventGroup, environment);
                }
            }
        }
        #endregion
    }
}
