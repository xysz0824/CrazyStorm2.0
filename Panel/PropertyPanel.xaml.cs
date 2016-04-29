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
        File file;
        CommandStack commandStack;
        IList<ParticleType> types;
        Component component;
        Action updateFunc;
        bool initializeType;
        #endregion

        #region Public Members
        public event Action OnBeginEditing;
        public event Action OnEndEditing;
        #endregion

        #region Constructor
        public PropertyPanel(CommandStack commandStack,
            File file, IList<ParticleType> types, Component component, Action updateFunc)
        {
            this.commandStack = commandStack;
            this.file = file;
            this.types = types;
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
            foreach (VariableResource item in file.Globals)
                environment.PutGlobal(item.Label, item.Value);
            //Add system structs.
            Script.Struct vector2 = new Script.Struct();
            vector2.PutField("x", 0.0f);
            vector2.PutField("y", 0.0f);
            environment.PutStruct(typeof(Vector2).ToString(), vector2);
            Script.Struct rgb = new Script.Struct();
            rgb.PutField("r", 0.0f);
            rgb.PutField("g", 0.0f);
            rgb.PutField("b", 0.0f);
            environment.PutStruct(typeof(RGB).ToString(), rgb);
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
            //Only emitter have particles, but special event of mask or rebound need it.
            //So there use stub to load particle properties.
            var stub = new MultiEmitter();
            var particleList = stub.Particle.InitializeProperties(typeof(Particle));
            LoadProperties(ParticleGrid, stub.Particle, particleList);
            if (component is MultiEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                particleList = (component as MultiEmitter).Particle.InitializeProperties(typeof(Particle));
                LoadProperties(ParticleGrid, (component as MultiEmitter).Particle, particleList);
            }
            else if (component is CurveEmitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                particleList = (component as CurveEmitter).CurveParticle.InitializeProperties(typeof(CurveParticle));
                LoadProperties(ParticleGrid, (component as CurveEmitter).CurveParticle, particleList);
            }
            //Load particle types.
            //First needs to merge repeated type name.
            var typesNorepeat = new List<ParticleType>();
            foreach (var item in types)
            {
                bool exist = false;
                for (int i = 0; i < typesNorepeat.Count; ++i)
                    if (item.Name == typesNorepeat[i].Name)
                    {
                        exist = true;
                        break;
                    }

                if (!exist)
                    typesNorepeat.Add(item);
            }
            TypeCombo.ItemsSource = typesNorepeat;
            ParticleType type = null;
            if (component is MultiEmitter)
                type = (component as MultiEmitter).Particle.Type;
            else if (component is CurveEmitter)
                type = (component as CurveEmitter).CurveParticle.Type;

            if (type != null)
            {
                //Select specific type.
                foreach (var item in typesNorepeat)
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
            else if (component is EventField)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("EventFieldEventList");
                SpecificEventList.ItemsSource = (component as EventField).EventFieldEventGroups;
            }
            else if (component is Rebounder)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("RebounderEventList");
                SpecificEventList.ItemsSource = (component as Rebounder).RebounderEventGroups;
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
                    Value = container.Properties[item.Name].Value
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
        void UpdateProperty(PropertyContainer container, IList<PropertyPanelItem> properties)
        {
            foreach (var item in properties)
            {
                if (!container.Properties[item.Info.Name].Expression)
                {
                    var result = item.Info.GetGetMethod().Invoke(container, null).ToString();
                    container.Properties[item.Info.Name].Value = result;
                }
                item.Value = container.Properties[item.Info.Name].Value;
            }
        }
        void InitializeColorCombo()
        {
            ColorCombo.Items.Clear();
            var selectedItem = TypeCombo.SelectedItem as ParticleType;
            foreach (var item in types)
            {
                if (item.Name == selectedItem.Name)
                {
                    var color = new ComboBoxItem();
                    color.Content = item.Color.ToString();
                    ColorCombo.Items.Add(color);
                }
            }
        }
        void OpenEventSetting(EventGroup eventGroup, Script.Environment environment, bool emitter, bool aboutParticle)
        {
            var properties = new IList<PropertyPanelItem>[3]
            { 
                ComponentGrid.DataContext as IList<PropertyPanelItem>, 
                SpecificGrid.DataContext as IList<PropertyPanelItem>, 
                ParticleGrid.DataContext as IList<PropertyPanelItem>
            };
            file.UpdateResource();
            Window window = new EventSetting(eventGroup, environment, file.Sounds, types, properties, emitter, aboutParticle);
            window.ShowDialog();
        }
        #endregion

        #region Public Methods
        public void UpdateProperty()
        {
            //Update component property.
            var componentProperties = ComponentGrid.DataContext as IList<PropertyPanelItem>;
            UpdateProperty(component, componentProperties);
            //Update specific property.
            var specificProperties = SpecificGrid.DataContext as IList<PropertyPanelItem>;
            UpdateProperty(component, specificProperties);
            //Update particle property.
            var particleProperties = ParticleGrid.DataContext as IList<PropertyPanelItem>;
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
        public void UpdateGlobals(UpdateType type, VariableResource variable, string newName)
        {
            switch (type)
            {
                case UpdateType.Add:
                    environment.PutGlobal(variable.Label, variable.Value);
                    break;
                case UpdateType.Delete:
                    environment.RemoveGlobal(variable.Label);
                    break;
                case UpdateType.Modify:
                    environment.RemoveGlobal(variable.Label);
                    environment.PutGlobal(newName, variable.Value);
                    break;
            }
        }
        #endregion

        #region Window EventHandlers
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (OnBeginEditing != null)
                OnBeginEditing();
        }
        private void ComponentGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetProperty(component, e);
            if (OnEndEditing != null)
                OnEndEditing();
        }
        private void SpecificGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            SetProperty(component, e);
            if (OnEndEditing != null)
                OnEndEditing();
        }
        private void ParticleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (component is MultiEmitter)
                SetProperty((component as MultiEmitter).Particle, e);
            else if (component is CurveEmitter)
                SetProperty((component as CurveEmitter).CurveParticle, e);

            if (OnEndEditing != null)
                OnEndEditing();
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
                foreach (var item in types)
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
            else if (component is EventField)
                (component as EventField).EventFieldEventGroups.Add(new EventGroup());
            else if (component is Rebounder)
                (component as Rebounder).RebounderEventGroups.Add(new EventGroup());
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
            else if (component is EventField)
                (component as EventField).EventFieldEventGroups.Remove(item);
            else if (component is Rebounder)
                (component as Rebounder).RebounderEventGroups.Remove(item);
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
                //Remove unnecessary properties
                environment.RemoveLocal("Name");
                //Add runtime component properties
                environment.PutLocal("CurrentFrame", 0);
                OpenEventSetting(ComponentEventList.SelectedItem as EventGroup, environment, 
                    component is Emitter, component is Emitter);
            }
        }
        private void SpecificEventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && SpecificEventList.SelectedItem != null)
            {
                Script.Environment environment = new Script.Environment(this.environment);
                //Remove component and emitter properties
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
                if (component is EventField || component is Rebounder)
                {
                    //For compatibility between particle and curveparticle, 
                    //Put uncommon particle properties
                    environment.PutLocal("Length", 0);
                }
                //Add runtime particle properties
                environment.PutLocal("Position", Vector2.Zero);
                environment.PutLocal("CurrentFrame", 0);
                OpenEventSetting(SpecificEventList.SelectedItem as EventGroup, environment, 
                    false, true);
            }
        }
        #endregion
    }
}
