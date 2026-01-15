/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
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
            var field = component as EventField;
            if (field.FieldShape == FieldShape.Rectangle)
            {
                var rect = new Rectangle();
                rect.Width = field.HalfWidth * 2;
                rect.Height = field.HalfHeight * 2;
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.Opacity = 0.3f;
                rect.RenderTransformOrigin = new Point(0.5f, 0.5f);
                rect.RenderTransform = new RotateTransform(field.Rotation);
                rect.SetValue(Canvas.LeftProperty, (double)x - field.HalfWidth);
                rect.SetValue(Canvas.TopProperty, (double)y - field.HalfHeight);
                canvas.Children.Add(rect);
            }
            else if (field.FieldShape == FieldShape.Circle)
            {
                var ellipse = new Ellipse();
                ellipse.Width = field.HalfWidth * 2;
                ellipse.Height = field.HalfWidth * 2;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.Opacity = 0.3f;
                ellipse.RenderTransformOrigin = new Point(0.5f, 0.5f);
                ellipse.RenderTransform = new RotateTransform(field.Rotation);
                ellipse.SetValue(Canvas.LeftProperty, (double)x - field.HalfWidth);
                ellipse.SetValue(Canvas.TopProperty, (double)y - field.HalfHeight);
                canvas.Children.Add(ellipse);
            }
        }
    }
}
