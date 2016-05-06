using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    public enum FieldShape
    {
        Rectangle,
        Ellipse
    }
    public enum Reach
    {
        All,
        Layer,
        Name
    }
    class EventField : Component
    {
        public float HalfWidth { get; set; }
        public float HalfHeight { get; set; }
        public FieldShape Shape { get; set; }
        public Reach Reach { get; set; }
        public string TargetName { get; set; }
        public IList<EventGroup> EventFieldEventGroups { get; private set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader eventFieldReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(eventFieldReader))
                {
                    HalfWidth = dataReader.ReadSingle();
                    HalfHeight = dataReader.ReadSingle();
                    Shape = PlayDataHelper.ReadEnum<FieldShape>(dataReader);
                    Reach = PlayDataHelper.ReadEnum<Reach>(dataReader);
                    TargetName = PlayDataHelper.ReadString(dataReader);
                }
                //eventFieldEventGroups
                //TODO
            }
        }
    }
}
