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
    class CopyLayerCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            if (History[0] == null)
            {
                var selectedLayer = Parameter[1] as Layer;
                var clone = selectedLayer.Clone() as Layer;
                foreach (var component in clone.Components)
                    component.RebuildReferenceFromCollection(clone.Components);

                foreach (var component in clone.Components)
                {
                    component.ID = selectedParticle.GetComponentIndex();
                    selectedParticle.GetAndIncreaseComponentIndex(component.GetType().ToString());
                }
                selectedParticle.AddLayer(clone);
                History[0] = clone;
            }
            else
            {
                var clone = History[0] as Layer;
                selectedParticle.AddLayer(clone);
            }
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            var clone = History[0] as Layer;
            selectedParticle.DeleteLayer(clone);
        }
    }
}
