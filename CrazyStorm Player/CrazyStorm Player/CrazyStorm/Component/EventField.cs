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
        public FieldShape FieldShape { get; set; }
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
                    FieldShape = PlayDataHelper.ReadEnum<FieldShape>(dataReader);
                    Reach = PlayDataHelper.ReadEnum<Reach>(dataReader);
                    TargetName = PlayDataHelper.ReadString(dataReader);
                }
                //eventFieldEventGroups
                PlayDataHelper.LoadObjectList(EventFieldEventGroups, eventFieldReader);
            }
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            //TODO
            List<ParticleBase> results = ParticleManager.ParticleQuadTree.SearchByRect(
                (int)(Position.x - HalfWidth), (int)(Position.x + HalfWidth),
                (int)(Position.y - HalfHeight), (int)(Position.y + HalfHeight));
            foreach (Particle particle in results)
            {
                if (particle.IgnoreMask)
                    continue;

                if (FieldShape == FieldShape.Ellipse)
                {

                }
                switch (Reach)
                {
                    case Reach.All:
                        break;
                    case Reach.Layer:
                        break;
                    case Reach.Name:
                        break;
                }
            }
            //for (int i = 0; i < EventFieldEventGroups.Count; ++i)
            //    EventFieldEventGroups[i].Execute();

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as EventField;
            HalfWidth = initialState.HalfWidth;
            HalfHeight = initialState.HalfHeight;
            FieldShape = initialState.FieldShape;
            Reach = initialState.Reach;
            TargetName = initialState.TargetName;
        }
    }
}
