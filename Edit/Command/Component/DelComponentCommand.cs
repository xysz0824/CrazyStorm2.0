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
    class DelComponentCommand : Command 
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedComponents = parameter[1] as List<Component>;
            List<Component> temp;
            if (history[0] == null)
            {
                //Be careful that the reference of list could be modified outside, 
                //so there should copy it.
                temp = new List<Component>();
                foreach (var item in selectedComponents)
                    temp.Add(item);
                history[0] = temp;
            }
            else
                temp = history[0] as List<Component>;

            var temp2 = new Dictionary<Layer, List<Component>>();
            foreach (var component in temp)
                foreach (var layer in selectedBarrage.Layers)
                    if (layer.Components.Contains(component))
                    {
                        if (!temp2.ContainsKey(layer))
                            temp2[layer] = new List<Component>();
                        temp2[layer].Add(component);
                        selectedBarrage.DeleteComponentFromLayer(layer, component);
                        break;
                    }

            history[1] = temp2;
            selectedComponents.Clear();
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedComponents = parameter[1] as List<Component>;
            var temp = history[0] as List<Component>;
            var layers = history[1] as Dictionary<Layer, List<Component>>;
            foreach (var component in temp)
                foreach (var layer in layers)
                    if (layer.Value.Contains(component))
                    {
                        selectedBarrage.AddComponentToLayer(layer.Key, component);
                        break;
                    }

            selectedComponents.Clear();
        }
    }
}
