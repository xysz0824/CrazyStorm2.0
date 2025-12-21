/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace CrazyStorm
{
    public class PropertyGridItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyInfo info;
        private string displayName;
        private string displayValue;

        public PropertyInfo Info
        {
            get { return info; }
            set { info = value; }
        }
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                this.displayValue = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayValue"));
            }
        }
    }
}
