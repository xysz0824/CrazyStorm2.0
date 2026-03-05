/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class MoveLayerCommand : Command
    {
        int direction;
        public MoveLayerCommand(int direction)
        {
            if (direction != -1 && direction != 1)
            {
                throw new ArgumentOutOfRangeException("direction");
            }
            this.direction = direction;
        }
        void MoveLayer(ParticleSystem selectedSystem, Layer selectedLayer, int step)
        {
            if (selectedSystem == null || selectedLayer == null) return;
            int index = selectedSystem.Layers.IndexOf(selectedLayer);
            if (index < 0) return;
            int target = index + step;
            if (target < 0 || target >= selectedSystem.Layers.Count) return;

            var temp = selectedSystem.Layers[index];
            selectedSystem.Layers[index] = selectedSystem.Layers[target];
            selectedSystem.Layers[target] = temp;
            for (int i = 0; i < selectedSystem.Layers.Count; ++i)
            {
                selectedSystem.Layers[i].SetComponentsID(i);
            }
        }
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            if (History[0] == null) History[0] = Parameter[0] as ParticleSystem;
            if (History[1] == null) History[1] = Parameter[1] as Layer;
            MoveLayer(History[0] as ParticleSystem, History[1] as Layer, direction);
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            MoveLayer(History[0] as ParticleSystem, History[1] as Layer, -direction);
        }
    }
}
