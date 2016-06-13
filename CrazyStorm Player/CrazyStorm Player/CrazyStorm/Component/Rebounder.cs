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
        public RebounderShape RebounderShape { get; set; }
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
                    RebounderShape = PlayDataHelper.ReadEnum<RebounderShape>(dataReader);
                    Rotation = dataReader.ReadSingle();
                    CollisionTime = dataReader.ReadInt32();
                }
                //rebounderEventGroups
                PlayDataHelper.LoadObjectList(RebounderEventGroups, rebounderReader);
            }
        }
        public override bool PushProperty(string propertyName)
        {
            base.PushProperty(propertyName);
            throw new NotImplementedException();
        }
        public override bool SetProperty(string propertyName)
        {
            base.SetProperty(propertyName);
            throw new NotImplementedException();
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            //TODO
            List<ParticleBase> results = ParticleManager.SearchByRect(
                (int)(Position.x - Size), (int)(Position.x + Size),
                (int)(Position.y - Size), (int)(Position.y + Size));
            foreach (ParticleBase particleBase in results)
            {
                if (particleBase.IgnoreRebound)
                    continue;

                switch (RebounderShape)
                {
                    case RebounderShape.Line:
                        break;
                    case RebounderShape.Circle:
                        break;
                }
            }
            //for (int i = 0; i < RebounderEventGroups.Count; ++i)
            //    RebounderEventGroups[i].Execute();

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as Rebounder;
            Size = initialState.Size;
            RebounderShape = initialState.RebounderShape;
            Rotation = initialState.Rotation;
            CollisionTime = initialState.CollisionTime;
        }
    }
}
