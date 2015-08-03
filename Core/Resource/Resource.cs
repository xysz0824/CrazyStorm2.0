/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace CrazyStorm.Core
{
    public abstract class Resource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected string label;
        protected bool isValid;
        public string Label
        {
            get { return label; }
            set
            {
                label = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Label"));
            }
        }
        public bool IsValid { get { CheckValid(); return isValid; } }

        public Resource(string label)
        {
            this.label = label;
        }

        public override string ToString()
        {
            return label;
        }

        public abstract void CheckValid();
    }
}
