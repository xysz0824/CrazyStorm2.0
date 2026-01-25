/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrazyStorm
{
    public partial class TypeSoundPanel : UserControl
    {
        #region Private Members
        Config config;
        List<ParticleType> types;
        List<FileResource> sounds;
        ParticleSystem selectedParticle;
        bool isPlaySound;
        #endregion

        #region Public Members
        public bool CanChangeMap { get; set; }
        #endregion

        #region Constructor
        public TypeSoundPanel(Config config, List<ParticleType> types, List<FileResource> sounds, ParticleSystem selectedParticle, ParticleType selectedType, 
            ParticleColor selectedColor, FileResource selectedSound)
        {
            this.config = config;
            this.types = types;
            this.sounds = sounds;
            this.selectedParticle = selectedParticle;
            InitializeComponent();
            LoadContent(selectedType, selectedColor, selectedSound);
        }
        #endregion

        #region Private Methods
        void LoadContent(ParticleType selectedType, ParticleColor selectedColor, FileResource selectedSound)
        {
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
            if (selectedType != null)
            {
                //Select specific type.
                foreach (var type in typesNorepeat)
                {
                    if (type.Name == selectedType.Name)
                    {
                        TypeCombo.SelectedItem = type;
                        break;
                    }
                }
                //Select specific color.
                InitializeColorCombo();
                for (int i = 0; i < ColorCombo.Items.Count; ++i)
                {
                    if (ExpressionHelper.Translate(selectedType.Color.ToString()) == (string)((ColorCombo.Items[i] as ComboBoxItem).Content))
                    {
                        ColorCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
            SoundCombo.ItemsSource = sounds;
            SoundCombo.SelectedItem = selectedSound;
        }
        void InitializeColorCombo()
        {
            ColorCombo.Items.Clear();
            if (TypeCombo.SelectedItem == null) return;
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
        #endregion

        #region Window EventHandlers
        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CanChangeMap) return;
            var selectedType = TypeCombo.SelectedItem as ParticleType;
            if (selectedType == null) return;
            var oldType = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as ParticleType : null;
            if (oldType != null && ColorCombo.SelectedIndex >= 0)
            {
                foreach (var type in types)
                {
                    if (type.Name == oldType.Name && ExpressionHelper.Translate(type.Color.ToString()) ==
                        (string)(ColorCombo.SelectedItem as ComboBoxItem).Content)
                    {
                        selectedParticle.TypeSoundMap.Remove(type.ID);
                        break;
                    }
                }
            }
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
                if (string.IsNullOrEmpty(config.TypeLibraryPath))
                {
                    var path = "pack://application:,,,/Images/barrages.png";
                    imageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                else
                {
                    var path = $"typelibrary\\{config.TypeLibraryPath}";
                    imageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Relative));
                }
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
        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CanChangeMap) return;
            var selectedType = TypeCombo.SelectedItem as ParticleType;
            if (selectedType == null || ColorCombo.SelectedItem == null) return;
            var oldColor = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as ComboBoxItem : null;
            if (oldColor != null)
            {
                foreach (var type in types)
                {
                    if (type.Name == selectedType.Name && ExpressionHelper.Translate(type.Color.ToString()) ==
                        (string)oldColor.Content)
                    {
                        selectedParticle.TypeSoundMap.Remove(type.ID);
                        break;
                    }
                }
            }
            if (SoundCombo.SelectedItem != null)
            {
                var sound = SoundCombo.SelectedItem as FileResource;
                foreach (var type in types)
                {
                    if (type.Name == selectedType.Name && ExpressionHelper.Translate(type.Color.ToString()) ==
                        (string)(ColorCombo.SelectedItem as ComboBoxItem).Content)
                    {
                        selectedParticle.TypeSoundMap[type.ID] = sound.ID;
                        break;
                    }
                }
            }
        }
        private void SoundCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorCombo_SelectionChanged(sender, e);
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
        #endregion
    }
}
