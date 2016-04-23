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
    class RebounderMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var Rebounder = component as Rebounder;
            var line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Red);
            line.StrokeThickness = 6;
            line.Opacity = 0.5f;
            line.X1 = x + Rebounder.Length * Math.Cos(Rebounder.Rotation / 180 * Math.PI);
            line.Y1 = y + Rebounder.Length * Math.Sin(Rebounder.Rotation / 180 * Math.PI);
            line.X2 = x + Rebounder.Length * Math.Cos((Rebounder.Rotation + 180) / 180 * Math.PI);
            line.Y2 = y + Rebounder.Length * Math.Sin((Rebounder.Rotation + 180) / 180 * Math.PI);
            canvas.Children.Add(line);
        }
    }
}
