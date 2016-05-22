using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Expression;

namespace CrazyStorm_Player.CrazyStorm
{
    class EventGroup : IPlayData
    {
        public VMInstruction[] Condition { get; set; }
        public IList<EventInfo> Events { get; set; }
        public void LoadPlayData(BinaryReader reader)
        {

        }
    }
}
