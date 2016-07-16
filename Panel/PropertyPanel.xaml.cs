/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
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
    public partial class PropertyPanel : UserControl
    {
        #region Private Members
        Expression.Environment environment;
        File file;
        CommandStack commandStack;
        List<ParticleType> types;
        Component component;
        List<PropertyInfo> componentPropertyList;
        List<PropertyInfo> specificPropertyList;
        List<PropertyInfo> particlePropertyList;
        Action updateFunc;
        string editText;
        Popup popup;
        bool initializeType;
        #endregion

        #region Public Members
        public event Action OnBeginEditing;
        public event Action OnEndEditing;
        #endregion

        #region Constructor
        public PropertyPanel(CommandStack commandStack, File file,
            List<ParticleType> types, Component component, Action updateFunc)
        {
            this.commandStack = commandStack;
            this.file = file;
            this.types = types;
            this.component = component;
            this.updateFunc = updateFunc;
            InitializeComponent();
            InitializeEnvironment();
            LoadContent();
        }
        #endregion

        #region Private Methods
        void InitializeEnvironment()
        {
            environment = new Expression.Environment();
            //Put globals.
            foreach (VariableResource item in file.Globals)
                environment.PutGlobal(item.Label, item.Value);
            //Put locals.
            foreach (VariableResource item in component.Variables)
                environment.PutLocal(item.Label, item.Value);
        }
        void LoadContent()
        {
            //Load component properties.
            componentPropertyList = component.InitializeAndGetProperties(typeof(Component));
            LoadProperties(ComponentGrid, component, componentPropertyList);
            //Load specific properties.
            if (component is Emitter)
                specificPropertyList = component.InitializeAndGetProperties(typeof(Emitter));
            else
                specificPropertyList = component.InitializeAndGetProperties(component.GetType());

            LoadProperties(SpecificGrid, component, specificPropertyList);
            //Load particle properties.
            if (component is Emitter)
            {
                ParticleGroup.Visibility = Visibility.Visible;
                particlePropertyList = (component as Emitter).Particle.InitializeAndGetProperties(typeof(ParticleBase));
                if (component is MultiEmitter)
                    particlePropertyList.AddRange((component as Emitter).Particle.InitializeAndGetProperties(typeof(Particle)));
                else
                    particlePropertyList.AddRange((component as Emitter).Particle.InitializeAndGetProperties(typeof(CurveParticle)));

                LoadProperties(ParticleGrid, (component as Emitter).Particle, particlePropertyList);
            }
            else
            {
                ParticleGroup.Visibility = Visibility.Collapsed;
                if (component is EventField || component is Rebounder)
                {
                    //Only emitter have particles, but special event of event field or rebounder need it.
                    var stub = new MultiEmitter();
                    particlePropertyList = stub.Particle.InitializeAndGetProperties(typeof(ParticleBase));
                    LoadProperties(ParticleGrid, stub.Particle, particlePropertyList);
                }
            }
            //Load particle types.
            //First needs to merge repeated type name.
            LoadTypes(types);
            //Load variables.
            VariableGrid.ItemsSource = component.Variables;
            DeleteVariable.IsEnabled = component.Variables.Count > 0 ? true : false;
            //Load component events.
            ComponentEventList.ItemsSource = component.ComponentEventGroups;
            //Load specific events.
            if (component is Emitter)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("ParticleEventListStr");
                SpecificEventList.ItemsSource = (component as Emitter).ParticleEventGroups;
            }
            else if (component is EventField)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("EventFieldEventListStr");
                SpecificEventList.ItemsSource = (component as EventField).EventFieldEventGroups;
            }
            else if (component is Rebounder)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("RebounderEventListStr");
                SpecificEventList.ItemsSource = (component as Rebounder).RebounderEventGroups;
            }
            else
                SpecificGroup.Visibility = Visibility.Collapsed;
        }
        void LoadProperties(FrameworkElement element, PropertyContainer container, IList<PropertyInfo> infos)
        {
            var propertyItems = new ObservableCollection<PropertyGridItem>();
            foreach (var item in infos)
            {
                var attributes = item.GetCustomAttributes(false);
                if (!(attributes.Length > 0 && attributes[0] is RuntimePropertyAttribute))
                {
                    var property = new PropertyGridItem()
                    {
                        Info = item,
                        Name = item.Name,
                        DisplayName = (string)FindResource(item.Name + "Str"),
                        Value = container.Properties[item.Name].Value
                    };
                    propertyItems.Add(property);
                }
                //Put this property into environment.
                environment.PutProperty(item.Name, item.GetGetMethod().Invoke(container, null));
            }
            element.DataContext = propertyItems;
        }
        void SetProperty(PropertyContainer container, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var property = e.Row.Item as PropertyGridItem;
                var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
                var newValue = (e.EditingElement as TextBox).Text;
                var attribute = property.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                new SetPropertyCommand().Do(commandStack, environment, container, property, cell, newValue, attribute, updateFunc);
            }
        }
        void UpdateProperty(PropertyContainer container, IList<PropertyGridItem> properties)
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
                    color.Content = (string)FindResource(item.Color.ToString() + "Str");
                    ColorCombo.Items.Add(color);
                }
            }
        }
        void OpenEventSetting(EventGroup eventGroup, Expression.Environment environment, bool emitter, bool aboutParticle)
        {
            file.UpdateResource();
            Window window = new EventSetting(eventGroup, environment, file.Sounds, types, emitter, aboutParticle);
            window.ShowDialog();
            window.Close();
        }
        void ShowIntellisense(PropertyContainer container, DataGridBeginningEditEventArgs e)
        {
            var property = e.Row.Item as PropertyGridItem;
            var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
            popup = new Popup();
            popup.PlacementTarget = cell;
            popup.Placement = PlacementMode.Bottom;
            popup.PopupAnimation = PopupAnimation.Fade;
            var listView = new ListView();
            if (property.Info.PropertyType == typeof(bool))
            {
                listView.Items.Add(true.ToString());
                listView.Items.Add(false.ToString());
            }
            else if (property.Info.PropertyType.IsSubclassOf(typeof(Enum)))
            {
                Array array = Enum.GetValues(property.Info.PropertyType);
                foreach (var item in array)
                    listView.Items.Add(item.ToString());
            }
            if (!listView.Items.IsEmpty)
            {
                listView.PreviewMouseLeftButtonDown += (sender, args) =>
                {
                    args.Handled = true;
                    if (!(args.OriginalSource is TextBlock))
                        return;

                    var textBox = cell.Content as TextBox;
                    textBox.Text = (args.OriginalSource as TextBlock).Text;
                };
                popup.Child = listView;
                popup.IsOpen = true;
            }
        }
        void HideIntellisense()
        {
            popup.Child = null;
            popup.IsOpen = false;
        }
        #endregion

        #region Public Methods
        public void UpdateProperty()
        {
            //Update component property.
            var componentProperties = ComponentGrid.DataContext as IList<PropertyGridItem>;
            UpdateProperty(component, componentProperties);
            //Update specific property.
            var specificProperties = SpecificGrid.DataContext as IList<PropertyGridItem>;
            UpdateProperty(component, specificProperties);
            //Update particle property.
            var particleProperties = ParticleGrid.DataContext as IList<PropertyGridItem>;
            if (component is Emitter)
            {
                var particle = (component as Emitter).Particle;
                UpdateProperty(particle, particleProperties);
            }
        }
        public void UpdateGlobals(UpdateType type, VariableResource variable, string newName, float newValue)
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
                    environment.PutGlobal(newName, newValue);
                    break;
            }
        }
        public void LoadTypes(List<ParticleType> types)
        {
            this.types = types;
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
            if (component is Emitter)
            {
                type = (component as Emitter).Particle.Type;
                if (!types.Contains(type))
                {
                    ColorCombo.Items.Clear();
                    (component as Emitter).Particle.Type = null;
                    type = null;
                }
            }
            if (type != null)
            {
                //Select specific type.
                foreach (var item in typesNorepeat)
                    if (item.Name == type.Name)
                    {
                        TypeCombo.SelectedItem = item;
                        break;
                    }
                //Select specific color.
                InitializeColorCombo();
                for (int i = 0; i < ColorCombo.Items.Count; ++i)
                    if ((string)FindResource(type.Color.ToString() + "Str") == (string)((ColorCombo.Items[i] as ComboBoxItem).Content))
                    {
                        ColorCombo.SelectedIndex = i;
                        break;
                    }
            }
        }
        #endregion

        #region Window EventHandlers
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var textBlock = e.EditingEventArgs.OriginalSource as TextBlock;
            if (textBlock != null)
            {
                editText = textBlock.Text;
                if (OnBeginEditing != null)
                    OnBeginEditing();
            }
        }
        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (OnEndEditing != null)
                OnEndEditing();
        }
        private void ComponentGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Grid_BeginningEdit(sender, e);
            ShowIntellisense(component, e);
        }
        private void ComponentGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Grid_CellEditEnding(sender, e);
            if ((e.EditingElement as TextBox).Text != editText)
                SetProperty(component, e);

            HideIntellisense();
        }
        private void ParticleGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Grid_BeginningEdit(sender, e);
            ShowIntellisense((component as Emitter).Particle, e);
        }
        private void ParticleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Grid_CellEditEnding(sender, e);
            if ((e.EditingElement as TextBox).Text != editText)
            {
                if (component is Emitter)
                    SetProperty((component as Emitter).Particle, e);
            }
            HideIntellisense();
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
                            MessageBox.Show((string)FindResource("NameRepeatingStr"), (string)FindResource("TipTitleStr"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Cancel = true;
                            (e.EditingElement as TextBox).Text = editItem.Label;
                            return;
                        }
                    //Modify local variable name
                    environment.RemoveLocal(editItem.Label);
                    environment.PutLocal(newValue, editItem.Value);
                }
                else if (e.Column.SortMemberPath == "Value")
                {
                    //Check the commit to avoid invalid value.
                    float value;
                    bool result = float.TryParse(newValue, out value);
                    if (!result)
                    {
                        MessageBox.Show((string)FindResource("ValueInvalidStr"), (string)FindResource("TipTitleStr"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        e.Cancel = true;
                        (e.EditingElement as TextBox).Text = editItem.Value.ToString();
                        return;
                    }
                    //Modify local variable value
                    environment.RemoveLocal(editItem.Label);
                    environment.PutLocal(editItem.Label, value);
                }
            }
        }
        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeCombo.SelectedItem != null)
            {
                var selectedType = TypeCombo.SelectedItem as ParticleType;
                //Refresh color combobox.
                InitializeColorCombo();
                //Show default type preview
                if (selectedType.Id >= ParticleType.DefaultTypeIndex)
                {
                    TypeComboTip.Visibility = Visibility.Visible;
                    TypeImageRect.Width = selectedType.Width;
                    TypeImageRect.Height = selectedType.Height;
                    var imageBrush = TypeImageRect.Fill as ImageBrush;
                    var bitmap = imageBrush.ImageSource as BitmapFrame;
                    imageBrush.Viewbox = new Rect(selectedType.StartPointX / bitmap.PixelWidth,
                        selectedType.StartPointY / bitmap.PixelHeight,
                        (float)selectedType.Width / bitmap.PixelWidth,
                        (float)selectedType.Height / bitmap.PixelHeight);
                }
                else
                    TypeComboTip.Visibility = Visibility.Hidden;
            }
        }
        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorCombo.SelectedItem != null && !initializeType)
            {
                var selectedType = TypeCombo.SelectedItem as ParticleType;
                var selectedColor = ColorCombo.SelectedItem as ComboBoxItem;
                foreach (var item in types)
                {
                    if (item.Name == selectedType.Name && 
                        (string)FindResource(item.Color.ToString() + "Str") == (string)selectedColor.Content)
                    {
                        (component as Emitter).Particle.Type = item;
                        break;
                    }
                }
            }
            initializeType = false;
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
                var environment = new Expression.Environment(this.environment);
                //Remove particle properties if the component isn't an emitter
                if (!(component is Emitter) && particlePropertyList != null)
                {
                    foreach (var item in particlePropertyList)
                        environment.RemoveProperty(item.Name);
                }
                OpenEventSetting(ComponentEventList.SelectedItem as EventGroup, environment, 
                    component is Emitter, component is Emitter);
            }
        }
        private void SpecificEventList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock && SpecificEventList.SelectedItem != null)
            {
                var environment = new Expression.Environment(this.environment);
                //Remove component and specific properties
                foreach (var item in componentPropertyList)
                    environment.RemoveProperty(item.Name);

                foreach (var item in specificPropertyList)
                    environment.RemoveProperty(item.Name);
               
                OpenEventSetting(SpecificEventList.SelectedItem as EventGroup, environment, false, true);
            }
        }
        #endregion
    }
}
