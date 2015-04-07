/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class SetLayerCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var layer = parameter[0] as Layer;
            history[1] = layer.Color;
            layer.Color = (LayerColor)parameter[1];
            history[2] = layer.BeginFrame;
            layer.BeginFrame = (int)parameter[2];
            history[3] = layer.TotalFrame;
            layer.TotalFrame = (int)parameter[3];
            history[4] = layer.Name;
            layer.Name = (string)parameter[4];
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var layer = parameter[0] as Layer;
            layer.Color = (LayerColor)history[1];
            layer.BeginFrame = (int)history[2];
            layer.TotalFrame = (int)history[3];
            layer.Name = (string)history[4];
        }
    }
}
