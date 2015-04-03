/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    class DelComponentCommand : Command 
    {
        public override void Do(CommandStack stack, params object[] parameter)
        {
            base.Do(stack, parameter);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedComponents = parameter[1] as List<Component>;
            foreach (var component in selectedComponents)
                foreach (var layer in selectedBarrage.Layers)
                    if (layer.Components.Contains(component))
                    {
                        selectedBarrage.DeleteComponentFromLayer(layer, component);
                        break;
                    }

            selectedComponents.Clear();
        }
        public override void Undo(CommandStack stack, params object[] parameter)
        {
            base.Undo(stack, parameter);

        }
    }
}
