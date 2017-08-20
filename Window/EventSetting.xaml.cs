/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Text.RegularExpressions;
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
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using CrazyStorm.Core;
using CrazyStorm.Expression;

namespace CrazyStorm
{
    public partial class EventSetting : Window
    {
        #region Private Members
        EventGroup eventGroup;
        Expression.Environment environment;
        IList<FileResource> sounds;
        IList<ParticleType> types;
        bool isPlaySound;
        bool isEditing;
        bool isExpressionResult;
        DockPanel editingPanel;
        Popup popup;
        IOrderedEnumerable<VariableComboBoxItem> sortedVaraibles;
        #endregion

        #region Constructor
        public EventSetting(EventGroup eventGroup, Expression.Environment environment,
            IList<FileResource> sounds, IList<ParticleType> types, bool emitter, bool aboutParticle)
        {
            this.eventGroup = eventGroup;
            this.environment = environment;
            this.sounds = sounds;
            this.types = types;
            InitializeComponent();
            InitializeSetting(emitter, aboutParticle);
            LoadContent();
            TranslateEvents();
            ResetAll();
        }
        #endregion

        #region Private Methods
        void InitializeSetting(bool emitter, bool aboutParticle)
        {
            GroupBox.DataContext = eventGroup;
            EventList.ItemsSource = eventGroup.TranslatedEvents;
            EmitParticle.Visibility = emitter ? Visibility.Visible : Visibility.Collapsed;
            ChangeType.Visibility = aboutParticle ? Visibility.Visible : Visibility.Collapsed;
        }
        void LoadContent()
        {
            //Load properties.
            foreach (var property in environment.Properties)
            {
                var item = new VariableComboBoxItem();
                item.Name = property.Key;
                var displayName = TranslateProperty(property.Key);
                item.DisplayName = displayName != null ? displayName : property.Key;
                LeftConditionComboBox.Items.Add(item);
                RightConditionComboBox.Items.Add(item);
                PropertyComboBox.Items.Add(item);
            }
            //Load locals.
            foreach (var local in environment.Locals)
            {
                var item = new VariableComboBoxItem();
                item.Name = local.Key;
                item.DisplayName = item.Name;
                LeftConditionComboBox.Items.Add(item);
                RightConditionComboBox.Items.Add(item);
                PropertyComboBox.Items.Add(item);
            }
            //Load globals.
            foreach (var global in environment.Globals)
            {
                var item = new VariableComboBoxItem();
                item.Name = global.Key;
                item.DisplayName = item.Name;
                LeftConditionComboBox.Items.Add(item);
                RightConditionComboBox.Items.Add(item);
                PropertyComboBox.Items.Add(item);
            }
            //Load sounds.
            foreach (FileResource sound in sounds)
            {
                if (sound.IsValid)
                    SoundCombo.Items.Add(sound);
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
            //Create sorted variable list for translating event
            var varaibleList = new List<VariableComboBoxItem>();
            foreach (VariableComboBoxItem item in PropertyComboBox.Items)
                varaibleList.Add(item);
            //Longer name first
            sortedVaraibles = varaibleList.OrderByDescending(s => s.Name.Length);
        }
        string TranslateProperty(string properyName)
        {
            string[] split = properyName.Split('.');
            var displayName = (string)TryFindResource(split[0] + "Str");
            if (displayName != null && split.Length > 1)
                displayName += "." + split[1];
            else if (displayName == null)
                displayName = split[0];

            return displayName;
        }
        string TranslateEvent(string originalEvent)
        {
            var info = EventHelper.SplitEvent(originalEvent);
            if (info.leftProperty != null)
                info.leftProperty = TranslateProperty(info.leftProperty);

            if (info.rightProperty != null)
                info.rightProperty = TranslateProperty(info.rightProperty);

            if (!info.isSpecialEvent)
            {
                info.resultProperty = TranslateProperty(info.resultProperty);
                string[] keywords = {"Linear", "Accelerated", "Decelerated", "Instant", "ChangeTo", "Increase", "Decrease"};
                foreach (string item in keywords)
                {
                    if (info.changeType == item)
                        info.changeType = (string)FindResource(item + "Str");

                    if (info.changeMode == item)
                        info.changeMode = (string)FindResource(item + "Str");
                }
            }
            else
            {
                string[] keywords = {"EmitParticle", "PlaySound", "Loop", "ChangeType"};
                foreach (string item in keywords)
                {
                    if (info.specialEvent == item)
                        info.specialEvent = (string)FindResource(item + "Str");
                }
            }
            return EventHelper.BuildEvent(info, false);
        }
        void TranslateEvents()
        {
            if (eventGroup.TranslatedEvents.Count != 0)
                return;

            foreach (string originalEvent in eventGroup.OriginalEvents)
                eventGroup.TranslatedEvents.Add(TranslateEvent(originalEvent));
        }
        void ChangeTextBoxState(TextBox source, ExpressionException error)
        {
            if (error != null)
            {
                var tip = new ToolTip();
                var tipText = new TextBlock();
                tipText.Text = (string)FindResource(error.Message + "Str");
                tip.Content = tipText;
                source.ToolTip = tip;
                source.Background = new SolidColorBrush(Color.FromRgb(255, 190, 190));
            }
            else
            {
                source.ToolTip = null;
                source.Background = new SolidColorBrush(Colors.White);
            }
        }
        bool SetConditionInfo(EventInfo eventInfo)
        {
            //Check if there have errors
            if (LeftValue.ToolTip != null || RightValue.ToolTip != null)
                return false;

            if (LeftLessThan.IsChecked == true)
                eventInfo.leftOperator = "<";
            else if (LeftEqual.IsChecked == true)
                eventInfo.leftOperator = "=";
            else if (LeftMoreThan.IsChecked == true)
                eventInfo.leftOperator = ">";

            if (LeftConditionComboBox.SelectedItem != null && !String.IsNullOrEmpty(LeftValue.Text) &&
                !String.IsNullOrEmpty(eventInfo.leftOperator))
            {
                var selectedItem = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                eventInfo.leftProperty = selectedItem.Name;
                eventInfo.leftType = GetValueType(selectedItem.Name);
                eventInfo.leftValue = LeftValue.Text;
            }
            if (RightLessThan.IsChecked == true)
                eventInfo.rightOperator = "<";
            else if (RightEqual.IsChecked == true)
                eventInfo.rightOperator = "=";
            else if (RightMoreThan.IsChecked == true)
                eventInfo.rightOperator = ">";

            if (RightConditionComboBox.SelectedItem != null && !String.IsNullOrEmpty(RightValue.Text) &&
                !String.IsNullOrEmpty(eventInfo.rightOperator))
            {
                var selectedItem = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                eventInfo.rightProperty = selectedItem.Name;
                eventInfo.rightType = GetValueType(selectedItem.Name);
                eventInfo.rightValue = RightValue.Text;
            }
            //Allow empty condition
            if (String.IsNullOrEmpty(eventInfo.leftProperty) && String.IsNullOrEmpty(eventInfo.rightProperty))
                return true;

            if (String.IsNullOrEmpty(eventInfo.leftProperty) && !String.IsNullOrEmpty(eventInfo.rightProperty))
            {
                eventInfo.leftProperty = eventInfo.rightProperty;
                eventInfo.rightProperty = null;
                eventInfo.leftOperator = eventInfo.rightOperator;
                eventInfo.rightOperator = null;
                eventInfo.leftType = eventInfo.rightType;
                eventInfo.rightType = PropertyType.IllegalType;
                eventInfo.leftValue = eventInfo.rightValue;
                eventInfo.rightValue = null;

            }
            if (!String.IsNullOrEmpty(eventInfo.leftProperty) && !String.IsNullOrEmpty(eventInfo.rightProperty))
            {
                if (And.IsChecked == true)
                    eventInfo.midOperator = "&";
                else if (Or.IsChecked == true)
                    eventInfo.midOperator = "|";
            }
            eventInfo.hasCondition = true;
            return true;
        }
        bool BuildEvent(out string text)
        {
            text = string.Empty;
            var eventInfo = new EventInfo();
            //Check if there have errors
            if (ResultValue.ToolTip != null || ChangeTime.ToolTip != null)
                return false;

            if (!SetConditionInfo(eventInfo))
                return false;

            if (ChangeTo.IsChecked == true)
                eventInfo.changeType = "ChangeTo";
            else if (Increase.IsChecked == true)
                eventInfo.changeType = "Increase";
            else if (Decrease.IsChecked == true)
                eventInfo.changeType = "Decrease";

            if (Linear.IsChecked == true)
                eventInfo.changeMode = "Linear";
            else if (Accelerated.IsChecked == true)
                eventInfo.changeMode = "Accelerated";
            else if (Decelerated.IsChecked == true)
                eventInfo.changeMode = "Decelerated";
            else if (Instant.IsChecked == true)
                eventInfo.changeMode = "Instant";

            if (PropertyComboBox.SelectedItem != null && !String.IsNullOrEmpty(ResultValue.Text) &&
                !String.IsNullOrEmpty(eventInfo.changeType) && !String.IsNullOrEmpty(eventInfo.changeMode) && 
                (!String.IsNullOrEmpty(ChangeTime.Text) || Instant.IsChecked == true))
            {
                var selectedItem = PropertyComboBox.SelectedItem as VariableComboBoxItem;
                eventInfo.resultProperty = selectedItem.Name;
                eventInfo.isExpressionResult = isExpressionResult;
                eventInfo.resultType = GetValueType(selectedItem.Name);
                eventInfo.resultValue = ResultValue.Text;
                if (Instant.IsChecked == true)
                    eventInfo.changeTime = "1";
                else
                    eventInfo.changeTime = ChangeTime.Text;
            }
            else
                return false;

            text = EventHelper.BuildEvent(eventInfo, true);
            return true;
        }
        PropertyType GetValueType(string name)
        {
            object value = environment.GetProperty(name);
            if (value == null)
                value = environment.GetLocal(name);

            if (value == null)
                value = environment.GetGlobal(name);

            return PropertyTypeRule.GetValueType(value);
        }
        bool BuildSpecialEvent(out string text)
        {
            text = string.Empty;
            var eventInfo = new EventInfo();
            if (!SetConditionInfo(eventInfo))
                return false;

            if (EmitParticle.IsChecked == true)
            {
                eventInfo.specialEvent = "EmitParticle";
                eventInfo.arguments = string.Empty;
            }
            else if (PlaySound.IsChecked == true)
            {
                if (SoundCombo.SelectedItem == null)
                    return false;

                eventInfo.specialEvent = "PlaySound";
                eventInfo.arguments = SoundCombo.SelectedItem + ", " + VolumeSlider.Value;
            }
            else if (Loop.IsChecked == true)
            {
                //Check if there have errors
                if (StopCondition.ToolTip != null || String.IsNullOrEmpty(StopCondition.Text))
                    return false;

                string arguments = string.Empty;
                arguments = StopCondition.Text;
                eventInfo.specialEvent = "Loop";
                eventInfo.arguments = arguments;
            }
            else if (ChangeType.IsChecked == true)
            {
                if (TypeCombo.SelectedItem == null || ColorCombo.SelectedItem == null)
                    return false;

                eventInfo.specialEvent = "ChangeType";
                eventInfo.arguments = (TypeCombo.SelectedItem as ParticleType).ID + "," + ColorCombo.SelectedIndex;
            }
            else
                return false;

            eventInfo.isSpecialEvent = true;
            text = EventHelper.BuildEvent(eventInfo, true);
            return true;
        }
        void ResetAll()
        {
            LeftConditionComboBox.SelectedIndex = -1;
            LeftMoreThan.IsChecked = false;
            LeftEqual.IsChecked = true;
            LeftLessThan.IsChecked = false;
            LeftValue.Text = string.Empty;
            And.IsChecked = true;
            Or.IsChecked = false;
            RightConditionComboBox.SelectedIndex = -1;
            RightMoreThan.IsChecked = false;
            RightEqual.IsChecked = true;
            RightLessThan.IsChecked = false;
            RightValue.Text = string.Empty;
            PropertyComboBox.SelectedIndex = -1;
            ChangeTo.IsChecked = true;
            Increase.IsChecked = false;
            Decrease.IsChecked = false;
            ResultValue.Text = string.Empty;
            Linear.IsChecked = true;
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTime.Text = string.Empty;
            EmitParticle.IsChecked = false;
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            PlaySound.IsChecked = false;
            SoundCombo.SelectedIndex = -1;
            isPlaySound = false;
            SoundTestButton.Content = (string)FindResource("TestStr");
            VolumeSlider.Value = 50;
            LoopPanel.Visibility = Visibility.Collapsed;
            Loop.IsChecked = false;
            StopCondition.Text = string.Empty;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
            ChangeType.IsChecked = false;
            TypeCombo.SelectedIndex = -1;
            ColorCombo.SelectedIndex = -1;
        }
        void MapEventText(string text)
        {
            ResetAll();
            Dictionary<string, RadioButton> buttonMap = new Dictionary<string, RadioButton>();
            buttonMap[">"] = LeftMoreThan;
            buttonMap["="] = LeftEqual;
            buttonMap["<"] = LeftLessThan;
            buttonMap["&"] = And;
            buttonMap["|"] = Or;
            buttonMap["ChangeTo"] = ChangeTo;
            buttonMap["Increase"] = Increase;
            buttonMap["Decrease"] = Decrease;
            buttonMap["Linear"] = Linear;
            buttonMap["Accelerated"] = Accelerated;
            buttonMap["Decelerated"] = Decelerated;
            buttonMap["Instant"] = Instant;
            buttonMap["EmitParticle"] = EmitParticle;
            buttonMap["PlaySound"] = PlaySound;
            buttonMap["Loop"] = Loop;
            buttonMap["ChangeType"] = ChangeType;
            EventInfo eventInfo = EventHelper.SplitEvent(text);
            //Backfill condition
            if (eventInfo.hasCondition)
            {
                if (eventInfo.rightProperty != null)
                {
                    for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                    {
                        var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                        if (item.Name == eventInfo.leftProperty)
                            LeftConditionComboBox.SelectedIndex = i;
                    }
                    buttonMap[eventInfo.leftOperator].IsChecked = true;
                    LeftValue.Text = eventInfo.leftValue;
                    buttonMap[eventInfo.midOperator].IsChecked = true;
                    for (int i = 0; i < RightConditionComboBox.Items.Count; ++i)
                    {
                        var item = RightConditionComboBox.Items[i] as VariableComboBoxItem;
                        if (item.Name == eventInfo.rightProperty)
                            RightConditionComboBox.SelectedIndex = i;
                    }
                    buttonMap[">"] = RightMoreThan;
                    buttonMap["="] = RightEqual;
                    buttonMap["<"] = RightLessThan;
                    buttonMap[eventInfo.rightOperator].IsChecked = true;
                    RightValue.Text = eventInfo.rightValue;
                }
                else
                {
                    for (int i = 0; i < LeftConditionComboBox.Items.Count; ++i)
                    {
                        var item = LeftConditionComboBox.Items[i] as VariableComboBoxItem;
                        if (item.Name == eventInfo.leftProperty)
                            LeftConditionComboBox.SelectedIndex = i;
                    }
                    buttonMap[eventInfo.leftOperator].IsChecked = true;
                    LeftValue.Text = eventInfo.leftValue;
                }
            }
            //Backfill event
            if (!eventInfo.isSpecialEvent)
            {
                for (int i = 0; i < PropertyComboBox.Items.Count; ++i)
                {
                    var item = PropertyComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == eventInfo.resultProperty)
                        PropertyComboBox.SelectedIndex = i;
                }
                isExpressionResult = eventInfo.isExpressionResult;
                buttonMap[eventInfo.changeType].IsChecked = true;
                ResultValue.Text = eventInfo.resultValue;
                buttonMap[eventInfo.changeMode].IsChecked = true;
                ChangeTime.Text = eventInfo.changeTime;
            }
            else
            {
                buttonMap[eventInfo.specialEvent].IsChecked = true;
                string[] split = eventInfo.arguments.Split(',');
                if (eventInfo.specialEvent == "PlaySound")
                {
                    for (int i = 0; i < SoundCombo.Items.Count; ++i)
                    {
                        if ((SoundCombo.Items[i] as FileResource).Label == split[0])
                        {
                            SoundCombo.SelectedIndex = i;
                            break;
                        }
                    }
                    VolumeSlider.Value = int.Parse(split[1]);
                }
                else if (eventInfo.specialEvent == "Loop")
                {
                    StopCondition.Text = split[0].Trim();
                }
                else if (eventInfo.specialEvent == "ChangeType")
                {
                    for (int i = 0; i < TypeCombo.Items.Count; ++i)
                    {
                        if ((TypeCombo.Items[i] as ParticleType).ID == int.Parse(split[0]))
                        {
                            TypeCombo.SelectedIndex = i;
                            break;
                        }
                    }
                    if (TypeCombo.SelectedItem != null)
                        ColorCombo.SelectedIndex = int.Parse(split[1].Trim());
                }
            }
        }
        void EditEvent(DockPanel editingPanel)
        {
            this.editingPanel = editingPanel;
            editingPanel.Background = SystemColors.HighlightBrush;
            EventList.IsEnabled = false;
            AddEvent.Content = (string)FindResource("ModifyStr");
            AddSpecialEvent.Content = AddEvent.Content;
            isEditing = true;
        }
        void DeleteEvent()
        {
            var item = EventList.SelectedItem;
            if (item != null)
            {
                eventGroup.OriginalEvents.RemoveAt(EventList.SelectedIndex);
                eventGroup.TranslatedEvents.RemoveAt(EventList.SelectedIndex);
            }
            else if (eventGroup.TranslatedEvents.Count > 0)
            {
                eventGroup.OriginalEvents.RemoveAt(0);
                eventGroup.TranslatedEvents.RemoveAt(0);
            }
            EventList.ItemsSource = eventGroup.TranslatedEvents;
        }
        void ShowIntellisense(object propertyName, UIElement element)
        {
            popup = new Popup();
            popup.PlacementTarget = element;
            popup.Placement = PlacementMode.Bottom;
            popup.PopupAnimation = PopupAnimation.Fade;
            var listView = new ListView();
            if (propertyName is bool)
            {
                listView.Items.Add(true.ToString());
                listView.Items.Add(false.ToString());
            }
            else if (propertyName is Enum)
            {
                Array array = Enum.GetValues(propertyName.GetType());
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

                    var textBox = element as TextBox;
                    textBox.Text = (args.OriginalSource as TextBlock).Text;
                };
                popup.Child = listView;
                popup.IsOpen = true;
            }
        }
        void HideIntellisense()
        {
            if (popup != null)
            {
                popup.Child = null;
                popup.IsOpen = false;
            }
        }
        #endregion

        #region Window EventHandlers
        private void Linear_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
        }
        private void Accelerated_Checked(object sender, RoutedEventArgs e)
        {
            Linear.IsChecked = false;
            Decelerated.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
        }
        private void Decelerated_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Linear.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
        }
        private void Instant_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Linear.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Collapsed;
            ChangeTime.Text = string.Empty;
        }
        private void EmitParticleButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
        }
        private void PlaySoundButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Visible;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
        }
        private void LoopButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Visible;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
        }
        private void ChangeTypeButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Visible;
        }
        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (isEditing)
            {
                if (BuildEvent(out text))
                {
                    eventGroup.OriginalEvents[EventList.SelectedIndex] = text;
                    eventGroup.TranslatedEvents[EventList.SelectedIndex] = TranslateEvent(text);
                    editingPanel.Background = null;
                    EventList.IsEnabled = true;
                    AddEvent.Content = (string)FindResource("AddStr");
                    PropertyEventPanel.IsEnabled = true;
                    AddSpecialEvent.Content = AddEvent.Content;
                    SpecialEventPanel.IsEnabled = true;
                    isEditing = false;
                }
            }
            else if (BuildEvent(out text))
            {
                if (EventList.SelectedIndex != -1 && EventList.Items.Count - 1 > EventList.SelectedIndex)
                {
                    eventGroup.OriginalEvents.Insert(EventList.SelectedIndex + 1, text);
                    eventGroup.TranslatedEvents.Insert(EventList.SelectedIndex + 1, TranslateEvent(text));
                }
                else
                {
                    eventGroup.OriginalEvents.Add(text);
                    eventGroup.TranslatedEvents.Add(TranslateEvent(text));
                }
            }
        }
        private void AddSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (isEditing)
            {
                if (BuildSpecialEvent(out text))
                {
                    eventGroup.OriginalEvents[EventList.SelectedIndex] = text;
                    eventGroup.TranslatedEvents[EventList.SelectedIndex] = TranslateEvent(text);
                    editingPanel.Background = null;
                    EventList.IsEnabled = true;
                    AddEvent.Content = (string)FindResource("AddStr");
                    PropertyEventPanel.IsEnabled = true;
                    AddSpecialEvent.Content = AddEvent.Content;
                    SpecialEventPanel.IsEnabled = true;
                    isEditing = false;
                }
            }
            else if (BuildSpecialEvent(out text))
            {
                if (EventList.SelectedIndex != -1 && EventList.Items.Count - 1 > EventList.SelectedIndex)
                {
                    eventGroup.OriginalEvents.Insert(EventList.SelectedIndex + 1, text);
                    eventGroup.TranslatedEvents.Insert(EventList.SelectedIndex + 1, TranslateEvent(text));
                }
                else
                {
                    eventGroup.OriginalEvents.Add(text);
                    eventGroup.TranslatedEvents.Add(TranslateEvent(text));
                }
            }
        }
        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            EditEvent((((e.OriginalSource as FrameworkElement).Parent as ContextMenu).PlacementTarget) as DockPanel);
        }
        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            DeleteEvent();
        }
        private void LeftConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftValue_PreviewLostKeyboardFocus(sender, null);
        }
        private void RightConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightValue_PreviewLostKeyboardFocus(sender, null);
        }
        private void PropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultValue_PreviewLostKeyboardFocus(sender, null);
        }
        private void LeftValue_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (LeftConditionComboBox.SelectedItem != null)
            {
                var item = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = environment.GetProperty(item.Name);
                ShowIntellisense(value, LeftValue);
            }
        }
        private void LeftValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HideIntellisense();
            ChangeTextBoxState(LeftValue, null);
            LeftValue.Text = LeftValue.Text.Trim();
            string input = LeftValue.Text;
            if (String.IsNullOrEmpty(input))
                return;

            if (LeftConditionComboBox.SelectedItem != null)
            {
                var item = LeftConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = environment.GetProperty(item.Name);
                if (value != null)
                {
                    if (!PropertyTypeRule.TryParse(value, input, out value))
                    {
                        ChangeTextBoxState(LeftValue, new ExpressionException("TypeError"));
                        return;
                    }
                    LeftValue.Text = value.ToString();
                    return;
                }
                if (value == null)
                {
                    value = environment.GetLocal(item.Name);
                }
                if (value == null)
                {
                    value = environment.GetGlobal(item.Name);
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        ChangeTextBoxState(LeftValue, new ExpressionException("TypeError"));
                        return;
                    }
                }
            }
        }
        private void RightValue_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (RightConditionComboBox.SelectedItem != null)
            {
                var item = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = environment.GetProperty(item.Name);
                ShowIntellisense(value, RightValue);
            }
        }
        private void RightValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HideIntellisense();
            ChangeTextBoxState(RightValue, null);
            RightValue.Text = RightValue.Text.Trim();
            string input = RightValue.Text;
            if (String.IsNullOrEmpty(input))
                return;

            if (RightConditionComboBox.SelectedItem != null)
            {
                var item = RightConditionComboBox.SelectedItem as VariableComboBoxItem;
                object value = environment.GetProperty(item.Name);
                if (value != null)
                {
                    if (!PropertyTypeRule.TryParse(value, input, out value))
                    {
                        ChangeTextBoxState(RightValue, new ExpressionException("TypeError"));
                        return;
                    }
                    RightValue.Text = value.ToString();
                    return;
                }
                if (value == null)
                {
                    value = environment.GetLocal(item.Name);
                }
                if (value == null)
                {
                    value = environment.GetGlobal(item.Name);
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        ChangeTextBoxState(RightValue, new ExpressionException("TypeError"));
                        return;
                    }
                }
            }
        }
        private void ResultValue_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PropertyComboBox.SelectedItem != null)
            {
                var item = PropertyComboBox.SelectedItem as VariableComboBoxItem;
                object value = environment.GetProperty(item.Name);
                ShowIntellisense(value, ResultValue);
            }
        }
        private void ResultValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HideIntellisense();
            ChangeTextBoxState(ResultValue, null);
            ResultValue.Text = ResultValue.Text.Trim();
            string input = ResultValue.Text;
            if (String.IsNullOrEmpty(input))
                return;

            if (PropertyComboBox.SelectedItem != null)
            {
                try
                {
                    var item = PropertyComboBox.SelectedItem as VariableComboBoxItem;
                    object value = environment.GetProperty(item.Name);
                    if (value != null)
                    {
                        object output = null;
                        if (PropertyTypeRule.TryParse(value, input, out output))
                        {
                            ResultValue.Text = output.ToString();
                            isExpressionResult = false;
                            return;
                        }
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        if (syntaxTree is Number)
                            throw new ExpressionException("TypeError");

                        var result = syntaxTree.Eval(environment);
                        if (!(PropertyTypeRule.IsMatchWith(value.GetType(), result.GetType())))
                            throw new ExpressionException("TypeError");

                        isExpressionResult = true;
                        return;
                    }
                    if (value == null)
                    {
                        value = environment.GetLocal(item.Name);
                    }
                    if (value == null)
                    {
                        value = environment.GetGlobal(item.Name);
                    }
                    if (value != null)
                    {
                        //Fields of support struct must be float type.
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        var result = syntaxTree.Eval(environment);
                        if (!(result is float))
                            throw new ExpressionException("TypeError");

                        isExpressionResult = true;
                    }
                }
                catch (ExpressionException error)
                {
                    ChangeTextBoxState(ResultValue, error);
                }
            }
        }
        private void ChangeTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ChangeTime, null);
            ChangeTime.Text = ChangeTime.Text.Trim();
            string input = ChangeTime.Text;
            if (String.IsNullOrEmpty(input))
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(ChangeTime, new ExpressionException("TypeError"));
            else if (value <= 0)
                ChangeTime.Text = "1";
        }
        private void Condition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(Condition, null);
            Condition.Text = Condition.Text.Trim();
            string input = Condition.Text;
            if (String.IsNullOrEmpty(input))
            {
                eventGroup.Condition = input;
                return;
            }
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Eval(environment);
                if (!(result is bool))
                    throw new ExpressionException("TypeError");
                eventGroup.Condition = input;
            }
            catch (ExpressionException error)
            {
                ChangeTextBoxState(Condition, error);
            }
        }
        private void StopCondition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(StopCondition, null);
            StopCondition.Text = StopCondition.Text.Trim();
            string input = StopCondition.Text;
            if (String.IsNullOrEmpty(input))
                return;

            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Eval(environment);
                if (!(result is bool))
                    throw new ExpressionException("TypeError");
            }
            catch (ExpressionException error)
            {
                ChangeTextBoxState(StopCondition, error);
            }
        }
        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeCombo.SelectedItem != null)
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
        }
        private void SoundTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundCombo.SelectedItem != null)
            {
                isPlaySound = !isPlaySound;
                if (isPlaySound)
                {
                    SoundTestButton.Content = (string)FindResource("PauseStr");
                    MediaPlayer.Source = new Uri(((FileResource)SoundCombo.SelectedItem).AbsolutePath, UriKind.Absolute);
                    MediaPlayer.Volume = VolumeSlider.Value / 100;
                    MediaPlayer.LoadedBehavior = MediaState.Manual;
                    MediaPlayer.Play();
                }
                else
                {
                    SoundTestButton.Content = (string)FindResource("TestStr");
                    MediaPlayer.Stop();
                }
            }
        }
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            isPlaySound = false;
            SoundTestButton.Content = (string)FindResource("TestStr");
            MediaPlayer.Stop();
        }
        private void EventItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //If mouse double clicked
            if (e.ClickCount == 2)
                EditEvent((e.OriginalSource as FrameworkElement).Parent as DockPanel);
        }
        private void EventList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeleteEvent();
        }
        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventList.SelectedIndex == -1)
                return;

            MapEventText(eventGroup.OriginalEvents[EventList.SelectedIndex]);
        }
        #endregion
    }
}
