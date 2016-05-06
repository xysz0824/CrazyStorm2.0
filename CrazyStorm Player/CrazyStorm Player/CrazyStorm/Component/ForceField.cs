using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    public enum ForceType
    {
        Direction,
        Inner,
        Outer
    }
    class ForceField : Component
    {
        public float HalfWidth { get; set; }
        public float HalfHeight { get; set; }
        public FieldShape Shape { get; set; }
        public Reach Reach { get; set; }
        public string TargetName { get; set; }
        public float Force { get; set; }
        public float Direction { get; set; }
        public ForceType ForceType { get; set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader forceFieldReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(forceFieldReader))
                {
                    HalfWidth = dataReader.ReadSingle();
                    HalfHeight = dataReader.ReadSingle();
                    Shape = PlayDataHelper.ReadEnum<FieldShape>(dataReader);
                    Reach = PlayDataHelper.ReadEnum<Reach>(dataReader);
                    TargetName = PlayDataHelper.ReadString(dataReader);
                    Force = dataReader.ReadSingle();
                    Direction = dataReader.ReadSingle();
                    ForceType = PlayDataHelper.ReadEnum<ForceType>(dataReader);
                }
            }
        }
    }
}
