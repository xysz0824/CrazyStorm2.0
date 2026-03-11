/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace CrazyStorm
{
    public enum PropertyEditorKind
    {
        Text,
        EnumCombo,
        BoolCheckBox,
        ParticleTypeCombo,
        ParticleColorCombo,
        EventFieldMaskTypeCombo,
        EmitterMaskTypeCombo,
    }
    public enum PropertyPseudoKind
    {
        None,
        ParticleType,
        ParticleColor,
        EventFieldMaskType,
        EmitterMaskType,
    }
    public class PropertyGridItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyInfo info;
        private string displayName;
        private string displayValue;
        private bool readOnly;
        private bool boolValue;
        private IList<string> itemsSource;
        private PropertyEditorKind editorKind;
        private bool isParticlePseudoProperty;
        private PropertyPseudoKind pseudoPropertyKind;

        public PropertyInfo Info
        {
            get { return info; }
            set { info = value; }
        }
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }
        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                displayValue = value;
                OnPropertyChanged("DisplayValue");
            }
        }
        public bool ReadOnly
        {
            get { return readOnly; }
            set
            {
                readOnly = value;
                OnPropertyChanged("ReadOnly");
            }
        }
        public bool BoolValue
        {
            get { return boolValue; }
            set
            {
                boolValue = value;
                OnPropertyChanged("BoolValue");
            }
        }
        public IList<string> ItemsSource
        {
            get { return itemsSource; }
            set
            {
                itemsSource = value;
                OnPropertyChanged("ItemsSource");
            }
        }
        public PropertyEditorKind EditorKind
        {
            get { return editorKind; }
            set
            {
                editorKind = value;
                OnPropertyChanged("EditorKind");
            }
        }
        public bool IsParticlePseudoProperty
        {
            get { return isParticlePseudoProperty; }
            set
            {
                isParticlePseudoProperty = value;
                OnPropertyChanged("IsParticlePseudoProperty");
            }
        }
        public PropertyPseudoKind PseudoPropertyKind
        {
            get { return pseudoPropertyKind; }
            set
            {
                pseudoPropertyKind = value;
                OnPropertyChanged("PseudoPropertyKind");
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
