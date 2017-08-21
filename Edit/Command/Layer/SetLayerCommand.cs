/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
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
            var layer = Parameter[0] as Layer;
            History[1] = layer.Color;
            layer.Color = (LayerColor)Parameter[1];
            History[2] = layer.BeginFrame;
            layer.BeginFrame = (int)Parameter[2];
            History[3] = layer.TotalFrame;
            layer.TotalFrame = (int)Parameter[3];
            History[4] = layer.Name;
            layer.Name = (string)Parameter[4];
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var layer = Parameter[0] as Layer;
            layer.Color = (LayerColor)History[1];
            layer.BeginFrame = (int)History[2];
            layer.TotalFrame = (int)History[3];
            layer.Name = (string)History[4];
        }
    }
}
