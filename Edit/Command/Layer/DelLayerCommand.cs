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
    class DelLayerCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedLayer = parameter[1] as Layer;
            selectedBarrage.DeleteLayer(selectedLayer);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedLayer = parameter[1] as Layer;
            selectedBarrage.AddLayer(selectedLayer);
        }
    }
}
