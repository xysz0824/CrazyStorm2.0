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
    class MaskMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var mask = component as Mask;
            if (mask.Shape == Core.Shape.Rectangle)
            {
                var rect = new Rectangle();
                rect.Width = mask.HalfWidth * 2;
                rect.Height = mask.HalfHeight * 2;
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.Opacity = 0.5f;
                rect.SetValue(Canvas.LeftProperty, (double)x - mask.HalfWidth);
                rect.SetValue(Canvas.TopProperty, (double)y - mask.HalfHeight);
                canvas.Children.Add(rect);
            }
            else if (mask.Shape == Core.Shape.Ellipse)
            {
                var rect = new Ellipse();
                rect.Width = mask.HalfWidth * 2;
                rect.Height = mask.HalfHeight * 2;
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.Opacity = 0.5f;
                rect.SetValue(Canvas.LeftProperty, (double)x - mask.HalfWidth);
                rect.SetValue(Canvas.TopProperty, (double)y - mask.HalfHeight);
                canvas.Children.Add(rect);
            }
        }
    }
}
