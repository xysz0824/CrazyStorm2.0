/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class EventFieldMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var EventField = component as EventField;
            if (EventField.FieldShape == FieldShape.Rectangle)
            {
                var rect = new Rectangle();
                rect.Width = EventField.HalfWidth * 2;
                rect.Height = EventField.HalfHeight * 2;
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.Opacity = 0.5f;
                rect.SetValue(Canvas.LeftProperty, (double)x - EventField.HalfWidth);
                rect.SetValue(Canvas.TopProperty, (double)y - EventField.HalfHeight);
                canvas.Children.Add(rect);
            }
            else if (EventField.FieldShape == FieldShape.Circle)
            {
                var ellipse = new Ellipse();
                ellipse.Width = EventField.HalfWidth * 2;
                ellipse.Height = EventField.HalfWidth * 2;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.Opacity = 0.5f;
                ellipse.SetValue(Canvas.LeftProperty, (double)x - EventField.HalfWidth);
                ellipse.SetValue(Canvas.TopProperty, (double)y - EventField.HalfWidth);
                canvas.Children.Add(ellipse);
            }
        }
    }
}
