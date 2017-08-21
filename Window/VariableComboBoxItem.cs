/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm
{
    class VariableComboBoxItem
    {
        private string name;
        private string displayName;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
    }
}
