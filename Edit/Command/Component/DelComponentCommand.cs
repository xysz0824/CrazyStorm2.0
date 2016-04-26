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
            var selectedParticle = Parameter[0] as ParticleSystem;
            var selectedComponents = Parameter[1] as List<Component>;
            List<Component> temp;
            if (History[0] == null)
            {
                //Be careful that the reference of list could be modified outside, 
                //so there should copy it.
                temp = new List<Component>();
                foreach (var item in selectedComponents)
                    temp.Add(item);
                History[0] = temp;
            }
            else
                temp = History[0] as List<Component>;
            
            var layers = new Dictionary<Layer, List<Component>>();
            foreach (var component in temp)
                foreach (var layer in selectedParticle.Layers)
                    if (layer.Components.Contains(component))
                    {
                        if (!layers.ContainsKey(layer))
                            layers[layer] = new List<Component>();
                        layers[layer].Add(component);
                        selectedParticle.DeleteComponentFromLayer(layer, component);
                        break;
                    }

            History[1] = layers;
            selectedComponents.Clear();
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            var selectedComponents = Parameter[1] as List<Component>;
            var temp = History[0] as List<Component>;
            var layers = History[1] as Dictionary<Layer, List<Component>>;
            foreach (var component in temp)
                foreach (var layer in layers)
                    if (layer.Value.Contains(component))
                    {
                        selectedParticle.AddComponentToLayer(layer.Key, component);
                        break;
                    }

            selectedComponents.Clear();
        }
    }
}
