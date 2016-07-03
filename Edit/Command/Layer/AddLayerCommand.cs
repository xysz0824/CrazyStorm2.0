/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
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
            var selectedParticle = Parameter[0] as ParticleSystem;
            if (History[0] == null)
            {
                var index = selectedParticle.LayerIndex;
                var newLayer = new Layer("Layer" + index);
                newLayer.Color = (LayerColor)((index + 1) % 7);
                History[0] = newLayer;
                selectedParticle.AddLayer(newLayer);
            }
            else
                selectedParticle.AddLayer(History[0] as Layer);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            selectedParticle.DeleteLayer(History[0] as Layer);
        }
    }
}
