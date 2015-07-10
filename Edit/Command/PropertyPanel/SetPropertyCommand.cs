/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetPropertyCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var container = Parameter[0] as PropertyContainer;
            var property = Parameter[1] as PropertyPanelItem;
            var cell = Parameter[2] as DataGridCell;
            var newValue = Parameter[3] as string;
            var attribute = Parameter[4] as PropertyAttribute;
            var updateFunc = Parameter[5] as Action;
            if (History[0] == null)
                History[0] = property.Value;
            History[1] = SetProperty(container, property.Info, cell, newValue, attribute, updateFunc);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var container = Parameter[0] as PropertyContainer;
            var property = Parameter[1] as PropertyPanelItem;
            var cell = Parameter[2] as DataGridCell;
            var newValue = History[0] as string;
            var attribute = Parameter[4] as PropertyAttribute;
            var updateFunc = Parameter[5] as Action;
            SetProperty(container, property.Info, cell, newValue, attribute, updateFunc);
            //If set invalid value, pop undo stack to avoid for redoing again.
            if (History[1] != null && !(bool)History[1])
                stack.UndoPop();
        }
        bool SetProperty(PropertyContainer container, 
            PropertyInfo propertyInfo, DataGridCell cell, string newValue, PropertyAttribute attribute, Action updateFunc)
        {
            object value;
            //TODO : Expression check.
            if (attribute.IsLegal(newValue, out value))
            {
                propertyInfo.GetSetMethod().Invoke(container, new object[] { value });
                cell.BorderThickness = new Thickness(0);
                updateFunc();
                return true;
            }
            else
            {
                cell.BorderBrush = new SolidColorBrush(Colors.Red);
                cell.BorderThickness = new Thickness(2);
                return false;
            }
        }
    }
}
