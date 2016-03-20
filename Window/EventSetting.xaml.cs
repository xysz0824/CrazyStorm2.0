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
using System.Windows.Shapes;
using CrazyStorm.Core;
using CrazyStorm.Script;

namespace CrazyStorm
{
    /// <summary>
    /// Interaction logic for EventSetting.xaml
    /// </summary>
    public partial class EventSetting : Window
    {
        #region Private Members
        EventGroup eventGroup;
        Script.Environment environment;
        IList<Resource> sounds;
        IList<ParticleType> types;
        IList<PropertyPanelItem>[] properties;
        bool emitter;
        bool aboutParticle;
        bool isPlaySound;
        #endregion

        #region Constructor
        public EventSetting(EventGroup eventGroup, Script.Environment environment,
            IList<Resource> sounds, IList<ParticleType> types, IList<PropertyPanelItem>[] properties, 
            bool emitter, bool aboutParticle)
        {
            this.eventGroup = eventGroup;
            this.environment = environment;
            this.sounds = sounds;
            this.types = types;
            this.properties = properties;
            this.emitter = emitter;
            this.aboutParticle = aboutParticle;
            InitializeComponent();
            InitializeSetting();
            LoadContent();
        }
        #endregion

        #region Private Methods
        void InitializeSetting()
        {
            GroupBox.DataContext = eventGroup;
            EventList.ItemsSource = eventGroup.Events;
            EmitParticleButton.Visibility = emitter ? Visibility.Visible : Visibility.Collapsed;
            ChangeTypeButton.Visibility = aboutParticle ? Visibility.Visible : Visibility.Collapsed;
        }
        void LoadContent()
        {
            //Load locals and globals.
            foreach (var item in environment.Locals)
            {
                LeftConditionComboBox.Items.Add(item.Key);
                RightConditionComboBox.Items.Add(item.Key);
                PropertyComboBox.Items.Add(item.Key);
                //Split fields of struct for convenience
                foreach (var structItem in environment.Structs)
                {
                    if (structItem.Key == item.Value.GetType().ToString())
                    {
                        foreach (var fieldItem in structItem.Value.GetFields())
                        {
                            LeftConditionComboBox.Items.Add(item.Key + "." + fieldItem.Key);
                            RightConditionComboBox.Items.Add(item.Key + "." + fieldItem.Key);
                            PropertyComboBox.Items.Add(item.Key + "." + fieldItem.Key);
                        }
                    }
                }
            }
            foreach (var item in environment.Globals)
            {
                LeftConditionComboBox.Items.Add(item.Key);
                RightConditionComboBox.Items.Add(item.Key);
                PropertyComboBox.Items.Add(item.Key);
            }
            //Load sounds.
            foreach (FileResource item in sounds)
            {
                if (item.IsValid)
                    SoundCombo.Items.Add(item);
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
        }
        void ChangeTextBoxState(TextBox source, bool hasError)
        {
            if (hasError)
            {
                var tip = new ToolTip();
                var tipText = new TextBlock();
                tipText.Text = (string)FindResource("ValueInvalid");
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
        #endregion

        #region Window EventHandler
        private void Linear_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Fixed.IsChecked = false;
        }
        private void Accelerated_Checked(object sender, RoutedEventArgs e)
        {
            Linear.IsChecked = false;
            Decelerated.IsChecked = false;
            Fixed.IsChecked = false;
        }
        private void Decelerated_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Linear.IsChecked = false;
            Fixed.IsChecked = false;
        }
        private void Fixed_Checked(object sender, RoutedEventArgs e)
        {
            Accelerated.IsChecked = false;
            Decelerated.IsChecked = false;
            Linear.IsChecked = false;
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
            //TODO : Add event.
        }
        private void AddSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            //TODO : Add special event.
        }
        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = EventList.SelectedItem;
            if (item != null)
                eventGroup.Events.Remove((string)item);
        }
        private void LeftConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftMoreThan.IsChecked = false;
            LeftEqual.IsChecked = false;
            LeftLessThan.IsChecked = false;
            LeftMoreThan.IsEnabled = true;
            LeftLessThan.IsEnabled = true;
            LeftValue.Text = string.Empty;
            ChangeTextBoxState(LeftValue, false);
            string selection = e.AddedItems[0].ToString();
            foreach (var item in environment.Locals)
            {
                if (selection == item.Key)
                {
                    LeftMoreThan.IsEnabled = (item.Value is int || item.Value is float);
                    LeftLessThan.IsEnabled = (item.Value is int || item.Value is float);
                    return;
                }
            }
        }
        private void RightConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightMoreThan.IsChecked = false;
            RightEqual.IsChecked = false;
            RightLessThan.IsChecked = false;
            RightMoreThan.IsEnabled = true;
            RightLessThan.IsEnabled = true;
            RightValue.Text = string.Empty;
            ChangeTextBoxState(RightValue, false);
            string selection = e.AddedItems[0].ToString();
            foreach (var item in environment.Locals)
            {
                if (selection == item.Key)
                {
                    RightMoreThan.IsEnabled = (item.Value is int || item.Value is float);
                    RightLessThan.IsEnabled = (item.Value is int || item.Value is float);
                    return;
                }
            }
        }
        private void PropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultValue.Text = string.Empty;
            ChangeTextBoxState(ResultValue, false);
        }
        private void LeftValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(LeftValue, false);
            LeftValue.Text = LeftValue.Text.Trim();
            string input = LeftValue.Text;
            if (input == string.Empty)
                return;

            if (LeftConditionComboBox.SelectedItem != null)
            {
                string[] selection = ((string)LeftConditionComboBox.SelectedItem).Split('.');
                object value = environment.GetLocal(selection[0]);
                if (value != null)
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        foreach (var item in properties[i])
                        {
                            if (selection[0] == item.Name && selection.Length <= 1)
                            {
                                var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                if (!attribute.IsLegal(input, out value))
                                {
                                    ChangeTextBoxState(LeftValue, true);
                                    return;
                                }
                                LeftValue.Text = value.ToString();
                                return;
                            }
                            else if (selection[0] == item.Name)
                            {
                                value = environment.GetStructs(item.Info.PropertyType.ToString()).GetField(selection[1]);
                                if (!Rule.IsMatchWith(value, input))
                                {
                                    ChangeTextBoxState(LeftValue, true);
                                }
                                return;
                            }
                        }
                    }
                }
                if (value == null)
                {
                    value = environment.GetGlobal(selection[0]);
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        ChangeTextBoxState(LeftValue, true);
                        return;
                    }
                }
            }
        }
        private void RightValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(RightValue, false);
            RightValue.Text = RightValue.Text.Trim();
            string input = RightValue.Text;
            if (input == string.Empty)
                return;

            if (RightConditionComboBox.SelectedItem != null)
            {
                string[] selection = ((string)RightConditionComboBox.SelectedItem).Split('.');
                object value = environment.GetLocal(selection[0]);
                if (value != null)
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        foreach (var item in properties[i])
                        {
                            if (selection[0] == item.Name && selection.Length <= 1)
                            {
                                var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                if (!attribute.IsLegal(input, out value))
                                {
                                    ChangeTextBoxState(RightValue, true);
                                    return;
                                }
                                RightValue.Text = value.ToString();
                                return;
                            }
                            else if (selection[0] == item.Name)
                            {
                                value = environment.GetStructs(item.Info.PropertyType.ToString()).GetField(selection[1]);
                                if (!Rule.IsMatchWith(value, input))
                                    ChangeTextBoxState(RightValue, true);

                                return;
                            }
                        }
                    }
                }
                if (value == null)
                {
                    value = environment.GetGlobal(selection[0]);
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        ChangeTextBoxState(RightValue, true);
                        return;
                    }
                }
            }
        }
        private void ResultValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ResultValue, false);
            ResultValue.Text = ResultValue.Text.Trim();
            string input = ResultValue.Text;
            if (input == string.Empty)
                return;

            if (PropertyComboBox.SelectedItem != null)
            {
                try
                {
                    string[] selection = ((string)PropertyComboBox.SelectedItem).Split('.');
                    object value = environment.GetLocal(selection[0]);
                    if (value != null)
                    {
                        for (int i = 0; i < properties.Length; ++i)
                        {
                            foreach (var item in properties[i])
                            {
                                if (selection[0] == item.Name && selection.Length <= 1)
                                {
                                    var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                    if (attribute.IsLegal(input, out value))
                                    {
                                        ResultValue.Text = value.ToString();
                                        return;
                                    }
                                    var lexer = new Lexer();
                                    lexer.Load(input);
                                    var syntaxTree = new Parser(lexer).Expression();
                                    if (syntaxTree is Number)
                                        throw new ScriptException();

                                    var result = syntaxTree.Test(environment);
                                    if (!(Rule.IsMatchWith(item.Info.PropertyType, result.GetType())))
                                        throw new ScriptException();

                                    return;
                                }
                                else if (selection[0] == item.Name)
                                {
                                    value = environment.GetStructs(item.Info.PropertyType.ToString()).GetField(selection[1]);
                                    var lexer = new Lexer();
                                    lexer.Load(input);
                                    var syntaxTree = new Parser(lexer).Expression();
                                    var result = syntaxTree.Test(environment);
                                    if (!Rule.IsMatchWith(value.GetType(), result.GetType()))
                                        throw new ScriptException();

                                    return;
                                }
                            }
                        }
                    }
                    if (value == null)
                    {
                        value = environment.GetGlobal(selection[0]);
                    }
                    if (value != null)
                    {
                        var lexer = new Lexer();
                        lexer.Load(input);
                        var syntaxTree = new Parser(lexer).Expression();
                        var result = syntaxTree.Test(environment);
                        if (!(result is float))
                            throw new ScriptException();
                    }
                }
                catch (ScriptException ex)
                {
                    ChangeTextBoxState(ResultValue, true);
                }
            }
        }
        private void ChangeTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ChangeTime, false);
            ChangeTime.Text = ChangeTime.Text.Trim();
            string input = ChangeTime.Text;
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(ChangeTime, true);
            else if (value <= 0)
                ChangeTime.Text = "1";
        }
        private void ExecuteTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(ExecuteTime, false);
            ExecuteTime.Text = ExecuteTime.Text.Trim();
            string input = ExecuteTime.Text;
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(ExecuteTime, true);
            else if (value < 0)
                ExecuteTime.Text = "0";
        }
        private void LoopTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(LoopTime, false);
            LoopTime.Text = LoopTime.Text.Trim();
            string input = LoopTime.Text;
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
                ChangeTextBoxState(LoopTime, true);
            else if (value <= 0)
                LoopTime.Text = "1";
        }
        private void Condition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(Condition, false);
            Condition.Text = Condition.Text.Trim();
            string input = Condition.Text;
            if (input == string.Empty)
            {
                eventGroup.Condition = input;
                return;
            }
            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Test(environment);
                if (!(result is bool))
                    throw new ScriptException();
                eventGroup.Condition = input;
            }
            catch (ScriptException ex)
            {
                ChangeTextBoxState(Condition, true);
            }
        }
        private void StopCondition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeTextBoxState(StopCondition, false);
            StopCondition.Text = StopCondition.Text.Trim();
            string input = StopCondition.Text;
            if (input == string.Empty)
                return;

            try
            {
                var lexer = new Lexer();
                lexer.Load(input);
                var syntaxTree = new Parser(lexer).Expression();
                var result = syntaxTree.Test(environment);
                if (!(result is bool))
                    throw new ScriptException();
            }
            catch (ScriptException ex)
            {
                ChangeTextBoxState(StopCondition, true);
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
                        color.Content = item.Color.ToString();
                        ColorCombo.Items.Add(color);
                    }
                }
            }
        }
        private void SoundTextButton_Click(object sender, RoutedEventArgs e)
        {
            isPlaySound = !isPlaySound;
            if (SoundCombo.SelectedItem != null)
            {
                if (isPlaySound)
                {
                    SoundTestButton.Content = (string)FindResource("Pause");
                    MediaPlayer.Source = new Uri(((FileResource)SoundCombo.SelectedItem).AbsolutePath, UriKind.Absolute);
                    MediaPlayer.Volume = VolumeSlider.Value / 100;
                    MediaPlayer.LoadedBehavior = MediaState.Manual;
                    MediaPlayer.Play();
                }
                else
                {
                    SoundTestButton.Content = (string)FindResource("Test");
                    MediaPlayer.Stop();
                }
            }
        }
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            isPlaySound = false;
            SoundTestButton.Content = (string)FindResource("Test");
            MediaPlayer.Stop();
        }
        #endregion
    }
}
