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
        static readonly string[] GlobalEventStrings = new string[]
        {
            "GotoFrame", "Target", "Count",
            "QuakeScreen", "Strength", "Duration",
            "StopScreen", "Intensity", "Duration",
        };
        #region Private Members
        EventGroup eventGroup;
        Expression.Environment environment;
        IList<FileResource> sounds;
        IList<ParticleType> types;
        bool isPlaySound;
        bool isExpressionResult;
        Popup popup;
        #endregion

        #region Constructor
        public EventSetting(EventGroup eventGroup, Expression.Environment environment,
            IList<FileResource> sounds, IList<ParticleType> types, bool emitter, bool aboutParticle, bool center)
        {
            this.eventGroup = eventGroup;
            this.environment = environment;
            this.sounds = sounds;
            this.types = types;
            InitializeComponent();
            InitializeSetting(emitter, aboutParticle, center);
            LoadContent();
            ResetAll();
        }
        #endregion

        #region Private Methods
        void InitializeSetting(bool emitter, bool aboutParticle, bool center)
        {
            EventGroupBox.DataContext = eventGroup;
            EventList.ItemsSource = eventGroup.Events;
            EmitParticle.Visibility = emitter ? Visibility.Visible : Visibility.Collapsed;
            ChangeType.Visibility = aboutParticle ? Visibility.Visible : Visibility.Collapsed;
            GlobalEvent.Visibility = center ? Visibility.Visible : Visibility.Collapsed;
            if (center)
            {
                for (int i = 0; i < GlobalEventStrings.Length / 3; ++i)
                {
                    var item = new ComboBoxItem();
                    item.Content = ExpressionHelper.Translate(GlobalEventStrings[i * 3]);
                    GlobalEventTypeCombo.Items.Add(item);
                }
            }
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
                sound.CheckValid();
                SoundCombo.Items.Add(sound);
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
            else if (Sin.IsChecked == true) eventInfo.changeMode = Enum.GetName(typeof(EventChangeMode), EventChangeMode.Sin);
            else if (Cos.IsChecked == true) eventInfo.changeMode = Enum.GetName(typeof(EventChangeMode), EventChangeMode.Cos);  
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
                eventInfo.arguments = SoundCombo.SelectedItem.ToString();
            }
            else if (Loop.IsChecked == true)
            {
                if (string.IsNullOrEmpty(StopCondition.Text)) return false;
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
            else if (GlobalEvent.IsChecked == true)
            {
                if (GlobalEventTypeCombo.SelectedItem == null) return false;
                if (string.IsNullOrEmpty(GlobalEventParam1Value.Text) || string.IsNullOrEmpty(GlobalEventParam2Value.Text)) return false;
                if (UIHelper.HasError(GlobalEventParam1Value) || UIHelper.HasError(GlobalEventParam2Value)) return false;
                eventInfo.specialEvent = GlobalEventStrings[GlobalEventTypeCombo.SelectedIndex * 3];
                eventInfo.arguments = GlobalEventParam1Value.Text + "," + GlobalEventParam2Value.Text;
            }
            else if (OtherFunction.IsChecked == true)
            {
                if (string.IsNullOrEmpty(OtherFunctionLine.Text)) return false;
                if (UIHelper.HasError(OtherFunctionLine)) return false;
                var line = ExpressionHelper.ReverseTranslate(OtherFunctionLine.Text);
                var split = line.Split('(');
                eventInfo.specialEvent = split[0];
                eventInfo.arguments = split.Length >= 2 ? split[1].Split(')')[0] : string.Empty;
            }
            else return false;
            eventInfo.isSpecialEvent = true;
            text = EventHelper.BuildEvent(eventInfo, false);
            return true;
        }
        void EditEvent()
        {
            if (BuildEvent(out string text) && EventList.SelectedIndex != -1)
            {
                var index = EventList.SelectedIndex;
                if (text != eventGroup.Events[index])
                {
                    eventGroup.Events[index] = text;
                    EventList.SelectedIndex = index;
                }
            }
        }
        void EditSpecialEvent()
        {
            if (BuildSpecialEvent(out string text) && EventList.SelectedIndex != -1)
            {
                var index = EventList.SelectedIndex;
                if (text != eventGroup.Events[index])
                {
                    eventGroup.Events[index] = text;
                    EventList.SelectedIndex = index;
                }
            }
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
            Sin.IsChecked = false;
            Cos.IsChecked = false;
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
            GlobalEvent.IsChecked = false;
            GlobalEventTypeCombo.SelectedIndex = -1;
            GlobalEventParam1Value.Text = string.Empty;
            GlobalEventParam2Value.Text = string.Empty;
            TypeCombo.SelectedIndex = -1;
            ColorCombo.SelectedIndex = -1;
            OtherFunction.IsChecked = false;
            OtherFunctionLine.Text = string.Empty;
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
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Sin)] = new[] { Sin };
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Cos)] = new[] { Cos };
            buttonMap[Enum.GetName(typeof(EventChangeMode), EventChangeMode.Instant)] = new[] { Instant };
            buttonMap["EmitParticle"] = new[] { EmitParticle };
            buttonMap["PlaySound"] = new[] { PlaySound };
            buttonMap["Loop"] = new[] { Loop };
            buttonMap["ChangeType"] = new[] { ChangeType };
            for (int i = 0; i < GlobalEventStrings.Length / 3; ++i)
            {
                buttonMap[GlobalEventStrings[i * 3]] = new[] { GlobalEvent };
            }
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
                if (buttonMap.ContainsKey(eventInfo.specialEvent))
                {
                    foreach (var button in buttonMap[eventInfo.specialEvent]) button.IsChecked = true;
                }
                else
                {
                    OtherFunction.IsChecked = true;
                    OtherFunctionLine.Text = ExpressionHelper.Translate(string.Format("{0}({1})", eventInfo.specialEvent,
                        eventInfo.arguments));
                    return;
                }
                string[] split = eventInfo.arguments.Split(',');
                if (eventInfo.specialEvent == "PlaySound")
                {
                    for (int i = 0; i < SoundCombo.Items.Count; ++i)
                    {
                        if ((SoundCombo.Items[i] as FileResource).Label == split[0])
                        {
                            SoundCombo.SelectedIndex = i;
                            return;
                        }
                    }
                }
                else if (eventInfo.specialEvent == "Loop")
                {
                    StopCondition.Text = ExpressionHelper.Translate(split[0].Trim());
                    return;
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
                    if (TypeCombo.SelectedItem != null) ColorCombo.SelectedIndex = int.Parse(split[1].Trim());
                    return;
                }
                else
                {
                    for (int i = 0; i < GlobalEventStrings.Length / 3; ++i)
                    {
                        if (eventInfo.specialEvent == GlobalEventStrings[i * 3])
                        {
                            GlobalEventTypeCombo.SelectedIndex = i;
                            GlobalEventParam1Value.Text = split[0].Trim();
                            GlobalEventParam2Value.Text = split[1].Trim();
                            return;
                        }
                    }
                }
            }
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
            Sin.IsChecked = false;
            Cos.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
            EditEvent();
        }
        private void Accelerated_Checked(object sender, RoutedEventArgs e)
        {
            Linear.IsChecked = false;
            Decelerated.IsChecked = false;
            Sin.IsChecked = false;
            Cos.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
            EditEvent();
        }
        private void Decelerated_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Sin.IsChecked = false;
            Cos.IsChecked = false;
            Linear.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
            EditEvent();
        }
        private void Sin_Checked(object sender, RoutedEventArgs e)
        {
            Linear.IsChecked = false;
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Cos.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
            EditEvent();
        }
        private void Cos_Checked(object sender, RoutedEventArgs e)
        {
            Linear.IsChecked = false;
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Sin.IsChecked = false;
            Instant.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Visible;
            EditEvent();
        }
        private void Instant_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Sin.IsChecked = false;
            Cos.IsChecked = false;
            Linear.IsChecked = false;
            ChangeTimePanel.Visibility = Visibility.Collapsed;
            ChangeTime.Text = string.Empty;
            EditEvent();
        }
        private void EmitParticleButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
            OtherFunctionPanel.Visibility = Visibility.Collapsed;
            EditSpecialEvent();
        }
        private void PlaySoundButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Visible;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
            GlobalEventPanel.Visibility = Visibility.Collapsed;
            OtherFunctionPanel.Visibility = Visibility.Collapsed;
            EditSpecialEvent();
        }
        private void LoopButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Visible;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
            GlobalEventPanel.Visibility = Visibility.Collapsed;
            OtherFunctionPanel.Visibility = Visibility.Collapsed;
            EditSpecialEvent();
        }
        private void ChangeTypeButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Visible;
            OtherFunctionPanel.Visibility = Visibility.Collapsed;
            EditSpecialEvent();
        }
        private void GlobalEventButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            GlobalEventPanel.Visibility = Visibility.Visible;
            GlobalEventParamPanel.Visibility = GlobalEventTypeCombo.SelectedItem != null ? 
                Visibility.Visible : Visibility.Collapsed;
            OtherFunctionPanel.Visibility = Visibility.Collapsed;
            EditSpecialEvent();
        }
        private void OtherFunctionButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
            ChangeTypePanel.Visibility = Visibility.Collapsed;
            GlobalEventPanel.Visibility = Visibility.Collapsed;
            OtherFunctionPanel.Visibility = Visibility.Visible;
            EditSpecialEvent();
        }
        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (BuildEvent(out text))
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
            DelEvent.IsEnabled = EventList.SelectedItem != null;
            DelSpecialEvent.IsEnabled = EventList.SelectedItem != null;
        }
        private void DelEvent_Click(object sender, RoutedEventArgs e)
        {
            DeleteEvent();
        }
        private void AddSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            string text;
            if (BuildSpecialEvent(out text))
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
            DelEvent.IsEnabled = EventList.SelectedItem != null;
            DelSpecialEvent.IsEnabled = EventList.SelectedItem != null;
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
        private void ChangeTypeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            EditEvent();
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
                            EditEvent();
                            return;
                        }
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        if (syntaxTree is Number) throw new ExpressionException("TypeError");
                        var result = syntaxTree.Eval(environment);
                        if (!(PropertyTypeRule.IsMatchWith(value.GetType(), result.GetType()))) throw new ExpressionException("TypeError");
                        isExpressionResult = true;
                        EditEvent();
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
                    EditEvent();
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
            EditEvent();
        }
        private void StopCondition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.SetErrorToolTip(StopCondition, null);
            StopCondition.Text = StopCondition.Text.Trim();
            string input = ExpressionHelper.ReverseTranslate(StopCondition.Text);
            if (String.IsNullOrEmpty(input)) return;
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Eval(environment);
                if (!(result is bool)) throw new ExpressionException("TypeError");
                EditSpecialEvent();
            }
            catch (ExpressionException error)
            {
                UIHelper.SetErrorToolTip(StopCondition, error);
            }
        }
        private void OtherFunctionLine_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UIHelper.SetErrorToolTip(OtherFunctionLine, null);
            OtherFunctionLine.Text = OtherFunctionLine.Text.Trim();
            string input = ExpressionHelper.ReverseTranslate(OtherFunctionLine.Text);
            if (string.IsNullOrEmpty(input)) return;
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                if (!(syntaxTree is Call)) throw new ExpressionException("IllegalInput");
                var arguments = (syntaxTree as Call).GetArguments() as Arguments;
                foreach (var argument in arguments.GetArguments())
                {
                    var result = argument.Eval(environment);
                    if (!(result is int) && !(result is float) && !(result is bool)) 
                        throw new ExpressionException("TypeError");
                }
                EditSpecialEvent();
            }
            catch (ExpressionException error)
            {
                UIHelper.SetErrorToolTip(OtherFunctionLine, error);
            }
        }
        private void GlobalEventParamValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            UIHelper.SetErrorToolTip(textBox, null);
            textBox.Text = textBox.Text.Trim();
            if (String.IsNullOrEmpty(textBox.Text)) return;
            try
            {
                float.Parse(textBox.Text);
                EditSpecialEvent();
            }
            catch
            {
                UIHelper.SetErrorToolTip(textBox, new ExpressionException("TypeError"));
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
        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorCombo.SelectedItem != null) EditSpecialEvent();
        }
        private void SoundCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoundCombo.SelectedItem != null) EditSpecialEvent();
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
        private void GlobalEventTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalEventTypeCombo.SelectedItem == null) return;
            GlobalEventParamPanel.Visibility = Visibility.Visible;
            GlobalEventParam1Name.Content = ExpressionHelper.Translate(GlobalEventStrings[GlobalEventTypeCombo.SelectedIndex * 3 + 1]);
            GlobalEventParam2Name.Content = ExpressionHelper.Translate(GlobalEventStrings[GlobalEventTypeCombo.SelectedIndex * 3 + 2]);
            EditSpecialEvent();
        }
        private void EventList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) DeleteEvent();
        }
        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventList.SelectedIndex == -1) return;
            MapEventText(eventGroup.Events[EventList.SelectedIndex]);
            DelEvent.IsEnabled = EventList.SelectedItem != null;
            DelSpecialEvent.IsEnabled = EventList.SelectedItem != null;
        }
        private void EventCondition_ConditionChanged(object sender, ConditionChangedEventArgs e)
        {
            if (EventList.SelectedIndex == -1) return;
            var text = eventGroup.Events[EventList.SelectedIndex];
            if (EventHelper.IsSpecialEvent(text)) EditSpecialEvent();
            else EditEvent();
        }
        private void ResultValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResultValue_PreviewLostKeyboardFocus(null, null);
                ResultValue.CaretIndex = ResultValue.Text.Length;
            }
        }
        private void ChangeTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ChangeTime_PreviewLostKeyboardFocus(null, null);
                ChangeTime.CaretIndex = ChangeTime.Text.Length;
            }
        }
        private void StopCondition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StopCondition_PreviewLostKeyboardFocus(null, null);
                StopCondition.CaretIndex = StopCondition.Text.Length;
            }
        }
        private void OtherFunctionLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OtherFunctionLine_PreviewLostKeyboardFocus(null, null);
                OtherFunctionLine.CaretIndex = OtherFunctionLine.Text.Length;
            }
        }
        private void GlobalEventParamValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GlobalEventParamValue_PreviewLostKeyboardFocus(sender, null);
                var textBox = sender as TextBox;
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
        #endregion
    }
}
