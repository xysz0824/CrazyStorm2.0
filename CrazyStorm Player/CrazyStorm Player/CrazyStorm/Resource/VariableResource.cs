using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class VariableResource : Resource
    {
        public float Value { get; set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader variableResourceReader = PlayDataHelper.GetBlockReader(reader))
            {
                Value = variableResourceReader.ReadSingle();
            }
        }
    }
}
