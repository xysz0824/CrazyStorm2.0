using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrazyStorm.CoreLibrary;

namespace CrazyStorm
{
    class SetLayerCommand : Command
    {
        public override void Do(CommandStack stack, params object[] parameter)
        {
            base.Do(stack, parameter);
            var layer = parameter[0] as Layer;
            layer.Color = (LayerColor)parameter[1];
            layer.BeginFrame = (int)parameter[2];
            layer.TotalFrame = (int)parameter[3];
            layer.Name = (string)parameter[4];
        }
        public override void Undo(CommandStack stack, params object[] parameter)
        {
            base.Undo(stack, parameter);
        }
    }
}
