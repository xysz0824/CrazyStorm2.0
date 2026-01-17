/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            foreach (VariableResource item in component.Locals)
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
            SpecificPropertyGroup.Visibility = specificPropertyList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            //Load particle properties.
            if (component is Emitter)
            {
                ParticlePropertyGroup.Visibility = Visibility.Visible;
                particlePropertyList = (component as Emitter).Particle.InitializeAndGetProperties(typeof(ParticleBase));
                if (component is MultiEmitter)
                    particlePropertyList.AddRange((component as Emitter).Particle.InitializeAndGetProperties(typeof(Particle)));
                else
                    particlePropertyList.AddRange((component as Emitter).Particle.InitializeAndGetProperties(typeof(CurveParticle)));

                LoadProperties(ParticleGrid, (component as Emitter).Particle, particlePropertyList);
            }
            else
            {
                ParticlePropertyGroup.Visibility = Visibility.Collapsed;
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
            VariableGrid.ItemsSource = component.Locals;
            DeleteVariable.IsEnabled = component.Locals.Count > 0;
            //Load component events.
            ComponentEventList.ItemsSource = component.ComponentEventGroups;
            DelComponentEventButton.IsEnabled = component.ComponentEventGroups.Count > 0;
            //Load specific events.
            if (component is Emitter)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("ParticleEventGroupStr");
                var eventGroups = (component as Emitter).ParticleEventGroups;
                SpecificEventList.ItemsSource = eventGroups;
                DelSpecificEventButton.IsEnabled = eventGroups.Count > 0;
            }
            else if (component is EventField)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("EventFieldEventGroupStr");
                var eventGroups = (component as EventField).EventFieldEventGroups;
                SpecificEventList.ItemsSource = eventGroups;
                DelSpecificEventButton.IsEnabled = eventGroups.Count > 0;
            }
            else if (component is Rebounder)
            {
                SpecificGroup.Visibility = Visibility.Visible;
                SpecificGroup.Header = (string)FindResource("RebounderEventGroupStr");
                var eventGroups = (component as Rebounder).RebounderEventGroups;
                SpecificEventList.ItemsSource = eventGroups;
                DelSpecificEventButton.IsEnabled = eventGroups.Count > 0;
            }
            else
            {
                SpecificGroup.Visibility = Visibility.Collapsed;
                AddSpecificEventButton.Visibility = Visibility.Collapsed;
                DelSpecificEventButton.Visibility = Visibility.Collapsed;
            }
        }
        void LoadProperties(FrameworkElement element, PropertyContainer container, IList<PropertyInfo> infos)
        {
            var propertyItems = new ObservableCollection<PropertyGridItem>();
            foreach (var item in infos)
            {
                var attributes = item.GetCustomAttributes(false);
                var runtimeProperty = attributes.FirstOrDefault((attr) => attr is RuntimePropertyAttribute);
                var readonlyProperty = attributes.FirstOrDefault((attr) => attr is ReadOnlyPropertyAttribute);
                var value = container.Properties[item.Name].Value;
                if (!(attributes.Length > 0 && runtimeProperty != null))
                {
                    var property = new PropertyGridItem()
                    {
                        Info = item,
                        DisplayName = (string)FindResource(item.Name + "Str"),
                        DisplayValue = item.PropertyType != typeof(string) && readonlyProperty == null ? 
                            ExpressionHelper.Translate(value) : value,
                        ReadOnly = readonlyProperty != null,
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
                var text = (e.EditingElement as TextBox).Text;
                var newValue = property.Info.PropertyType != typeof(string) ? ExpressionHelper.ReverseTranslate(text) : text;
                var attribute = property.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                new SetPropertyCommand().Do(commandStack, environment, container, property, cell, newValue, attribute, updateFunc);
            }
        }
        void UpdateProperty(PropertyContainer container, IList<PropertyGridItem> properties)
        {
            foreach (var item in properties)
            {
                if (!item.ReadOnly && !container.Properties[item.Info.Name].Expression)
                {
                    var result = item.Info.GetGetMethod().Invoke(container, null).ToString();
                    container.Properties[item.Info.Name].Value = result;
                }
                var value = container.Properties[item.Info.Name].Value;
                item.DisplayValue = item.Info.PropertyType != typeof(string) && !item.ReadOnly ? 
                    ExpressionHelper.Translate(value) : value ;
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
                    color.Content = ExpressionHelper.Translate(item.Color.ToString());
                    ColorCombo.Items.Add(color);
                }
            }
        }
        void OpenEventSetting(EventGroup eventGroup, Expression.Environment environment, 
            bool emitter, bool aboutParticle, bool center)
        {
            file.UpdateResource();
            Window window = new EventSetting(eventGroup, environment, file.Sounds, types, emitter, aboutParticle, center);
            window.ShowDialog();
            window.Close();
        }
        void ShowIntellisense(PropertyContainer container, DataGridBeginningEditEventArgs e)
        {
            var property = e.Row.Item as PropertyGridItem;
            var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
            popup = new Popup();
            UIHelper.ShowIntellisense(popup, property.Info.PropertyType, cell);
        }
        void HideIntellisense()
        {
            UIHelper.HideIntellisense(popup);
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

                if (!exist) typesNorepeat.Add(item);
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
                {
                    if (item.Name == type.Name)
                    {
                        TypeCombo.SelectedItem = item;
                        break;
                    }
                }
                //Select specific color.
                InitializeColorCombo();
                for (int i = 0; i < ColorCombo.Items.Count; ++i)
                {
                    if (ExpressionHelper.Translate(type.Color.ToString()) == (string)((ColorCombo.Items[i] as ComboBoxItem).Content))
                    {
                        ColorCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (component is Emitter)
            {
                (component as Emitter).Particle.Type = types[0];
                type = (component as Emitter).Particle.Type;
                TypeCombo.SelectedItem = typesNorepeat[0];
                ColorCombo.SelectedIndex = 0;
            }
        }
        #endregion

        #region Window EventHandlers
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var item = e.Row.Item as PropertyGridItem;
            if (item.ReadOnly)
            {
                e.Cancel = true;
                return;
            }
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
            var label = (string)FindResource("LocalStr");
            for (int i = 0;; ++i)
            {
                //To avoid repeating name, use number.
                var name = label + i;
                bool ok = true;
                for (int k = 0;k < component.Locals.Count;++k)
                    if (component.Locals[k].Label == name)
                    {
                        ok = false;
                        break;
                    }

                if (ok)
                {
                    new AddLocalCommand().Do(commandStack, name, component, environment, DeleteVariable);
                    return;
                }
            }
        }
        private void DeleteVariable_Click(object sender, RoutedEventArgs e)
        {
            if (VariableGrid.SelectedItem != null)
            {
                var item = VariableGrid.SelectedItem as VariableResource;
                new DelLocalCommand().Do(commandStack, item, component, environment, DeleteVariable);
            }
        }
        private void VariableGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Grid_CellEditEnding(sender, e);
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var editItem = e.EditingElement.DataContext as VariableResource;
                var newValue = (e.EditingElement as TextBox).Text;
                if (e.Column.SortMemberPath == "Label")
                {
                    //Check the commit to avoid repeating name.
                    newValue = newValue.Trim();
                    foreach (var item in component.Locals)
                        if (item != editItem && item.Label == newValue)
                        {
                            MessageBox.Show((string)FindResource("NameRepeatingStr"), (string)FindResource("TipTitleStr"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Cancel = true;
                            (e.EditingElement as TextBox).Text = editItem.Label;
                            return;
                        }
                    new ModifyLocalCommand().Do(commandStack, editItem, newValue, editItem.Value.ToString(), environment);
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
                    new ModifyLocalCommand().Do(commandStack, editItem, editItem.Label, value.ToString(), environment);
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
                if (ColorCombo.Items.Count > 0) ColorCombo.SelectedIndex = 0;
                //Show default type preview
                TypeComboTip.Visibility = Visibility.Visible;
                TypeImageRect.Width = selectedType.Width;
                TypeImageRect.Height = selectedType.Height;
                var imageBrush = TypeImageRect.Fill as ImageBrush;
                if (selectedType.ID >= ParticleType.DefaultTypeIndex)
                {
                    var path = "pack://application:,,,/Images/barrages.png";
                    imageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                else
                {
                    var path = selectedType.Image.AbsolutePath;
                    imageBrush.ImageSource = new BitmapImage(new Uri(path));
                }
                var bitmap = imageBrush.ImageSource as BitmapImage;
                imageBrush.Viewbox = new Rect(selectedType.StartPointX / bitmap.PixelWidth,
                    selectedType.StartPointY / bitmap.PixelHeight,
                    (float)selectedType.Width / bitmap.PixelWidth, 
                    (float)selectedType.Height / bitmap.PixelHeight);
            }
        }
        bool typeChangingByUI;
        private void ColorCombo_SelectionChanging(object sender, InputEventArgs e)
        {
            typeChangingByUI = true;
        }
        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = TypeCombo.SelectedItem as ParticleType;
            ParticleColor selectedColor = default;
            if (ColorCombo.SelectedItem != null)
            {
                var str = (string)(ColorCombo.SelectedItem as ComboBoxItem).Content;
                str = ExpressionHelper.ReverseTranslate(str);
                selectedColor = (ParticleColor)Enum.Parse(typeof(ParticleColor), str);
                foreach (var type in types)
                {
                    if (type.Name == selectedType.Name && type.Color == selectedColor)
                    {
                        selectedType = type;
                        break;
                    }
                }
                if (typeChangingByUI)
                {
                    new SetParticleTypeCommand().Do(commandStack, component as Emitter, selectedType,
                        new Action<Emitter, ParticleType>(ParticleTypeUpdate));
                    typeChangingByUI = false;
                    ColorCombo.Focusable = false;
                }
            }
        }
        private void ParticleTypeUpdate(Emitter emitter, ParticleType type)
        {
            if (this == null) return;
            emitter.Particle.Type = type;
            var types = TypeCombo.ItemsSource as List<ParticleType>;
            foreach (var item in types)
            {
                if (item.Name == type.Name)
                {
                    TypeCombo.SelectedItem = item;
                    break;
                }
            }
            ColorCombo.SelectedIndex = ColorCombo.Items.IndexOf(
                ColorCombo.Items.Cast<ComboBoxItem>().First(i => (string)i.Content == 
                ExpressionHelper.Translate(type.Color.ToString())));
        }
        private void AddComponentEvent_Click(object sender, RoutedEventArgs e)
        {
            new AddEventGroupCommand().Do(commandStack, (string)FindResource("NewEventGroupStr"),
                component.ComponentEventGroups, DelComponentEventButton);
        }
        private void CopyComponentEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = ComponentEventList.SelectedItem as EventGroup;
            if (item != null)
            {
                new CopyEventGroupCommand().Do(commandStack, item, component.ComponentEventGroups, 
                    DelComponentEventButton);
            }
        }
        private void DelComponentEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = ComponentEventList.SelectedItem as EventGroup;
            if (item != null)
            {
                new DelEventGroupCommand().Do(commandStack, item, component.ComponentEventGroups,
                    DelComponentEventButton);
            }
        }
        private void AddSpecificEvent_Click(object sender, RoutedEventArgs e)
        {
            IList<EventGroup> groups = null;
            if (component is Emitter) groups = (component as Emitter).ParticleEventGroups;
            else if (component is EventField) groups = (component as EventField).EventFieldEventGroups;
            else if (component is Rebounder) groups = (component as Rebounder).RebounderEventGroups;
            new AddEventGroupCommand().Do(commandStack, (string)FindResource("NewEventGroupStr"),
                groups, DelSpecificEventButton);
        }
        private void CopySpecificEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = SpecificEventList.SelectedItem as EventGroup;
            if (item != null)
            {
                IList<EventGroup> groups = null;
                if (component is Emitter) groups = (component as Emitter).ParticleEventGroups;
                else if (component is EventField) groups = (component as EventField).EventFieldEventGroups;
                else if (component is Rebounder) groups = (component as Rebounder).RebounderEventGroups;
                new CopyEventGroupCommand().Do(commandStack, item, groups, DelSpecificEventButton);
            }
        }
        private void DelSpecificEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = SpecificEventList.SelectedItem as EventGroup;
            if (item != null)
            {
                IList<EventGroup> groups = null;
                if (component is Emitter) groups = (component as Emitter).ParticleEventGroups;
                else if (component is EventField) groups = (component as EventField).EventFieldEventGroups;
                else if (component is Rebounder) groups = (component as Rebounder).RebounderEventGroups;
                new DelEventGroupCommand().Do(commandStack, item, groups, DelSpecificEventButton);
            }
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
                    component is Emitter, component is Emitter, component is Center);
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
               
                OpenEventSetting(SpecificEventList.SelectedItem as EventGroup, environment, false, true, false);
            }
        }
        #endregion
    }
}
