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
        public FieldShape FieldShape { get; set; }
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
                    FieldShape = PlayDataHelper.ReadEnum<FieldShape>(dataReader);
                    Reach = PlayDataHelper.ReadEnum<Reach>(dataReader);
                    TargetName = PlayDataHelper.ReadString(dataReader);
                    Force = dataReader.ReadSingle();
                    Direction = dataReader.ReadSingle();
                    ForceType = PlayDataHelper.ReadEnum<ForceType>(dataReader);
                }
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
                (int)(Position.x - HalfWidth), (int)(Position.x + HalfWidth), 
                (int)(Position.y - HalfHeight), (int)(Position.y + HalfHeight));
            foreach (Particle particle in results)
            {
                if (particle.IgnoreForce)
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
            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as ForceField;
            HalfWidth = initialState.HalfWidth;
            HalfHeight = initialState.HalfHeight;
            FieldShape = initialState.FieldShape;
            Reach = initialState.Reach;
            TargetName = initialState.TargetName;
            Force = initialState.Force;
            Direction = initialState.Direction;
            ForceType = initialState.ForceType;
        }
    }
}
