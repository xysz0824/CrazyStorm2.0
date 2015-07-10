using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace CrazyStorm
{
    public class PropertyPanelItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyInfo info;
        private string name;
        private string value;

        public PropertyInfo Info
        {
            get { return info; }
            set { info = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }
    }
}
