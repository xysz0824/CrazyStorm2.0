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
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "HalfWidth":
                    VM.PushFloat(HalfWidth);
                    return true;
                case "HalfHeight":
                    VM.PushFloat(HalfHeight);
                    return true;
                case "FieldShape":
                    VM.PushEnum((int)FieldShape);
                    return true;
                case "Reach":
                    VM.PushEnum((int)Reach);
                    return true;
                case "TargetName":
                    VM.PushString(TargetName);
                    return true;
                case "Force":
                    VM.PushFloat(Force);
                    return true;
                case "Direction":
                    VM.PushFloat(Direction);
                    return true;
                case "ForceType":
                    VM.PushEnum((int)ForceType);
                    return true;
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "HalfWidth":
                    HalfWidth = VM.PopFloat();
                    return true;
                case "HalfHeight":
                    HalfHeight = VM.PopFloat();
                    return true;
                case "FieldShape":
                    FieldShape = (FieldShape)VM.PopEnum();
                    return true;
                case "Reach":
                    Reach = (Reach)VM.PopEnum();
                    return true;
                case "TargetName":
                    TargetName = VM.PopString();
                    return true;
                case "Force":
                    Force = VM.PopFloat();
                    return true;
                case "Direction":
                    Direction = VM.PopFloat();
                    return true;
                case "ForceType":
                    ForceType = (ForceType)VM.PopEnum();
                    return true;
            }
            return false;
        }
        public override bool Update(int currentFrame)
        {
            if (!base.Update(currentFrame))
                return false;

            if (BindingTarget == null || BindingTarget.Particles.Count == 0)
                Update(Position);
            else
            {
                foreach (var particle in BindingTarget.Particles)
                    Update(particle.PPosition);
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
        void Update(Vector2 position)
        {
            base.ExecuteExpression("HalfWidth");
            base.ExecuteExpression("HalfHeight");
            base.ExecuteExpression("Force");
            base.ExecuteExpression("Direction");
            List<ParticleBase> results = ParticleManager.SearchByRect(position.x - HalfWidth, position.x + HalfWidth,
                position.y - HalfHeight, position.y + HalfHeight);
            foreach (Particle particle in results)
            {
                if (particle.IgnoreForce)
                    continue;

                switch (Reach)
                {
                    case Reach.Layer:
                        if (particle.Emitter.LayerName != TargetName && particle.Emitter.LayerName != LayerName)
                            continue;

                        break;
                    case Reach.Name:
                        if (particle.Emitter.Name != TargetName)
                            continue;

                        break;
                }
                if (FieldShape == FieldShape.Circle)
                {
                    Vector2 v = position - particle.PPosition;
                    if (Math.Sqrt(v.x * v.x + v.y * v.y) > HalfWidth)
                        continue;
                }
                switch (ForceType)
                {
                    case ForceType.Direction:
                        Vector2 v = new Vector2();
                        MathHelper.SetVector2(ref v, Force / particle.Mass, Direction);
                        particle.PSpeedVector += v;
                        break;
                    case ForceType.Inner:
                        v = position - particle.PPosition;
                        float d = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
                        particle.PSpeedVector += v / d * (Force / particle.Mass);
                        break;
                    case ForceType.Outer:
                        v = particle.PPosition - position;
                        d = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
                        particle.PSpeedVector += v / d * (Force / particle.Mass);
                        break;
                }
            }
        }
    }
}
