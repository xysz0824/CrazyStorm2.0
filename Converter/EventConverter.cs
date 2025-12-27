/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CrazyStorm
{
    public class EventConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var originalEvent = (string)value;
            var info = EventHelper.SplitEvent(originalEvent);
            if (info.condition != null) info.condition = ExpressionHelper.Translate(info.condition);
            if (!info.isSpecialEvent)
            {
                info.resultProperty = ExpressionHelper.TranslateProperty(info.resultProperty);
                info.changeType = ExpressionHelper.FindTranslation(info.changeType);
                info.changeMode = ExpressionHelper.FindTranslation(info.changeMode);
                info.resultValue = ExpressionHelper.Translate(info.resultValue);
            }
            else
            {
                info.specialEvent = ExpressionHelper.FindTranslation(info.specialEvent);
            }
            return EventHelper.BuildEvent(info, false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
