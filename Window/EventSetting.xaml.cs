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
        IList<PropertyPanelItem>[] properties;
        bool emitter;
        bool aboutParticle;
        #endregion

        #region Constructor
        public EventSetting(EventGroup eventGroup, Script.Environment environment, IList<PropertyPanelItem>[] properties, 
            bool emitter, bool aboutParticle)
        {
            this.eventGroup = eventGroup;
            this.environment = environment;
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
            foreach (var item in environment.Locals)
            {
                LeftConditionComboBox.Items.Add(item.Key);
                RightConditionComboBox.Items.Add(item.Key);
                PropertyComboBox.Items.Add(item.Key);
                //Split struct into parts for convenience
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
        }
        private void LeftValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string input = LeftValue.Text.Trim();
            if (input == string.Empty)
                return;

            if (LeftConditionComboBox.SelectedItem != null)
            {
                string selection = (string)LeftConditionComboBox.SelectedItem;
                object value = environment.GetLocal(selection);
                if (value != null)
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        foreach (var item in properties[i])
                        {
                            if (selection == item.Name)
                            {
                                var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                if (!attribute.IsLegal(input, out value))
                                {
                                    MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                                    LeftValue.Text = string.Empty;
                                    return;
                                }
                                LeftValue.Text = value.ToString();
                                return;
                            }
                        }
                    }
                }
                if (value == null)
                {
                    value = environment.GetGlobal(selection);
                }
                if (value == null && selection.Contains('.'))
                {
                    value = 0.0f;
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        LeftValue.Text = string.Empty;
                    }
                }
            }
        }
        private void RightValue_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string input = RightValue.Text.Trim();
            if (input == string.Empty)
                return;

            if (RightConditionComboBox.SelectedItem != null)
            {
                string selection = (string)RightConditionComboBox.SelectedItem;
                object value = environment.GetLocal(selection);
                if (value != null)
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        foreach (var item in properties[i])
                        {
                            if (selection == item.Name)
                            {
                                var attribute = item.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                                if (!attribute.IsLegal(input, out value))
                                {
                                    MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                                    RightValue.Text = string.Empty;
                                    return;
                                }
                                RightValue.Text = value.ToString();
                                return;
                            }
                        }
                    }
                }
                if (value == null)
                {
                    value = environment.GetGlobal(selection);
                }
                if (value == null && selection.Contains('.'))
                {
                    value = 0.0f;
                }
                if (value != null)
                {
                    float testValue;
                    if (!float.TryParse(input, out testValue))
                    {
                        MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        RightValue.Text = string.Empty;
                    }
                }
            }
        }
        private void ChangeTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string input = ChangeTime.Text.Trim();
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
            {
                MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ChangeTime.Text = string.Empty;
            }
            else if (value <= 0)
                ChangeTime.Text = "1";
        }
        private void ExecuteTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string input = ExecuteTime.Text.Trim();
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
            {
                MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecuteTime.Text = string.Empty;
            }
            else if (value < 0)
                ExecuteTime.Text = "0";
        }
        private void LoopTime_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string input = LoopTime.Text.Trim();
            if (input == string.Empty)
                return;

            int value;
            if (!int.TryParse(input, out value))
            {
                MessageBox.Show((string)FindResource("ValueInvalid"), (string)FindResource("TipTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoopTime.Text = string.Empty;
            }
            else if (value <= 0)
                LoopTime.Text = "1";
        }
        private void Condition_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string input = Condition.Text.Trim();
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
                MessageBox.Show((string)FindResource("ExpressionInvalid"), (string)FindResource("TipTitle"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                Condition.Text = string.Empty;
            }
        }
        #endregion
    }
}
