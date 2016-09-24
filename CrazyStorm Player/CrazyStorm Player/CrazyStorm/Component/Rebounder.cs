/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    public enum RebounderShape
    {
        Line,
        Circle
    }
    class Rebounder : Component
    {
        float lastRotation;
        public int Size { get; set; }
        public RebounderShape RebounderShape { get; set; }
        public float Rotation { get; set; }
        public IList<EventGroup> RebounderEventGroups { get; private set; }
        public Rebounder()
        {
            RebounderEventGroups = new List<EventGroup>();
        }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader rebounderReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(rebounderReader))
                {
                    Size = dataReader.ReadInt32();
                    RebounderShape = PlayDataHelper.ReadEnum<RebounderShape>(dataReader);
                    Rotation = dataReader.ReadSingle();
                    lastRotation = Rotation;
                }
                //rebounderEventGroups
                PlayDataHelper.LoadObjectList(RebounderEventGroups, rebounderReader, version);
            }
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
#if GENERATE_SNIPPET
                case "Size":
                    VM.PushInt(Size);
                    return true;
                case "RebounderShape":
                    VM.PushEnum((int)RebounderShape);
                    return true;
                case "Rotation":
                    VM.PushFloat(Rotation);
                    return true;
#endif
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
#if GENERATE_SNIPPET
                case "Size":
                    Size = VM.PopInt();
                    return true;
                case "RebounderShape":
                    RebounderShape = (RebounderShape)VM.PopEnum();
                    return true;
                case "Rotation":
                    Rotation = VM.PopFloat();
                    return true;
#endif
            }
            return false;
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            if (BindingTarget == null)
                Update();
            else
                BindingUpdate(Update);

            return true;
        }
        public override void Reset()
        {
            base.Reset();
            var initialState = base.initialState as Rebounder;
            Size = initialState.Size;
            RebounderShape = initialState.RebounderShape;
            Rotation = initialState.Rotation;
        }
        void Update()
        {
            base.ExecuteExpression("Size");
            base.ExecuteExpression("Rotation");
            List<ParticleBase> results = ParticleManager.SearchByRect(Position.x - Size, Position.x + Size,
                Position.y - Size, Position.y + Size);
            foreach (ParticleBase particle in results)
            {
                if (particle.IgnoreRebound || particle.Type == null || particle.PSpeedVector == Vector2.Zero)
                    continue;

                float radius = Math.Max(particle.Type.Width, particle.Type.Height) / 2;
                float rotation = Rotation;
                switch (RebounderShape)
                {
                    case RebounderShape.Line:
                        Vector2 p1 = Position + MathHelper.GetVector2(Size, Rotation);
                        Vector2 p2 = Position + MathHelper.GetVector2(Size, Rotation + 180);
                        if (!MathHelper.LineIntersectWithCircle(p1, p2, particle.PPosition, radius))
                            continue;

                        if (SpeedVector != Vector2.Zero && Vector2.Dot(particle.PSpeedVector, SpeedVector) >= 0)
                            continue;

                        float dr = Rotation - lastRotation;
                        if (dr > 0)
                        {
                            Vector2 rotationVector = MathHelper.GetVector2(1, Rotation + 90);
                            if (Vector2.Dot(particle.PSpeedVector, rotationVector) >= 0)
                                continue;
                        }
                        else if (dr < 0)
                        {
                            Vector2 rotationVector = MathHelper.GetVector2(1, Rotation - 90);
                            if (Vector2.Dot(particle.PSpeedVector, rotationVector) >= 0)
                                continue;
                        }
                        break;
                    case RebounderShape.Circle:
                        if (!MathHelper.TwoCirclesIntersect(Position, Size, particle.PPosition, radius))
                            continue;

                        if (MathHelper.PointInsideCircle(Position, Size, particle.PPosition))
                        {
                            if (Vector2.Dot(particle.PSpeedVector, Position - particle.PPosition) >= 0)
                                continue;
                        }
                        else if (Vector2.Dot(particle.PSpeedVector, particle.PPosition - Position) >= 0)
                            continue;

                        rotation = MathHelper.GetDegree(particle.PPosition - Position) + 90;
                        break;
                }
                particle.PSpeedVector = MathHelper.GetVector2(particle.PSpeed, 2 * rotation - particle.PSpeedAngle);
                for (int i = 0; i < RebounderEventGroups.Count; ++i)
                    RebounderEventGroups[i].Execute(particle);
            }
            lastRotation = Rotation;
        }
    }
}
