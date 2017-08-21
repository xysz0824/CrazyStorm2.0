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
    class DelLayerCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            var selectedLayer = Parameter[1] as Layer;
            selectedParticle.DeleteLayer(selectedLayer);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            var selectedLayer = Parameter[1] as Layer;
            selectedParticle.AddLayer(selectedLayer);
        }
    }
}
