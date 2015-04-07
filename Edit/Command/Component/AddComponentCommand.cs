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
    class AddComponentCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedLayer = parameter[1] as Layer;
            var aimComponent = parameter[2] as Component;
            selectedBarrage.AddComponentToLayer(selectedLayer, aimComponent);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedLayer = parameter[1] as Layer;
            var aimComponent = parameter[2] as Component;
            selectedBarrage.DeleteComponentFromLayer(selectedLayer, aimComponent);
        }
    }
}
