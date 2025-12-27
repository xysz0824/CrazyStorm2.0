/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
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
using System.Windows.Documents.Serialization;

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
            ResetAll();
        }
        #endregion

        #region Private Methods
        void InitializeSetting(bool emitter, bool aboutParticle)
        {
            EventGroupBox.DataContext = eventGroup;
            EventList.ItemsSource = eventGroup.Events;
            EmitParticle.Visibility = emitter ? Visibility.Visible : Visibility.Collapsed;
            ChangeType.Visibility = aboutParticle ? Visibility.Visible : Visibility.Collapsed;
        }
        void LoadContent()
        {
            GroupCondition.Environment = environment;
            EventCondition.Environment = environment;
            //Load properties.
            foreach (var property in environment.Properties)
            {
                var item = new VariableComboBoxItem();
                item.Name = property.Key;
                var displayName = ExpressionHelper.TranslateProperty(property.Key);
                item.DisplayName = displayName != null ? displayName : property.Key;
                GroupCondition.AddConditionVariable(item);
                EventCondition.AddConditionVariable(item);
                PropertyComboBox.Items.Add(item);
            }
            //Load locals.
            foreach (var local in environment.Locals)
            {
                var item = new VariableComboBoxItem();
                item.Name = local.Key;
                item.DisplayName = item.Name;
                GroupCondition.AddConditionVariable(item);
                EventCondition.AddConditionVariable(item);
                PropertyComboBox.Items.Add(item);
            }
            //Load globals.
            foreach (var global in environment.Globals)
            {
                var item = new VariableComboBoxItem();
                item.Name = global.Key;
                item.DisplayName = item.Name;
                GroupCondition.AddConditionVariable(item);
                EventCondition.AddConditionVariable(item);
                PropertyComboBox.Items.Add(item);
            }
            //backfill group condition
            GroupCondition.MapCondition(eventGroup.Condition);
            //Load sounds.
            foreach (FileResource sound in sounds)
            {
                if (sound.IsValid) SoundCombo.Items.Add(sound);
            }
            //Load particle types.
            //First needs to merge repeated type name.
            var typesNorepeat = new List<ParticleType>();
            foreach (var item in types)
            {
                bool exist = false;
                for (int i = 0; i < typesNorepeat.Count; ++i)
                {
                    if (item.Name == typesNorepeat[i].Name)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist) typesNorepeat.Add(item);
            }
            TypeCombo.ItemsSource = typesNorepeat;
            //Create sorted variable list for translating event
            var varaibleList = new List<VariableComboBoxItem>();
            foreach (VariableComboBoxItem item in PropertyComboBox.Items) varaibleList.Add(item);
            //Longer name first
            sortedVaraibles = varaibleList.OrderByDescending(s => s.Name.Length);
        }
        bool BuildEvent(out string text)
        {
            text = string.Empty;
            var eventInfo = new EventInfo();
            //Check if there have errors
            if (UIHelper.HasError(ResultValue) || UIHelper.HasError(ChangeTime)) return false;
            eventInfo.condition = EventCondition.BuildCondition();
            if (EventCondition.HasError()) return false;
            if (ChangeTo.IsChecked == true) eventInfo.changeType = Enum.GetName(typeof(EventChangeType), EventChangeType.ChangeTo);
            else if (Increase.IsChecked == true) eventInfo.changeType = Enum.GetName(typeof(EventChangeType), EventChangeType.Increase);
            else if (Decrease.IsChecked == true) eventInfo.changeType = Enum.GetName(typeof(EventChangeType), EventChangeType.Decrease);
            if (Linear.IsChecked == true) eventInfo.changeMode = Enum.GetName(typeof(EventChangeMode), EventChangeMode.Linear);
            else if (Accelerated.IsChecked == true) eventInfo.changeMode = Enum.GetName(typeof(EventChangeMode), EventChangeMode.Accelerated);
            else if (Decelerated.IsChecked == true) eventInfo.changeMode = Enum.GetName(typeof(EventChangeMode), EventChangeMode.Decelerated);
            else if (Instant.IsChecked == true) eventInfo.changeMode = Enum.GetName(typeof(EventChangeMode), EventChangeMode.Instant);
            if (PropertyComboBox.SelectedItem != null && !String.IsNullOrEmpty(ResultValue.Text) &&
                !String.IsNullOrEmpty(eventInfo.changeType) && !String.IsNullOrEmpty(eventInfo.changeMode) &&
                (!String.IsNullOrEmpty(ChangeTime.Text) || Instant.IsChecked == true))
            {
                var selectedItem = PropertyComboBox.SelectedItem as VariableComboBoxItem;
                eventInfo.resultProperty = selectedItem.Name;
                eventInfo.isExpressionResult = isExpressionResult;
                eventInfo.resultType = environment.GetValueType(selectedItem.Name);
                eventInfo.resultValue = ExpressionHelper.ReverseTranslate(ResultValue.Text);
                if (Instant.IsChecked == true) eventInfo.changeTime = "1";
                else eventInfo.changeTime = ChangeTime.Text;
            }
            else return false;
            text = EventHelper.BuildEvent(eventInfo, true);
            return true;
        }
        bool BuildSpecialEvent(out string text)
        {
            text = string.Empty;
            var eventInfo = new EventInfo();
            eventInfo.condition = EventCondition.BuildCondition();
            if (EventCondition.HasError()) return false;
            if (EmitParticle.IsChecked == true)
            {
                eventInfo.specialEvent = "EmitParticle";
                eventInfo.arguments = string.Empty;
            }
            else if (PlaySound.IsChecked == true)
            {
                if (SoundCombo.SelectedItem == null) return false;
                eventInfo.specialEvent = "PlaySound";
                eventInfo.arguments = SoundCombo.SelectedItem + ", " + VolumeSlider.Value;
            }
            else if (Loop.IsChecked == true)
            {
                //Check if there have errors
                if (UIHelper.HasError(StopCondition)) return false;
                string arguments = string.Empty;
                arguments = ExpressionHelper.ReverseTranslate(StopCondition.Text);
                eventInfo.specialEvent = "Loop";
                eventInfo.arguments = arguments;
            }
            else if (ChangeType.IsChecked == true)
            {
                if (TypeCombo.SelectedItem == null || ColorCombo.SelectedItem == null) return false;
                eventInfo.specialEvent = "ChangeType";
                eventInfo.arguments = (TypeCombo.SelectedItem as ParticleType).ID + "," + ColorCombo.SelectedIndex;
            }
            else return false;
            eventInfo.isSpecialEvent = true;
            text = EventHelper.BuildEvent(eventInfo, true);
            return true;
        }
        void ResetAll()
        {
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
            var buttonMap = new Dictionary<string, RadioButton[]>();
            buttonMap[Enum.GetName(typeof(EventChangeType), EventChangeType.ChangeTo)] = new[] { ChangeTo };
            buttonMap[Enum.GetName(typeof(EventChangeType), EventChangeType.Increase)] = new[] { Increase };
            buttonMap[Enum.GetName(typeof(EventChangeType), EventChangeType.Decrease)] = new[] { Decrease };
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Linear)] = new[] { Linear };
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Accelerated)] = new[] { Accelerated };
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Decelerated)] = new[] { Decelerated };
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Instant)] = new[] { Instant };
            buttonMap["EmitParticle"] = new[] { EmitParticle };
            buttonMap["PlaySound"] = new[] { PlaySound };
            buttonMap["Loop"] = new[] { Loop };
            buttonMap["ChangeType"] = new[] { ChangeType };
            EventInfo eventInfo = EventHelper.SplitEvent(text);
            //Backfill condition
            EventCondition.MapCondition(eventInfo.condition);
            //Backfill event
            if (!eventInfo.isSpecialEvent)
            {
                for (int i = 0; i < PropertyComboBox.Items.Count; ++i)
                {
                    var item = PropertyComboBox.Items[i] as VariableComboBoxItem;
                    if (item.Name == eventInfo.resultProperty)
                    {
                        PropertyComboBox.SelectedIndex = i;
                        break;
                    }
                }
                isExpressionResult = eventInfo.isExpressionResult;
                foreach (var button in buttonMap[eventInfo.changeType]) button.IsChecked = true;
                ResultValue.Text = ExpressionHelper.Translate(eventInfo.resultValue);
                foreach (var button in buttonMap[eventInfo.changeMode]) button.IsChecked = true;
                ChangeTime.Text = eventInfo.changeTime;
            }
            else
            {
                foreach (var button in buttonMap[eventInfo.specialEvent]) button.IsChecked = true;
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
            DelEvent.IsEnabled = false;
            DelSpecialEvent.IsEnabled = false;
            isEditing = true;
        }
        void DeleteEvent()
        {
            var item = EventList.SelectedItem;
            if (item != null)
            {
                eventGroup.Events.RemoveAt(EventList.SelectedIndex);
            }
            EventList.ItemsSource = eventGroup.Events;
            DelEvent.IsEnabled = EventList.SelectedItem != null;
            DelSpecialEvent.IsEnabled = EventList.SelectedItem != null;
        }
        void ShowIntellisense(object property, UIElement element)
        {
            popup = new Popup();
            UIHelper.ShowIntellisense(popup, property.GetType(), element);
        }
        void HideIntellisense()
        {
            UIHelper.HideIntellisense(popup);
        }
        #endregion

        #region Window EventHandlers
        private void GroupCondition_ConditionChanged(object sender, ConditionChangedEventArgs e)
        {
            eventGroup.Condition = e.ModifiedCondition;
        }
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
                    eventGroup.Events[EventList.SelectedIndex] = text;
                    editingPanel.Background = null;
                    EventList.IsEnabled = true;
                    AddEvent.Content = (string)FindResource("AddStr");
                    PropertyEventPanel.IsEnabled = true;
                    AddSpecialEvent.Content = AddEvent.Content;
                    SpecialEventPanel.IsEnabled = true;
                    DelEvent.IsEnabled = EventList.SelectedItem != null;
                    DelSpecialEvent.IsEnabled = EventList.SelectedItem != null;
                    isEditing = false;
                }
            }
            else if (BuildEvent(out text))
            {
                if (EventList.SelectedIndex != -1 && EventList.Items.Count - 1 > EventList.SelectedIndex)
                {
                    eventGroup.Events.Insert(EventList.SelectedIndex + 1, text);
                }
                else
                {
                    eventGroup.Events.Add(text);
                }
            }
        }
        private void DelEvent_Click(object sender, RoutedEventArgs e)
        {
            DeleteEvent();
        }
        private void AddSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (isEditing)
            {
                if (BuildSpecialEvent(out text))
                {
                    eventGroup.Events[EventList.SelectedIndex] = text;
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
                    eventGroup.Events.Insert(EventList.SelectedIndex + 1, text);
                }
                else
                {
                    eventGroup.Events.Add(text);
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
        private void DelSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            DeleteEvent();
        }   
        private void PropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultValue_PreviewLostKeyboardFocus(sender, null);
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
            UIHelper.SetErrorToolTip(ResultValue, null);
            ResultValue.Text = ResultValue.Text.Trim();
            string input = ExpressionHelper.ReverseTranslate(ResultValue.Text);
            if (String.IsNullOrEmpty(input)) return;
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
                            ResultValue.Text = ExpressionHelper.Translate(output.ToString());
                            isExpressionResult = false;
                            return;
                        }
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        if (syntaxTree is Number) throw new ExpressionException("TypeError");
                        var result = syntaxTree.Eval(environment);
                        if (!(PropertyTypeRule.IsMatchWith(value.GetType(), result.GetType()))) throw new ExpressionException("TypeError");
                        isExpressionResult = true;
                        return;
                    }
                    if (value == null) value = environment.GetLocal(item.Name);
                    if (value == null) value = environment.GetGlobal(item.Name);
                    if (value != null)
                    {
                        //Fields of support struct must be float type.
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        var result = syntaxTree.Eval(environment);
                        if (!(result is float)) throw new ExpressionException("TypeError");
                        isExpressionResult = true;
                    }
                }
                catch (ExpressionException error)
                {
                    UIHelper.SetErrorToolTip(ResultValue, error);
                }
            }
        }
        private void ChangeTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.SetErrorToolTip(ChangeTime, null);
            ChangeTime.Text = ChangeTime.Text.Trim();
            string input = ChangeTime.Text;
            if (String.IsNullOrEmpty(input)) return;
            int value;
            if (!int.TryParse(input, out value)) UIHelper.SetErrorToolTip(ChangeTime, new ExpressionException("TypeError"));
            else if (value <= 0) ChangeTime.Text = "1";
        }
        private void StopCondition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.SetErrorToolTip(StopCondition, null);
            StopCondition.Text = StopCondition.Text.Trim();
            string input = ExpressionHelper.Translate(StopCondition.Text);
            if (String.IsNullOrEmpty(input)) return;
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Eval(environment);
                if (!(result is bool)) throw new ExpressionException("TypeError");
            }
            catch (ExpressionException error)
            {
                UIHelper.SetErrorToolTip(StopCondition, error);
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
            if (e.ClickCount == 2) EditEvent((e.OriginalSource as FrameworkElement).Parent as DockPanel);
        }
        private void EventList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && EventList.SelectedItem != null)
            {
                var container = EventList.ItemContainerGenerator.ContainerFromItem(EventList.SelectedItem) as ListViewItem;
                if (container == null) return;
                EditEvent(VisualHelper.GetVisualChild<DockPanel>(container));
            }
            else if (e.Key == Key.Delete) DeleteEvent();
        }
        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventList.SelectedIndex == -1) return;
            MapEventText(eventGroup.Events[EventList.SelectedIndex]);
            DelEvent.IsEnabled = EventList.SelectedItem != null;
            DelSpecialEvent.IsEnabled = EventList.SelectedItem != null;
        }
        private void EventCondition_ConditionConfirmed(object sender, EventArgs e)
        {
            if (isEditing)
            {
                AddEvent_Click(null, null);
                AddEvent.Focus();
            }
        }
        private void ResultValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (isEditing && e.Key == Key.Enter)
            {
                ResultValue_PreviewLostKeyboardFocus(null, null);
                if (UIHelper.HasError(ResultValue)) return;
                AddEvent_Click(null, null);
                AddEvent.Focus();
            }
        }
        private void ChangeTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (isEditing && e.Key == Key.Enter)
            {
                ChangeTime_PreviewLostKeyboardFocus(null, null);
                if (UIHelper.HasError(ChangeTime)) return;
                AddEvent_Click(null, null);
                AddEvent.Focus();
            }
        }
        private void StopCondition_KeyDown(object sender, KeyEventArgs e)
        {
            if (isEditing && e.Key == Key.Enter)
            {
                StopCondition_PreviewLostKeyboardFocus(null, null);
                if (UIHelper.HasError(StopCondition)) return;
                AddSpecialEvent_Click(null, null);
                AddSpecialEvent.Focus();
            }
        }
        #endregion
    }
}
