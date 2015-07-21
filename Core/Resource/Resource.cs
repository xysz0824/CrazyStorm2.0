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
    public class Resource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected string label;
        protected bool valid;
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
        public bool Valid { get { return valid; } }

        public Resource(string label)
        {
            this.label = label;
        }
        public virtual void CheckValid()
        {
        }
        public override string ToString()
        {
            return label;
        }
    }
}
