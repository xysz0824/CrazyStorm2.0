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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using CrazyStorm_Player;

namespace CrazyStorm
{
    public partial class PropertyPanel : UserControl
    {
        #region Private Members
        Config config;
        Expression.Environment environment;
        File file;
        CommandStack commandStack;
        List<ParticleType> types;
        List<MaskType> maskTypes;
        List<DistortType> distortTypes;
        Component component;
        List<PropertyInfo> componentPropertyList;
        List<PropertyInfo> specificPropertyList;
        List<PropertyInfo> particlePropertyList;
        Action updateFunc;
        string editText;
        Popup popup;
        bool suppressComboBoxEvent;
        #endregion

        #region Public Members
        public event Action OnBeginEditing;
        public event Action OnEndEditing;
        #endregion

        #region Constructor
        public PropertyPanel(CommandStack commandStack, Config config, File file,
            List<ParticleType> types, List<MaskType> maskTypes, List<DistortType> distortTypes, Component component, Action updateFunc)
        {
            this.commandStack = commandStack;
            this.config = config;
            this.file = file;
            this.types = types ?? new List<ParticleType>();
            this.maskTypes = maskTypes ?? new List<MaskType>();
            this.distortTypes = distortTypes ?? new List<DistortType>();
            this.component = component;
            this.updateFunc = updateFunc;
            InitializeComponent();
            InitializeEnvironment();
            LoadContent();
            UpdateProperty();
        }
        #endregion

        #region Public Methods
        public void ScrollToEventSection()
        {
            ScrollToEventSectionCore();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(ScrollToEventSectionCore));
        }
        public void UpdateProperty()
        {
            UpdateProperty(component, ComponentGrid.DataContext as IList<PropertyGridItem>);
            UpdateProperty(component, SpecificGrid.DataContext as IList<PropertyGridItem>);
            if (component is Emitter)
            {
                UpdateProperty((component as Emitter).InitialTemplate, ParticleGrid.DataContext as IList<PropertyGridItem>);
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
            this.types = types ?? new List<ParticleType>();
            var emitter = component as Emitter;
            if (emitter == null) return;

            var currentType = emitter.InitialTemplate.Type;
            if (currentType != null && !this.types.Contains(currentType))
            {
                emitter.InitialTemplate.Type = null;
            }
            RefreshParticlePseudoProperties();
        }
        public void LoadMaskTypes(List<MaskType> maskTypes)
        {
            this.maskTypes = maskTypes ?? new List<MaskType>();
            var eventField = component as EventField;
            if (eventField != null && eventField.MaskType != null && !this.maskTypes.Contains(eventField.MaskType))
            {
                eventField.MaskType = null;
            }

            var emitter = component as Emitter;
            if (emitter != null && emitter.InitialTemplate.MaskType != null && !this.maskTypes.Contains(emitter.InitialTemplate.MaskType))
            {
                emitter.InitialTemplate.MaskType = null;
            }
            RefreshEventFieldMaskPseudoProperty();
            RefreshEmitterMaskPseudoProperty();
        }
        public void LoadDistortTypes(List<DistortType> distortTypes)
        {
            this.distortTypes = distortTypes ?? new List<DistortType>();
            var emitter = component as Emitter;
            if (emitter != null && emitter.InitialTemplate.DistortType != null &&
                !this.distortTypes.Contains(emitter.InitialTemplate.DistortType))
            {
                emitter.InitialTemplate.DistortType = null;
            }
            RefreshEmitterDistortPseudoProperty();
        }
        #endregion

        #region Private Methods
        void ScrollToEventSectionCore()
        {
            UpdateLayout();

            var parent = Parent as DependencyObject;
            while (parent != null)
            {
                var scroll = parent as ScrollViewer;
                if (scroll != null)
                {
                    scroll.UpdateLayout();
                    scroll.ScrollToBottom();
                    return;
                }
                parent = VisualTreeHelper.GetParent(parent) ?? LogicalTreeHelper.GetParent(parent);
            }
        }
        void InitializeEnvironment()
        {
            environment = new Expression.Environment();
            foreach (VariableResource item in file.Globals) environment.PutGlobal(item.Label, item.Value);
            foreach (VariableResource item in component.Locals) environment.PutLocal(item.Label, item.Value);
        }
        void LoadContent()
        {
            componentPropertyList = component.InitializeAndGetProperties(typeof(Component));
            LoadProperties(ComponentGrid, component, componentPropertyList);
            if (component is Emitter) specificPropertyList = component.InitializeAndGetProperties(typeof(Emitter));
            else specificPropertyList = component.InitializeAndGetProperties(component.GetType());
            LoadProperties(SpecificGrid, component, specificPropertyList);
            SpecificPropertyGroup.Visibility = specificPropertyList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            TextBindingHint.Visibility = component is CrazyStorm.Core.Text ? Visibility.Visible : Visibility.Collapsed;
            if (component is Emitter)
            {
                var particle = (component as Emitter).InitialTemplate;
                ParticlePropertyGroup.Visibility = Visibility.Visible;
                particlePropertyList = particle.InitializeAndGetProperties(typeof(ParticleBase));
                if (component is MultiEmitter) particlePropertyList.AddRange(particle.InitializeAndGetProperties(typeof(Particle)));
                else particlePropertyList.AddRange(particle.InitializeAndGetProperties(typeof(CurveParticle)));
                LoadProperties(ParticleGrid, particle, particlePropertyList);
            }
            else
            {
                ParticlePropertyGroup.Visibility = Visibility.Collapsed;
                if (component is EventField || component is Rebounder)
                {
                    var stub = new MultiEmitter();
                    particlePropertyList = stub.InitialTemplate.InitializeAndGetProperties(typeof(ParticleBase));
                    LoadProperties(ParticleGrid, stub.InitialTemplate, particlePropertyList);
                }
            }
            LoadTypes(types);
            LoadMaskTypes(maskTypes);
            LoadDistortTypes(distortTypes);
            VariableGrid.ItemsSource = component.Locals;
            DeleteVariable.IsEnabled = component.Locals.Count > 0;
            ComponentEventList.ItemsSource = component.ComponentEventGroups;
            DelComponentEventButton.IsEnabled = component.ComponentEventGroups.Count > 0;
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
            if (element == ParticleGrid && component is Emitter)
            {
                propertyItems.Add(CreateParticleTypePropertyItem(component as Emitter));
                propertyItems.Add(CreateParticleColorPropertyItem(component as Emitter));
            }
            if (element == SpecificGrid && component is Text)
            {
                propertyItems.Add(CreateFontFamilyPropertyItem(component as Text));
                propertyItems.Add(CreateFontFacePropertyItem(component as Text));
            }
            foreach (var item in infos)
            {
                var property = CreatePropertyGridItem(container, item);
                if (property != null) propertyItems.Add(property);
                environment.PutProperty(item.Name, item.GetGetMethod().Invoke(container, null));
            }
            element.DataContext = propertyItems;
        }
        void UpdateProperty(PropertyContainer container, IList<PropertyGridItem> properties)
        {
            if (properties == null) return;
            foreach (var item in properties)
            {
                if (item == null) continue;
                if (item.PseudoPropertyKind == PropertyPseudoKind.EventFieldMaskType)
                {
                    ApplyEventFieldMaskTypeValue(item, container as EventField);
                    continue;
                }
                if (item.PseudoPropertyKind == PropertyPseudoKind.EmitterMaskType)
                {
                    ApplyEmitterMaskTypeValue(item, container as ParticleBase);
                    continue;
                }
                if (item.PseudoPropertyKind == PropertyPseudoKind.EmitterDistortType)
                {
                    ApplyEmitterDistortTypeValue(item, container as ParticleBase);
                    continue;
                }
                if (item.PseudoPropertyKind != PropertyPseudoKind.None) continue;
                if (!item.ReadOnly && !container.Properties[item.Info.Name].Expression)
                {
                    var result = item.Info.GetGetMethod().Invoke(container, null).ToString();
                    container.Properties[item.Info.Name].Value = result;
                }
                ApplyReflectionPropertyValue(item, container.Properties[item.Info.Name].Value);
            }
        }
        PropertyGridItem CreatePropertyGridItem(PropertyContainer container, PropertyInfo info)
        {
            var attributes = info.GetCustomAttributes(false);
            if (attributes.OfType<RuntimePropertyAttribute>().Any()) return null;

            var propertyAttribute = GetEditablePropertyAttribute(info);
            var editorKind = GetEditorKind(propertyAttribute);
            var item = new PropertyGridItem()
            {
                Info = info,
                DisplayName = (string)FindResource(info.Name + "Str"),
                ReadOnly = attributes.OfType<ReadOnlyPropertyAttribute>().Any(),
                EditorKind = editorKind,
                ItemsSource = BuildItemsSource(editorKind, info.PropertyType),
            };
            ApplyReflectionPropertyValue(item, container.Properties[info.Name].Value);
            if (component is EventField && info.PropertyType == typeof(MaskType))
            {
                item.ReadOnly = false;
                item.EditorKind = PropertyEditorKind.EventFieldMaskTypeCombo;
                item.PseudoPropertyKind = PropertyPseudoKind.EventFieldMaskType;
                item.ItemsSource = BuildEventFieldMaskTypeItems();
                ApplyEventFieldMaskTypeValue(item, container as EventField);
            }
            else if (component is Emitter && container is ParticleBase && info.PropertyType == typeof(MaskType))
            {
                item.ReadOnly = false;
                item.EditorKind = PropertyEditorKind.EmitterMaskTypeCombo;
                item.PseudoPropertyKind = PropertyPseudoKind.EmitterMaskType;
                item.ItemsSource = BuildEmitterMaskTypeItems();
                ApplyEmitterMaskTypeValue(item, container as ParticleBase);
            }
            else if (component is Emitter && container is ParticleBase && info.PropertyType == typeof(DistortType))
            {
                item.ReadOnly = false;
                item.EditorKind = PropertyEditorKind.EmitterDistortTypeCombo;
                item.PseudoPropertyKind = PropertyPseudoKind.EmitterDistortType;
                item.ItemsSource = BuildEmitterDistortTypeItems();
                ApplyEmitterDistortTypeValue(item, container as ParticleBase);
            }
            return item;
        }
        void OpenEventSetting(EventGroup eventGroup, Expression.Environment environment, 
            bool emitter, bool aboutParticle, bool center)
        {
            file.UpdateResource();
            Window window = new EventSetting(eventGroup, environment, file.Sounds, types, emitter, aboutParticle, center);
            window.ShowDialog();
            window.Close();
        }
        void ShowIntellisense(PropertyGridItem property, UIElement element)
        {
            popup = new Popup();
            UIHelper.ShowIntellisense(popup, property.Info.PropertyType, element, UIHelper.BuildExpressionIntellisenseItems(environment));
        }
        void HideIntellisense()
        {
            UIHelper.HideIntellisense(popup);
        }
        PropertyEditorKind GetEditorKind(PropertyAttribute attribute)
        {
            if (attribute != null && attribute is EnumPropertyAttribute) return PropertyEditorKind.EnumCombo;
            if (attribute != null && attribute is BoolPropertyAttribute) return PropertyEditorKind.BoolCheckBox;
            return PropertyEditorKind.Text;
        }
        IList<string> BuildItemsSource(PropertyEditorKind editorKind, Type propertyType)
        {
            if (editorKind != PropertyEditorKind.EnumCombo || propertyType == null || !propertyType.IsEnum) return null;
            return Enum.GetNames(propertyType).Select(ExpressionHelper.Translate).ToList();
        }
        List<string> BuildEventFieldMaskTypeItems()
        {
            var items = new List<string> { string.Empty };
            items.AddRange(maskTypes.Select(item => item.Name));
            return items;
        }
        List<string> BuildEmitterMaskTypeItems()
        {
            var items = new List<string> { string.Empty };
            items.AddRange(maskTypes.Select(item => item.Name));
            return items;
        }
        List<string> BuildEmitterDistortTypeItems()
        {
            var items = new List<string> { string.Empty };
            items.AddRange(distortTypes.Select(item => item.Name));
            return items;
        }
        void ApplyReflectionPropertyValue(PropertyGridItem item, string internalValue)
        {
            if (item.EditorKind == PropertyEditorKind.BoolCheckBox)
            {
                bool parsed;
                bool.TryParse(internalValue, out parsed);
                item.BoolValue = parsed;
                item.DisplayValue = ExpressionHelper.Translate(parsed.ToString());
                return;
            }
            if (item.EditorKind == PropertyEditorKind.EnumCombo)
            {
                item.DisplayValue = ExpressionHelper.Translate(internalValue);
                return;
            }
            item.DisplayValue = item.Info != null && item.Info.PropertyType != typeof(string) && !item.ReadOnly ?
                ExpressionHelper.Translate(internalValue) : internalValue;
        }
        void ApplyEventFieldMaskTypeValue(PropertyGridItem item, EventField eventField)
        {
            if (item == null || eventField == null) return;
            item.DisplayValue = eventField.MaskType != null ? eventField.MaskType.Name : string.Empty;
        }
        void ApplyEmitterMaskTypeValue(PropertyGridItem item, ParticleBase particle)
        {
            if (item == null || particle == null) return;
            item.DisplayValue = particle.MaskType != null ? particle.MaskType.Name : string.Empty;
        }
        void ApplyEmitterDistortTypeValue(PropertyGridItem item, ParticleBase particle)
        {
            if (item == null || particle == null) return;
            item.DisplayValue = particle.DistortType != null ? particle.DistortType.Name : string.Empty;
        }
        PropertyGridItem CreateParticleTypePropertyItem(Emitter emitter)
        {
            var type = emitter.InitialTemplate.Type;
            return new PropertyGridItem()
            {
                DisplayName = (string)FindResource("TypeStr"),
                DisplayValue = type != null ? type.Name : string.Empty,
                EditorKind = PropertyEditorKind.ParticleTypeCombo,
                PseudoPropertyKind = PropertyPseudoKind.ParticleType,
                ItemsSource = GetDistinctParticleTypeNames(),
            };
        }
        PropertyGridItem CreateParticleColorPropertyItem(Emitter emitter)
        {
            var type = emitter.InitialTemplate.Type;
            var colorItems = type != null ? BuildParticleColorItems(type.Name) : new List<string>();
            return new PropertyGridItem()
            {
                DisplayName = (string)FindResource("RGBStr"),
                DisplayValue = type != null ? ExpressionHelper.Translate(type.Color.ToString()) : string.Empty,
                EditorKind = PropertyEditorKind.ParticleColorCombo,
                PseudoPropertyKind = PropertyPseudoKind.ParticleColor,
                ItemsSource = colorItems,
            };
        }
        List<string> GetDistinctParticleTypeNames()
        {
            return types.Select(item => item.Name).Distinct().ToList();
        }
        List<string> BuildParticleColorItems(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return new List<string>();
            return types.Where(item => item.Name == typeName)
                .Select(item => ExpressionHelper.Translate(item.Color.ToString()))
                .Distinct()
                .ToList();
        }
        PropertyGridItem CreateFontFamilyPropertyItem(Text text)
        {
            return new PropertyGridItem()
            {
                DisplayName = (string)FindResource("FontFamilyStr"),
                DisplayValue = FontHelper.GetFontFamilyLocale(text.FontFamily),
                EditorKind = PropertyEditorKind.FontFamilyCombo,
                PseudoPropertyKind = PropertyPseudoKind.FontFamily,
                ItemsSource = FontHelper.GetFontFamilysLocale(),
            };
        }
        PropertyGridItem CreateFontFacePropertyItem(Text text)
        {
            return new PropertyGridItem()
            {
                DisplayName = (string)FindResource("FontFaceStr"),
                DisplayValue = FontHelper.GetFontFaceLocale(text.FontFamily, text.FontFace),
                EditorKind = PropertyEditorKind.FontFaceCombo,
                PseudoPropertyKind = PropertyPseudoKind.FontFace,
                ItemsSource = FontHelper.GetFontFacesLocale(text.FontFamily),
            };
        }
        void RefreshParticlePseudoProperties()
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var items = ParticleGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var typeItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.ParticleType);
            var colorItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.ParticleColor);
            if (typeItem == null || colorItem == null) return;

            var currentType = emitter.InitialTemplate.Type;
            suppressComboBoxEvent = true;
            try
            {
                var typeNames = GetDistinctParticleTypeNames();
                typeItem.ItemsSource = typeNames;
                typeItem.DisplayValue = currentType != null ? currentType.Name : string.Empty;

                var colorItems = currentType != null ? BuildParticleColorItems(currentType.Name) : new List<string>();
                colorItem.ItemsSource = colorItems;
                colorItem.DisplayValue = currentType != null ? ExpressionHelper.Translate(currentType.Color.ToString()) : string.Empty;
            }
            finally
            {
                suppressComboBoxEvent = false;
            }
        }
        void RefreshEventFieldMaskPseudoProperty()
        {
            var eventField = component as EventField;
            if (eventField == null) return;

            var items = SpecificGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var maskTypeItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.EventFieldMaskType);
            if (maskTypeItem == null) return;

            suppressComboBoxEvent = true;
            try
            {
                maskTypeItem.ItemsSource = BuildEventFieldMaskTypeItems();
                ApplyEventFieldMaskTypeValue(maskTypeItem, eventField);
            }
            finally
            {
                suppressComboBoxEvent = false;
            }
        }
        void RefreshEmitterMaskPseudoProperty()
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var items = ParticleGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var maskTypeItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.EmitterMaskType);
            if (maskTypeItem == null) return;

            suppressComboBoxEvent = true;
            try
            {
                maskTypeItem.ItemsSource = BuildEmitterMaskTypeItems();
                ApplyEmitterMaskTypeValue(maskTypeItem, emitter.InitialTemplate);
            }
            finally
            {
                suppressComboBoxEvent = false;
            }
        }
        void RefreshEmitterDistortPseudoProperty()
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var items = ParticleGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var distortTypeItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.EmitterDistortType);
            if (distortTypeItem == null) return;

            suppressComboBoxEvent = true;
            try
            {
                distortTypeItem.ItemsSource = BuildEmitterDistortTypeItems();
                ApplyEmitterDistortTypeValue(distortTypeItem, emitter.InitialTemplate);
            }
            finally
            {
                suppressComboBoxEvent = false;
            }
        }
        void RefreshFontPseudoProperties()
        {
            var text = component as Text;
            if (text == null) return;

            var items = SpecificGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var familyItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.FontFamily);
            var faceItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.FontFace);
            if (faceItem == null) return;

            suppressComboBoxEvent = true;
            try
            {
                familyItem.DisplayValue = FontHelper.GetFontFamilyLocale(text.FontFamily);
                faceItem.ItemsSource = FontHelper.GetFontFacesLocale(text.FontFamily);
                faceItem.DisplayValue = FontHelper.GetFontFaceLocale(text.FontFamily, text.FontFace);
            }
            finally
            {
                suppressComboBoxEvent = false;
            }
        }
        void PreviewParticleTypeSelection(string typeName)
        {
            var items = ParticleGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var colorItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.ParticleColor);
            if (colorItem == null) return;

            var colorItems = BuildParticleColorItems(typeName);
            suppressComboBoxEvent = true;
            try
            {
                colorItem.ItemsSource = colorItems;
                colorItem.DisplayValue = colorItems.Count > 0 ? colorItems[0] : string.Empty;
            }
            finally
            {
                suppressComboBoxEvent = false;
            }
        }
        PropertyAttribute GetEditablePropertyAttribute(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(false).OfType<PropertyAttribute>()
                .FirstOrDefault(attr => !(attr is RuntimePropertyAttribute) && !(attr is ReadOnlyPropertyAttribute));
        }
        bool TryGetTargetContainer(DataGrid grid, out PropertyContainer container)
        {
            container = null;
            if (grid == ComponentGrid || grid == SpecificGrid)
            {
                container = component;
                return true;
            }
            if (grid == ParticleGrid && component is Emitter)
            {
                container = (component as Emitter).InitialTemplate;
                return true;
            }
            return false;
        }
        DataGridCell GetEditingCell(DataGridCellEditEndingEventArgs e)
        {
            var directCell = VisualHelper.FindParent<DataGridCell>(e.EditingElement);
            if (directCell != null) return directCell;

            var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
            return presenter != null ? presenter.ItemContainerGenerator.ContainerFromIndex(e.Column.DisplayIndex) as DataGridCell : null;
        }
        TextBox GetEditingTextBox(FrameworkElement element)
        {
            var textBox = element as TextBox;
            if (textBox != null) return textBox;

            var visual = element as Visual;
            return visual != null ? VisualHelper.GetVisualChild<TextBox>(visual) : null;
        }
        void CommitTextBoxValue(TextBox textBox)
        {
            var property = textBox != null ? textBox.DataContext as PropertyGridItem : null;
            if (textBox == null || property == null || property.EditorKind != PropertyEditorKind.Text || property.Info == null) return;

            var originalText = textBox.Tag as string ?? string.Empty;
            if (textBox.Text == originalText) return;

            var grid = VisualHelper.FindParent<DataGrid>(textBox);
            PropertyContainer container;
            if (grid == null || !TryGetTargetContainer(grid, out container)) return;

            var cell = VisualHelper.FindParent<DataGridCell>(textBox);
            if (cell == null) return;

            CommitReflectionProperty(container, property, cell, textBox.Text);
            textBox.Tag = property.DisplayValue;
        }
        void CommitReflectionProperty(PropertyContainer container, PropertyGridItem property, DataGridCell cell, string displayValue)
        {
            if (container == null || property == null || property.Info == null || cell == null) return;

            var attribute = GetEditablePropertyAttribute(property.Info);
            if (attribute == null) return;

            string newValue;
            if (property.EditorKind == PropertyEditorKind.EnumCombo)
                newValue = ExpressionHelper.ReverseTranslate(displayValue);
            else if (property.EditorKind == PropertyEditorKind.BoolCheckBox)
                newValue = property.BoolValue.ToString();
            else
                newValue = property.Info.PropertyType != typeof(string) ? ExpressionHelper.ReverseTranslate(displayValue) : displayValue;

            new SetPropertyCommand().Do(commandStack, environment, container, property, cell, newValue, attribute, updateFunc);
        }
        void CommitTextEdit(DataGridCellEditEndingEventArgs e)
        {
            var property = e.Row.Item as PropertyGridItem;
            if (property == null || property.EditorKind != PropertyEditorKind.Text || e.EditAction != DataGridEditAction.Commit) return;

            var textBox = GetEditingTextBox(e.EditingElement);
            if (textBox == null || textBox.Text == editText) return;

            var grid = VisualHelper.FindParent<DataGrid>(e.EditingElement);
            PropertyContainer container;
            if (grid == null || !TryGetTargetContainer(grid, out container)) return;
            CommitReflectionProperty(container, property, GetEditingCell(e), textBox.Text);
        }
        void CommitComboSelection(ComboBox comboBox)
        {
            var property = comboBox.DataContext as PropertyGridItem;
            if (property == null) return;

            var displayValue = comboBox.SelectedItem as string ?? string.Empty;
            if (property.EditorKind == PropertyEditorKind.FontFamilyCombo)
            {
                CommitFontFamilySelection(displayValue);
            }
            else if (property.EditorKind == PropertyEditorKind.FontFaceCombo)
            {
                CommitFontFaceSelection(displayValue);
            }
            else if (property.EditorKind == PropertyEditorKind.ParticleTypeCombo)
            {
                CommitParticleTypeSelection(displayValue);
            }
            else if (property.EditorKind == PropertyEditorKind.ParticleColorCombo)
            {
                CommitParticleColorSelection(displayValue);
            }
            else if (property.EditorKind == PropertyEditorKind.EventFieldMaskTypeCombo)
            {
                CommitEventFieldMaskTypeSelection(displayValue);
            }
            else if (property.EditorKind == PropertyEditorKind.EmitterMaskTypeCombo)
            {
                CommitEmitterMaskTypeSelection(displayValue);
            }
            else if (property.EditorKind == PropertyEditorKind.EmitterDistortTypeCombo)
            {
                CommitEmitterDistortTypeSelection(displayValue);
            }
            else if (displayValue != property.DisplayValue)
            {
                property.DisplayValue = displayValue;
                DataGrid grid;
                PropertyContainer container;
                if (TryGetDataGrid(comboBox, out grid) && TryGetTargetContainer(grid, out container))
                {
                    CommitReflectionProperty(container, property, VisualHelper.FindParent<DataGridCell>(comboBox), displayValue);
                }
            }
            CommitGridEditHost(comboBox);
        }
        void CommitParticleTypeSelection(string typeName)
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var currentType = emitter.InitialTemplate.Type;
            if (currentType != null && currentType.Name == typeName) return;

            var colorItems = BuildParticleColorItems(typeName);
            var selectedColor = colorItems.FirstOrDefault();
            var targetType = ResolveParticleType(typeName, selectedColor);
            if (currentType == targetType) return;

            new SetParticleTypeCommand().Do(commandStack, emitter, targetType,
                new Action<Emitter, ParticleType>(ParticleTypeUpdate));
        }
        void CommitParticleColorSelection(string colorDisplayValue)
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var items = ParticleGrid.DataContext as IList<PropertyGridItem>;
            if (items == null) return;

            var typeItem = items.FirstOrDefault(item => item.PseudoPropertyKind == PropertyPseudoKind.ParticleType);
            var targetType = ResolveParticleType(typeItem != null ? typeItem.DisplayValue : string.Empty, colorDisplayValue);
            if (emitter.InitialTemplate.Type == targetType) return;

            new SetParticleTypeCommand().Do(commandStack, emitter, targetType,
                new Action<Emitter, ParticleType>(ParticleTypeUpdate));
        }
        void CommitEventFieldMaskTypeSelection(string maskTypeName)
        {
            var eventField = component as EventField;
            if (eventField == null) return;

            var targetMaskType = maskTypes.FirstOrDefault(item => item.Name == maskTypeName);
            if (eventField.MaskType == targetMaskType) return;

            new SetEventFieldMaskTypeCommand().Do(commandStack, eventField, targetMaskType,
                new Action<EventField, MaskType>(EventFieldMaskTypeUpdate));
        }
        void CommitEmitterMaskTypeSelection(string maskTypeName)
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var targetMaskType = maskTypes.FirstOrDefault(item => item.Name == maskTypeName);
            if (emitter.InitialTemplate.MaskType == targetMaskType) return;

            new SetEmitterMaskTypeCommand().Do(commandStack, emitter, targetMaskType,
                new Action<Emitter, MaskType>(EmitterMaskTypeUpdate));
        }
        void CommitEmitterDistortTypeSelection(string distortTypeName)
        {
            var emitter = component as Emitter;
            if (emitter == null) return;

            var targetDistortType = distortTypes.FirstOrDefault(item => item.Name == distortTypeName);
            if (emitter.InitialTemplate.DistortType == targetDistortType) return;

            new SetEmitterDistortTypeCommand().Do(commandStack, emitter, targetDistortType,
                new Action<Emitter, DistortType>(EmitterDistortTypeUpdate));
        }
        void CommitFontFamilySelection(string fontFamilyLocale)
        {
            var text = component as Text;
            var fontFamily = FontHelper.GetFontFamilyByLocale(fontFamilyLocale);
            if (text == null || text.FontFamily == fontFamily) return;
            var fontFaces = FontHelper.GetFontFaces(fontFamily);
            var selectedFace = fontFaces.FirstOrDefault();
            new SetFontCommand().Do(commandStack, text, fontFamily, selectedFace, new Action<Text, string, string>(FontUpdate));
        }
        void CommitFontFaceSelection(string fontFaceLocale)
        {
            var text = component as Text;
            var fontFace = FontHelper.GetFontFaceByLocale(text.FontFamily, fontFaceLocale);
            if (text == null || text.FontFace == fontFace) return;
            new SetFontCommand().Do(commandStack, text, text.FontFamily, fontFace, new Action<Text, string, string>(FontUpdate));
        }
        ParticleType ResolveParticleType(string typeName, string colorDisplayValue)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            var colorName = !string.IsNullOrEmpty(colorDisplayValue) ? ExpressionHelper.ReverseTranslate(colorDisplayValue) : null;
            var selectedColor = default(ParticleColor);
            var hasColor = !string.IsNullOrEmpty(colorName) && Enum.TryParse(colorName, out selectedColor);
            foreach (var type in types)
            {
                if (type.Name != typeName) continue;
                if (!hasColor || type.Color == selectedColor) return type;
            }
            return null;
        }
        void ParticleTypeUpdate(Emitter emitter, ParticleType type)
        {
            if (this == null) return;
            emitter.InitialTemplate.Type = type;
            RefreshParticlePseudoProperties();
            if (updateFunc != null) updateFunc();
        }
        void FontUpdate(Text text, string family, string face)
        {
            if (this == null) return;
            text.FontFamily = family;
            text.FontFace = face;
            RefreshFontPseudoProperties();
            if (updateFunc != null) updateFunc();
        }
        void EventFieldMaskTypeUpdate(EventField eventField, MaskType maskType)
        {
            if (this == null) return;
            eventField.MaskType = maskType;
            RefreshEventFieldMaskPseudoProperty();
            if (updateFunc != null) updateFunc();
        }
        void EmitterMaskTypeUpdate(Emitter emitter, MaskType maskType)
        {
            if (this == null) return;
            emitter.InitialTemplate.MaskType = maskType;
            RefreshEmitterMaskPseudoProperty();
            if (updateFunc != null) updateFunc();
        }
        void EmitterDistortTypeUpdate(Emitter emitter, DistortType distortType)
        {
            if (this == null) return;
            emitter.InitialTemplate.DistortType = distortType;
            RefreshEmitterDistortPseudoProperty();
            if (updateFunc != null) updateFunc();
        }
        bool TryGetDataGrid(DependencyObject source, out DataGrid grid)
        {
            grid = VisualHelper.FindParent<DataGrid>(source);
            return grid != null;
        }
        void CommitGridEditHost(DependencyObject source)
        {
            DataGrid grid;
            if (!TryGetDataGrid(source, out grid)) return;
            grid.CommitEdit(DataGridEditingUnit.Cell, true);
            grid.CommitEdit(DataGridEditingUnit.Row, true);
        }
        void RefreshPropertyComboBoxToolTip(ComboBox comboBox)
        {
            var property = comboBox != null ? comboBox.DataContext as PropertyGridItem : null;
            if (comboBox == null || property == null ||
                property.PseudoPropertyKind != PropertyPseudoKind.ParticleType)
            {
                if (comboBox != null) comboBox.ToolTip = null;
                return;
            }

            var selectedTypeName = comboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedTypeName))
                selectedTypeName = property.DisplayValue;

            comboBox.ToolTip = BuildParticleTypeToolTip(ResolvePreviewParticleType(selectedTypeName));
        }
        ToolTip BuildParticleTypeToolTip(ParticleType selectedType)
        {
            if (selectedType == null) return null;

            var rectangle = new Rectangle() { Width = selectedType.Width, Height = selectedType.Height };
            var imageBrush = new ImageBrush();
            rectangle.Fill = imageBrush;
            if (selectedType.ID >= ParticleType.DefaultTypeIndex)
            {
                if (string.IsNullOrEmpty(config.TypeLibraryPath))
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/barrages.png", UriKind.Absolute));
                else
                    imageBrush.ImageSource = new BitmapImage(new Uri($"typelibrary\\{System.IO.Path.GetFileNameWithoutExtension(config.TypeLibraryPath)}.png", UriKind.Relative));
            }
            else if (selectedType.Image != null)
            {
                imageBrush.ImageSource = new BitmapImage(new Uri(selectedType.Image.AbsolutePath));
            }
            var bitmap = imageBrush.ImageSource as BitmapImage;
            if (bitmap != null)
            {
                imageBrush.Viewbox = new Rect(selectedType.StartPointX / bitmap.PixelWidth,
                    selectedType.StartPointY / bitmap.PixelHeight,
                    (float)selectedType.Width / bitmap.PixelWidth,
                    (float)selectedType.Height / bitmap.PixelHeight);
            }
            return new ToolTip() { Content = rectangle };
        }
        ParticleType ResolvePreviewParticleType(string typeName)
        {
            var emitter = component as Emitter;
            if (emitter != null && emitter.InitialTemplate.Type != null && emitter.InitialTemplate.Type.Name == typeName)
                return emitter.InitialTemplate.Type;
            return types.FirstOrDefault(item => item.Name == typeName);
        }
        #endregion

        #region Window EventHandlers
        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sv = VisualHelper.VisualUpwardSearch<ScrollViewer>(sender as DependencyObject) as ScrollViewer;
            if (sv == null) return;
            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        private void PropertyGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell ?? VisualHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell == null || cell.IsEditing || cell.Column == null || cell.Column.DisplayIndex != 1) return;

            var item = cell.DataContext as PropertyGridItem;
            if (item == null || item.ReadOnly || item.EditorKind != PropertyEditorKind.Text) return;

            var grid = VisualHelper.FindParent<DataGrid>(cell);
            if (grid == null) return;

            if (!cell.IsFocused) cell.Focus();
            var row = VisualHelper.FindParent<DataGridRow>(cell);
            if (row != null && !row.IsSelected) row.IsSelected = true;
            grid.CurrentCell = new DataGridCellInfo(cell);
            if (!cell.IsEditing)
            {
                grid.BeginEdit(e);
                e.Handled = true;
            }
        }
        private void PropertyTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null) textBox.Tag = textBox.Text;
        }
        private void PropertyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var property = textBox != null ? textBox.DataContext as PropertyGridItem : null;
            if (textBox == null || property == null || property.EditorKind != PropertyEditorKind.Text || property.Info == null) return;

            editText = textBox.Text;
            textBox.Tag = textBox.Text;
            if (OnBeginEditing != null) OnBeginEditing();
            ShowIntellisense(property, textBox);
        }
        private void PropertyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            CommitTextBoxValue(textBox);
            HideIntellisense();
            if (OnEndEditing != null) OnEndEditing();
        }
        private void PropertyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var textBox = sender as TextBox;
            CommitTextBoxValue(textBox);
            HideIntellisense();
            if (OnEndEditing != null) OnEndEditing();
            e.Handled = true;
        }
        private void Grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var item = e.Row.Item as PropertyGridItem;
            if (item != null && item.ReadOnly)
            {
                e.Cancel = true;
                return;
            }
            editText = item != null ? item.DisplayValue : string.Empty;
            if (OnBeginEditing != null)
                OnBeginEditing();
        }
        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (OnEndEditing != null)
                OnEndEditing();
        }
        private void ComponentGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Grid_BeginningEdit(sender, e);
        }
        private void ComponentGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Grid_CellEditEnding(sender, e);
            CommitTextEdit(e);
            HideIntellisense();
        }
        private void ParticleGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Grid_BeginningEdit(sender, e);
        }
        private void PropertyGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var property = e.Row.Item as PropertyGridItem;
            if (property == null || property.EditorKind != PropertyEditorKind.Text || property.Info == null)
            {
                HideIntellisense();
                return;
            }

            var editor = GetEditingTextBox(e.EditingElement);
            if (editor != null)
            {
                ShowIntellisense(property, editor);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    editor.Focus();
                    editor.SelectAll();
                }), DispatcherPriority.Input);
            }
        }
        private void ParticleGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Grid_CellEditEnding(sender, e);
            CommitTextEdit(e);
            HideIntellisense();
        }
        private void PropertyComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.Tag = false;
                RefreshPropertyComboBoxToolTip(comboBox);
            }
        }
        private void PropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressComboBoxEvent) return;

            var comboBox = sender as ComboBox;
            var property = comboBox != null ? comboBox.DataContext as PropertyGridItem : null;
            if (comboBox == null || property == null) return;

            RefreshPropertyComboBoxToolTip(comboBox);

            if (property.EditorKind == PropertyEditorKind.ParticleTypeCombo && comboBox.IsDropDownOpen)
            {
                PreviewParticleTypeSelection(comboBox.SelectedItem as string);
            }
            else if (!comboBox.IsDropDownOpen && comboBox.IsKeyboardFocusWithin)
            {
                CommitComboSelection(comboBox);
            }
        }
        private void PropertyComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || e.Key != Key.Enter) return;

            if (!comboBox.IsDropDownOpen)
            {
                comboBox.IsDropDownOpen = true;
            }
            else
            {
                comboBox.Tag = true;
                CommitComboSelection(comboBox);
                comboBox.IsDropDownOpen = false;
            }
            e.Handled = true;
        }
        private void PropertyComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            RefreshPropertyComboBoxToolTip(comboBox);

            if (comboBox.Tag is bool && (bool)comboBox.Tag)
            {
                comboBox.Tag = false;
                return;
            }
            CommitComboSelection(comboBox);
        }
        private void PropertyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var property = checkBox != null ? checkBox.DataContext as PropertyGridItem : null;
            if (checkBox == null || property == null) return;

            var newValue = checkBox.IsChecked == true;
            if (property.BoolValue != newValue)
            {
                property.BoolValue = newValue;
                property.DisplayValue = ExpressionHelper.Translate(newValue.ToString());

                DataGrid grid;
                PropertyContainer container;
                if (TryGetDataGrid(checkBox, out grid) && TryGetTargetContainer(grid, out container))
                {
                    CommitReflectionProperty(container, property, VisualHelper.FindParent<DataGridCell>(checkBox), property.DisplayValue);
                }
            }
            CommitGridEditHost(checkBox);
        }
        private void PropertyCheckBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space) return;

            var checkBox = sender as CheckBox;
            if (checkBox == null) return;

            checkBox.IsChecked = !(checkBox.IsChecked == true);
            PropertyCheckBox_Click(checkBox, e);
            e.Handled = true;
        }
        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            var label = (string)FindResource("LocalStr");
            for (int i = 0;; ++i)
            {
                //To avoid repeating name, use number.
                var name = label + i;
                bool ok = true;
                for (int k = 0; k < component.Locals.Count; ++k)
                {
                    if (component.Locals[k].Label == name)
                    {
                        ok = false;
                        break;
                    }
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
                    {
                        if (item != editItem && item.Label == newValue)
                        {
                            MessageBox.Show((string)FindResource("NameRepeatingStr"), (string)FindResource("TipTitleStr"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            e.Cancel = true;
                            (e.EditingElement as TextBox).Text = editItem.Label;
                            return;
                        }
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
