using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    class DelLayerCommand : Command
    {
        public override void Do(CommandStack stack, params object[] parameter)
        {
            base.Do(stack, parameter);
            var selectedBarrage = parameter[0] as Barrage;
            var selectedLayer = parameter[1] as Layer;
            selectedBarrage.DeleteLayer(selectedLayer);
        }
        public override void Undo(CommandStack stack, params object[] parameter)
        {
            base.Undo(stack, parameter);

        }
    }
}
