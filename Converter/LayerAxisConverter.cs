/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace CrazyStorm
{
    public class LayerAxisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new Rect((float)((int)value / (float)Enum.GetNames(typeof(LayerColor)).Length) + 0.01f, 0.01f, 0.1f, 0.99f);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
