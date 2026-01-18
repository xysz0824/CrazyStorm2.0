/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace CrazyStorm
{
    public partial class ParticleSystemSetting : Window
    {
        #region Private Members
        File file;
        ParticleSystem selectedParticle;
        ParticleType selectedType;
        TabItem selectedTab;
        List<ParticleType> types;
        List<FileResource> sounds;
        #endregion

        #region Constructor
        public ParticleSystemSetting(File file, ParticleSystem particleSystem, TabItem selectedTab)
        {
            this.file = file;
            this.selectedParticle = particleSystem;
            this.selectedTab = selectedTab;
            InitializeComponent();
            InitializeDataBinding();
        }
        #endregion

        #region Private Methods
        void InitializeDataBinding()
        {
            ParticleSystemName.DataContext = selectedParticle;
            FirstAsTop.IsChecked = selectedParticle.OrderType == OrderType.FirstAsTop;
            LastAsTop.IsChecked = selectedParticle.OrderType == OrderType.LastAsTop;
            TypeList.ItemsSource = selectedParticle.CustomTypes;
            file.UpdateResource();
            //Load images.
            foreach (var image in file.Images)
            {
                image.CheckValid();
                ImageCombo.Items.Add(image);
            }
            types = new List<ParticleType>();
            foreach (var type in ParticleType.DefaultTypes) types.Add(type);
            foreach (var type in selectedParticle.CustomTypes) types.Add(type);
            sounds = new List<FileResource>();
            foreach (var sound in file.Sounds) sounds.Add(sound);
            //Load type sounds
            foreach (var typeSound in selectedParticle.TypeSoundMap)
            {
                var type = types.FirstOrDefault((item) => item.ID == typeSound.Key);
                var sound = sounds.FirstOrDefault((item) => item.ID == typeSound.Value);
                var typeSoundPanel = new TypeSoundPanel(types, sounds, selectedParticle, type, type.Color, sound);
                TypeSoundList.Items.Add(typeSoundPanel);
                typeSoundPanel.CanChangeMap = true;
            }
        }
        void UpdateColor()
        {
            for (int i = 0; i < ColorPanel.Children.Count;++i )
            {
                var item = ColorPanel.Children[i] as Label;
                if (selectedType != null && (int)selectedType.Color == i)
                    item.BorderThickness = new Thickness(1);
                else
                    item.BorderThickness = new Thickness(0);
            }
        }
        void UpdatePreview()
        {
            if (Setting.DataContext == null) return;
            int frame = 0, width = 0, height = 0, startPointX = 0, startPointY = 0;
            if (!int.TryParse(Frames.Text, out frame) || !int.TryParse(RectWidth.Text, out width) ||
                !int.TryParse(RectHeight.Text, out height) || !int.TryParse(StartPointX.Text, out startPointX) ||
                !int.TryParse(StartPointY.Text, out startPointY)) return;
            for (int i = 0; i < Preview.Children.Count;++i)
            {
                if (((FrameworkElement)Preview.Children[i]).Name == "FrameRect")
                {
                    Preview.Children.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 1;i < frame;++i)
            {
                var rect = new Rectangle();
                rect.Name = "FrameRect";
                rect.Width = width;
                rect.Height = height;
                rect.Stroke = new SolidColorBrush(Colors.Red);
                Preview.Children.Add(rect);
                Canvas.SetLeft(rect, startPointX + i * width);
                Canvas.SetTop(rect, startPointY);
                rect.Opacity = 0.8f;
            }
        }
        #endregion

        #region Window EventHandlers
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ParticleSystemName.Text))
            {
                MessageBox.Show((string)FindResource("ParticleSystemNameCanNotBeEmptyStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            selectedParticle.Name = ParticleSystemName.Text;
            selectedTab.Header = ParticleSystemName.Text;
            MessageBox.Show((string)FindResource("ParticleSystemNameSuccessfullyChangedStr"), (string)FindResource("TipTitleStr"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedType = e.AddedItems[0] as ParticleType;
                Setting.IsEnabled = true;
                Setting.DataContext = selectedType;
            }
            else
            {
                selectedType = null;
                Setting.IsEnabled = false;
                Setting.DataContext = null;
            }
            UpdateColor();
        }
        private void AddNewType_Click(object sender, RoutedEventArgs e)
        {
            selectedParticle.CustomTypes.Add(new ParticleType(selectedParticle.CustomTypeIndex,
                (string)FindResource("ParticleTypeStr")));
            DelType.IsEnabled = true;
        }
        private void ColorPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var selectedColor = e.Source as Label;
            for (int i = 0; i < ColorPanel.Children.Count; ++i)
            {
                var item = ColorPanel.Children[i] as Label;
                if (selectedColor == item)
                {
                    selectedType.Color = (ParticleColor)i;
                    UpdateColor();
                }
            }
        }
        private void DeleteType_Click(object sender, RoutedEventArgs e)
        {
            selectedParticle.CustomTypes.Remove(selectedType);
            DelType.IsEnabled = selectedParticle.CustomTypes.Count > 0;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddType.Focus();
            }
        }
        private void ImageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageCombo.SelectedItem == null)
            {
                Image.Source = null;
                Image.Width = Image.Height = 0;
            }
            else
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri((ImageCombo.SelectedItem as FileResource).AbsolutePath));
                    Image.Source = bitmap;
                    Image.Width = bitmap.PixelWidth;
                    Image.Height = bitmap.PixelHeight;
                }
                catch
                {
                    Image.Source = null;
                }
                //Check if width or height is 2 to the power of n
                if (((int)Image.Width & ((int)Image.Width - 1)) != 0 ||
                    ((int)Image.Height & ((int)Image.Height - 1)) != 0)
                {
                    MessageBox.Show((string)FindResource("ImageSizeWaringStr"), (string)FindResource("TipTitleStr"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }
        private void FirstAsTop_Checked(object sender, RoutedEventArgs e)
        {
            selectedParticle.OrderType = OrderType.FirstAsTop;
        }
        private void LastAsTop_Checked(object sender, RoutedEventArgs e)
        {
            selectedParticle.OrderType = OrderType.LastAsTop;
        }
        private void TypeSoundList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeSoundList.SelectedItem != null) DelTypeSound.IsEnabled = true;
        }
        private void AddTypeSound_Click(object sender, RoutedEventArgs e)
        {
            var newTypeSoundPanel = new TypeSoundPanel(types, sounds, selectedParticle, null, default, null);
            TypeSoundList.Items.Add(newTypeSoundPanel);
            newTypeSoundPanel.CanChangeMap = true;
            DelTypeSound.IsEnabled = true;
        }
        private void DelTypeSound_Click(object sender, RoutedEventArgs e)
        {
            var selectedPanel = TypeSoundList.SelectedItem as TypeSoundPanel;
            if (selectedPanel != null)
            {
                var selectedType = selectedPanel.TypeCombo.SelectedItem as ParticleType;
                if (selectedType != null) selectedParticle.TypeSoundMap.Remove(selectedType.ID);
                var index = TypeSoundList.SelectedIndex;
                TypeSoundList.Items.Remove(selectedPanel);
                TypeSoundList.SelectedIndex = index - 1;
            }
            DelTypeSound.IsEnabled = TypeSoundList.Items.Count > 0;
        }
        #endregion
    }
}
