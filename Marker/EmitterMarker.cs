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
    class EmitterMarker : IComponentMark
    {
        public virtual void Draw(Canvas canvas, Component component, int x, int y)
        {
            var multiEmitter = component as Emitter;
            var left = multiEmitter.EmitAngle - multiEmitter.EmitRange / 2;
            var right = multiEmitter.EmitAngle + multiEmitter.EmitRange / 2;
            DrawHelper.DrawFan(canvas, x, y, 60, right, left, Colors.Red, 0.5f);
            var step = multiEmitter.EmitRange / (multiEmitter.EmitCount);
            var angle = left - step / 2;
            for (float i = 0; i < multiEmitter.EmitCount; ++i)
            {
                angle += step;
                DrawHelper.DrawArrow(canvas, x, y, 40, 2, angle, Colors.Aqua, 1);
            }
        }
    }
}
