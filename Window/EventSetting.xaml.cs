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
        #endregion

        #region Constructor
        public EventSetting(EventGroup eventGroup, Script.Environment environment)
        {
            this.eventGroup = eventGroup;
            this.environment = environment;
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
        }
        private void PlaySoundButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Visible;
            LoopPanel.Visibility = Visibility.Collapsed;
        }
        private void LoopButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Visible;
        }
        private void RecoverButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundPanel.Visibility = Visibility.Collapsed;
            LoopPanel.Visibility = Visibility.Collapsed;
        }
        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            eventGroup.Events.Add("aaaaaaaaaaaaaa");
        }
        private void AddSpecialEvent_Click(object sender, RoutedEventArgs e)
        {
            eventGroup.Events.Add("bbbbbbbbbbbbbb");
        }
        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            var item = EventList.SelectedItem;
            if (item != null)
                eventGroup.Events.Remove((string)item);
        }
        #endregion
    }
}
