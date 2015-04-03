using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    class NewLayerCommand : Command
    {
        public override void Do(CommandStack stack, params object[] parameter)
        {
            base.Do(stack, parameter);
            var selectedBarrage = parameter[0] as Barrage;
            selectedBarrage.AddLayer(new Layer("New Layer"));
        }
        public override void Undo(CommandStack stack, params object[] parameter)
        {
            base.Undo(stack, parameter);

        }
    }
}
