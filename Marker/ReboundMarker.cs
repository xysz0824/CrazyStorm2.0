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
    class ReboundMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            var rebound = component as Rebound;
            var line = new Line();
            line.Stroke = new SolidColorBrush(Colors.Red);
            line.StrokeThickness = 6;
            line.Opacity = 0.5f;
            line.X1 = x + rebound.Length * Math.Cos(rebound.Rotation / 180 * Math.PI);
            line.Y1 = y + rebound.Length * Math.Sin(rebound.Rotation / 180 * Math.PI);
            line.X2 = x + rebound.Length * Math.Cos((rebound.Rotation + 180) / 180 * Math.PI);
            line.Y2 = y + rebound.Length * Math.Sin((rebound.Rotation + 180) / 180 * Math.PI);
            canvas.Children.Add(line);
        }
    }
}
