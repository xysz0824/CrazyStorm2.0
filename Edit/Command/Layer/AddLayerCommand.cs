/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class AddLayerCommand : Command
    {
        string defaultLayerName;
        public AddLayerCommand(string defaultLayerName)
        {
            this.defaultLayerName = defaultLayerName;
        }
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            if (History[0] == null)
            {
                var index = selectedParticle.LayerIndex;
                var newLayer = new Layer(defaultLayerName);
                newLayer.Color = (LayerColor)((index + 1) % Enum.GetNames(typeof(LayerColor)).Length);
                History[0] = newLayer;
                selectedParticle.InsertLayer(newLayer);
            }
            else
                selectedParticle.InsertLayer(History[0] as Layer);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedParticle = Parameter[0] as ParticleSystem;
            selectedParticle.DeleteLayer(History[0] as Layer);
        }
    }
}
