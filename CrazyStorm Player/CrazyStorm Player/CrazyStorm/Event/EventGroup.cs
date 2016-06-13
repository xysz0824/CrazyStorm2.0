using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm_Player.CrazyStorm;

namespace CrazyStorm_Player.CrazyStorm
{
    class EventGroup : IPlayData
    {
        public VMInstruction[] Condition { get; set; }
        public IList<EventInfo> Events { get; set; }
        public EventGroup()
        {
            Events = new List<EventInfo>();
        }
        public void LoadPlayData(BinaryReader reader)
        {
            using (BinaryReader eventGroupReader = PlayDataHelper.GetBlockReader(reader))
            {
                //compiledCondition
                int length = eventGroupReader.ReadInt32();
                if (length > 0)
                    Condition = VM.Decode(eventGroupReader.ReadBytes(length));
            
                //compiledEvents
                while (!PlayDataHelper.EndOfReader(eventGroupReader))
                {
                    length = eventGroupReader.ReadInt32();
                    Events.Add(EventHelper.BuildFromPlayData(eventGroupReader.ReadBytes(length)));
                }
            }
        }
        public void Execute(PropertyContainer propertyContainer)
        {
            VM.Execute(Condition);
            if (VM.PopBool())
            {
                for (int i = 0; i < Events.Count; ++i)
                    EventHelper.Execute(propertyContainer, Events[i]);
            }
        }
    }
}
