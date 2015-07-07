/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace CrazyStorm
{
    class PropertyPanelHelper
    {
        public static void UpdateProperty(PropertyContainer container, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var property = e.Row.Item as Property;
                var presenter = VisualHelper.GetVisualChild<DataGridCellsPresenter>(e.Row);
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1);
                var newValue = (e.EditingElement as TextBox).Text;
                var attribute = property.Info.GetCustomAttributes(false)[0] as PropertyAttribute;
                object value;
                //TODO : Expression check.
                if (attribute.IsLegal(newValue, out value))
                {
                    property.Info.GetSetMethod().Invoke(container, new object[] { value });
                    var result = property.Info.GetGetMethod().Invoke(container, null).ToString();
                    container.Properties[property.Info] = result;
                    property.Value = result;
                    cell.BorderBrush = new SolidColorBrush(Colors.Black);
                    cell.BorderThickness = new Thickness(1);
                }
                else
                {
                    cell.BorderBrush = new SolidColorBrush(Colors.Red);
                    cell.BorderThickness = new Thickness(2);
                }
            }
        }
    }
}
