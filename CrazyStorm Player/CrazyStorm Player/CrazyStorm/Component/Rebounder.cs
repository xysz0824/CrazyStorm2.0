using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    public enum RebounderShape
    {
        Line,
        Circle
    }
    class Rebounder : Component
    {
        public int Size { get; set; }
        public RebounderShape Shape { get; set; }
        public float Rotation { get; set; }
        public int CollisionTime { get; set; }
        public IList<EventGroup> RebounderEventGroups { get; private set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader rebounderReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(rebounderReader))
                {
                    Size = dataReader.ReadInt32();
                    Shape = PlayDataHelper.ReadEnum<RebounderShape>(dataReader);
                    Rotation = dataReader.ReadSingle();
                    CollisionTime = dataReader.ReadInt32();
                }
                //rebounderEventGroups
                PlayDataHelper.LoadObjectList(RebounderEventGroups, rebounderReader);
            }
        }
    }
}
