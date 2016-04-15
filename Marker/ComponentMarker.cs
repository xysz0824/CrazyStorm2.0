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
    class ComponentMarker : IComponentMark
    {
        public void Draw(Canvas canvas, Component component, int x, int y)
        {
            if (component.Speed != 0)
                DrawHelper.DrawArrow(canvas, x, y, 60, 2, component.SpeedAngle, Colors.Red, 1);
            if (component.Acspeed != 0)
                DrawHelper.DrawArrow(canvas, x, y, 50, 2, component.AcspeedAngle, Colors.Violet, 1);
        }
    }
}
