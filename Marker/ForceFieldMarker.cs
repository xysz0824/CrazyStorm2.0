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
    class ForceFieldMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var force = component as ForceField;
            if (force.Shape == Core.Shape.Rectangle)
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
            else if (force.Shape == Core.Shape.Ellipse)
            {
                var rect = new Ellipse();
                rect.Width = force.HalfWidth * 2;
                rect.Height = force.HalfHeight * 2;
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.Opacity = 0.5f;
                rect.SetValue(Canvas.LeftProperty, (double)x - force.HalfWidth);
                rect.SetValue(Canvas.TopProperty, (double)y - force.HalfHeight);
                canvas.Children.Add(rect);
            }
            switch (force.ForceType)
            {
                case ForceType.Direction:
                    DrawHelper.DrawArrow(canvas, x, y, 40, 2, force.DirectionAngle, Colors.Yellow, 1);
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
