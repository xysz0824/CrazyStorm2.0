/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
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
    class RebounderMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var Rebounder = component as Rebounder;
            if (Rebounder.RebounderShape == RebounderShape.Line)
            {
                var line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Red);
                line.StrokeThickness = 6;
                line.Opacity = 0.5f;
                line.X1 = x + Rebounder.Size * Math.Cos(Rebounder.Rotation / 180 * Math.PI);
                line.Y1 = y + Rebounder.Size * Math.Sin(Rebounder.Rotation / 180 * Math.PI);
                line.X2 = x + Rebounder.Size * Math.Cos((Rebounder.Rotation + 180) / 180 * Math.PI);
                line.Y2 = y + Rebounder.Size * Math.Sin((Rebounder.Rotation + 180) / 180 * Math.PI);
                canvas.Children.Add(line);
            }
            else if (Rebounder.RebounderShape == RebounderShape.Circle)
            {
                var ellipse = new Ellipse();
                ellipse.Width = Rebounder.Size * 2;
                ellipse.Height = Rebounder.Size * 2;
                ellipse.Stroke = new SolidColorBrush(Colors.Red);
                ellipse.StrokeThickness = 6;
                ellipse.Opacity = 0.5f;
                ellipse.SetValue(Canvas.LeftProperty, (double)x - Rebounder.Size);
                ellipse.SetValue(Canvas.TopProperty, (double)y - Rebounder.Size);
                canvas.Children.Add(ellipse);
            }
        }
    }
}
