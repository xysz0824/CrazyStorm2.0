/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
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
    class ForceFieldMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var force = component as ForceField;
            if (force.FieldShape == FieldShape.Rectangle)
            {
                var rect = new Rectangle();
                rect.Width = force.HalfWidth * 2;
                rect.Height = force.HalfHeight * 2;
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.Opacity = 0.5f;
                rect.SetValue(Canvas.LeftProperty, (double)x - force.HalfWidth);
                rect.SetValue(Canvas.TopProperty, (double)y - force.HalfHeight);
                canvas.Children.Add(rect);
            }
            else if (force.FieldShape == FieldShape.Circle)
            {
                var ellipse = new Ellipse();
                ellipse.Width = force.HalfWidth * 2;
                ellipse.Height = force.HalfWidth * 2;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.Opacity = 0.5f;
                ellipse.SetValue(Canvas.LeftProperty, (double)x - force.HalfWidth);
                ellipse.SetValue(Canvas.TopProperty, (double)y - force.HalfWidth);
                canvas.Children.Add(ellipse);
            }
            switch (force.ForceType)
            {
                case ForceType.Direction:
                    DrawHelper.DrawArrow(canvas, x, y, 40, 2, force.Direction, Colors.Yellow, 1);
                    break;
                case ForceType.Inner:
                    for (float i = 0; i < 360; i += 20)
                    {
                        int ax = x + (int)(80 * Math.Cos(i / 180 * Math.PI));
                        int ay = y + (int)(80 * Math.Sin(i / 180 * Math.PI));
                        DrawHelper.DrawArrow(canvas, ax, ay, 40, 2, i + 180, Colors.Yellow, 1);
                    }
                    break;
                case ForceType.Outer:
                    for (float i = 0; i < 360; i += 20)
                    {
                        int ax = x + (int)(40 * Math.Cos(i / 180 * Math.PI));
                        int ay = y + (int)(40 * Math.Sin(i / 180 * Math.PI));
                        DrawHelper.DrawArrow(canvas, ax, ay, 40, 2, i, Colors.Yellow, 1);
                    }
                    break;
            }
        }
    }
}
