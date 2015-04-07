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
    class AddLayerCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            if (history[0] == null)
            {
                var newLayer = new Layer("New Layer");
                history[0] = newLayer;
                selectedBarrage.AddLayer(newLayer);
            }
            else
                selectedBarrage.AddLayer(history[0] as Layer);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            selectedBarrage.DeleteLayer(history[0] as Layer);
        }
    }
}
