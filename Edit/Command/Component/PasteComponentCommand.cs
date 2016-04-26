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
    class PasteComponentCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            var selectedLayer = Parameter[1] as Layer;
            var clipBoard = Parameter[2] as List<Component>;
            if (History[0] == null)
            {
                var clones = new List<Component>();
                foreach (var component in clipBoard)
                {
                    var clone = component.Clone() as Component;
                    clone.Selected = true;
                    selectedParticle.AddComponentToLayer(selectedLayer, clone);
                    clones.Add(clone);
                }
                History[0] = clones;
            }
            else
            {
                var clones = History[0] as List<Component>;
                foreach (var clone in clones)
                    selectedParticle.AddComponentToLayer(selectedLayer, clone);
            }
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            var selectedLayer = Parameter[1] as Layer;
            var clones = History[0] as List<Component>;
            foreach (var clone in clones)
                selectedParticle.DeleteComponentFromLayer(selectedLayer, clone);
        }
    }
}
