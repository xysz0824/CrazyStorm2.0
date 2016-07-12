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
    class Component : PropertyContainer, IPlayData, IRebuildReference<Component>, IPlayable
    {
        Vector2 parentAbsolutePosition;
        Vector2 speedVector;
        Vector2 acspeedVector;
        protected Component initialState;
        public int Id { get; set; }
        public string LayerName { get; set; }
        public string Name { get; set; }
        public int CurrentFrame { get; set; }
        public int BeginFrame { get; set; }
        public int TotalFrame { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 SpeedVector
        {
            get { return speedVector; }
            set 
            { 
                speedVector = value;
                if (speedVector != Vector2.Zero)
                    SpeedAngle = MathHelper.GetDegree(speedVector);
            }
        }
        public Vector2 AcspeedVector
        {
            get { return acspeedVector; }
            set { acspeedVector = value; }
        }
        public float Speed { get; set; }
        public float SpeedAngle { get; set; }
        public float Acspeed { get; set; }
        public float AcspeedAngle { get; set; }
        public bool Visibility { get; set; }
        public Component Parent { get; private set; }
        public int ParentID { get; private set; }
        public Emitter BindingTarget { get; private set; }
        public int BindingTargetID { get; private set; }
        public IList<VariableResource> Variables { get; set; }
        public IList<VariableResource> Globals { get; set; }
        public IList<EventGroup> ComponentEventGroups { get; private set; }
        public Component()
        {
            ParentID = -1;
            BindingTargetID = -1;
            Variables = new List<VariableResource>();
            ComponentEventGroups = new List<EventGroup>();
        }
        public virtual void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader componentReader = PlayDataHelper.GetBlockReader(reader))
            {
                string specificType = PlayDataHelper.ReadString(componentReader);
                Id = componentReader.ReadInt32();
                Name = PlayDataHelper.ReadString(componentReader);
                //properties
                base.LoadPropertyExpressions(componentReader);
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(componentReader))
                {
                    CurrentFrame = dataReader.ReadInt32();
                    BeginFrame = dataReader.ReadInt32();
                    TotalFrame = dataReader.ReadInt32();
                    Position = PlayDataHelper.ReadVector2(dataReader);
                    Speed = dataReader.ReadSingle();
                    SpeedAngle = dataReader.ReadSingle();
                    Acspeed = dataReader.ReadSingle();
                    AcspeedAngle = dataReader.ReadSingle();
                    Visibility = dataReader.ReadBoolean();
                }
                //parent
                ParentID = componentReader.ReadInt32();
                //bindingTarget
                BindingTargetID = componentReader.ReadInt32();
                //variables
                PlayDataHelper.LoadObjectList(Variables, componentReader, version);
                //componentEventGroups
                PlayDataHelper.LoadObjectList(ComponentEventGroups, componentReader, version);
            }
        }
        public void RebuildReferenceFromCollection(IList<Component> collection)
        {
            if (ParentID != -1)
            {
                foreach (var target in collection)
                {
                    if (ParentID == target.Id)
                    {
                        Parent = target;
                        break;
                    }
                }
            }
            if (BindingTargetID != -1)
            {
                foreach (var target in collection)
                {
                    if (BindingTargetID == target.Id)
                    {
                        BindingTarget = target as Emitter;
                        break;
                    }
                }
            }
        }
        public Vector2 GetAbsolutePosition()
        {
            if (Parent != null && parentAbsolutePosition == Vector2.Zero)
            {
                parentAbsolutePosition = Parent.GetAbsolutePosition();
                return Position + parentAbsolutePosition;
            }
            return Position;
        }
        public Vector2 GetRelativePosition()
        {
            if (Parent != null)
            {
                Vector2 relative = Position - parentAbsolutePosition;
                parentAbsolutePosition = Vector2.Zero;
                return relative;
            }
            return Position;
        }
        public override bool PushProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Name":
                    VM.PushString(Name);
                    return true;
                case "CurrentFrame":
                    VM.PushInt(CurrentFrame);
                    return true;
                case "BeginFrame":
                    VM.PushInt(BeginFrame);
                    return true;
                case "TotalFrame":
                    VM.PushInt(TotalFrame);
                    return true;
                case "Position":
                    VM.PushVector2(Position);
                    return true;
                case "Position.x":
                    VM.PushFloat(Position.x);
                    return true;
                case "Position.y":
                    VM.PushFloat(Position.y);
                    return true;
                case "Speed":
                    VM.PushFloat(Speed);
                    return true;
                case "SpeedAngle":
                    VM.PushFloat(SpeedAngle);
                    return true;
                case "Acspeed":
                    VM.PushFloat(Acspeed);
                    return true;
                case "AcspeedAngle":
                    VM.PushFloat(AcspeedAngle);
                    return true;
                case "Visibility":
                    VM.PushBool(Visibility);
                    return true;
                default:
                    for (int i = 0; i < Variables.Count; ++i)
                        if (Variables[i].Label == propertyName)
                        {
                            VM.PushFloat(Variables[i].Value);
                            return true;
                        }

                    for (int i = 0; i < Globals.Count; ++i)
                        if (Globals[i].Label == propertyName)
                        {
                            VM.PushFloat(Globals[i].Value);
                            return true;
                        }

                    return false;
            }
        }
        public override bool SetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = VM.PopString();
                    return true;
                case "CurrentFrame":
                    CurrentFrame = VM.PopInt();
                    return true;
                case "BeginFrame":
                    BeginFrame = VM.PopInt();
                    return true;
                case "TotalFrame":
                    TotalFrame = VM.PopInt();
                    return true;
                case "Position":
                    Position = VM.PopVector2();
                    return true;
                case "Position.x":
                    Position = new Vector2(VM.PopFloat(), Position.y);
                    return true;
                case "Position.y":
                    Position = new Vector2(Position.x, VM.PopFloat());
                    return true;
                case "Speed":
                    Speed = VM.PopFloat();
                    MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
                    return true;
                case "SpeedAngle":
                    SpeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
                    return true;
                case "Acspeed":
                    Acspeed = VM.PopFloat();
                    MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
                    return true;
                case "AcspeedAngle":
                    AcspeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
                    return true;
                case "Visibility":
                    Visibility = VM.PopBool();
                    return true;
                default:
                    for (int i = 0; i < Variables.Count; ++i)
                        if (Variables[i].Label == propertyName)
                        {
                            Variables[i].Value = VM.PopFloat();
                            return true;
                        }

                    for (int i = 0; i < Globals.Count; ++i)
                        if (Globals[i].Label == propertyName)
                        {
                            Globals[i].Value = VM.PopFloat();
                            return true;
                        }

                    return false;
            }
        }
        public delegate void Action();
        public void BindingUpdate(Action updateFunc)
        {
            int saveCurrentFrame = CurrentFrame;
            Vector2 savePosition = Position;
            float saveSpeed = Speed;
            float saveSpeedAngle = SpeedAngle;
            float saveAcspeed = Acspeed;
            float saveAcspeedAngle = AcspeedAngle;
            foreach (var particle in BindingTarget.Particles)
            {
                CurrentFrame = particle.PCurrentFrame;
                Position = particle.PPosition;
                Speed = particle.PSpeed;
                SpeedAngle = particle.PSpeedAngle;
                Acspeed = particle.PAcspeed;
                AcspeedAngle = particle.PAcspeedAngle;
                if (updateFunc != null)
                    updateFunc();
            }
            CurrentFrame = saveCurrentFrame;
            Position = savePosition;
            Speed = saveSpeed;
            SpeedAngle = saveSpeedAngle;
            Acspeed = saveAcspeed;
            AcspeedAngle = saveAcspeedAngle;
        }
        public virtual bool Update(int currentFrame)
        {
            if (currentFrame < BeginFrame || currentFrame >= BeginFrame + TotalFrame || !Visibility)
                return false;

            CurrentFrame = currentFrame - BeginFrame;
            Position = GetRelativePosition();
            if (BindingTarget == null || BindingTarget.Particles.Count == 0)
            {
                speedVector += acspeedVector;
                Position += speedVector;
                for (int i = 0; i < ComponentEventGroups.Count; ++i)
                    ComponentEventGroups[i].Execute(this);
            }
            else
                BindingUpdate(ExecuteEventGroups);

            Position = GetAbsolutePosition();
            return true;
        }
        void ExecuteEventGroups()
        {
            for (int i = 0; i < ComponentEventGroups.Count; ++i)
                ComponentEventGroups[i].Execute(this);
        }
        public virtual void Reset()
        {
            parentAbsolutePosition = Vector2.Zero;
            if (initialState == null)
            {
                initialState = this.MemberwiseClone() as Component;
                initialState.Variables = new List<VariableResource>();
                foreach (VariableResource item in Variables)
                {
                    var variable = new VariableResource { Value = item.Value };
                    initialState.Variables.Add(variable);
                }
                initialState.ExecuteExpressions();
            }
            else
            {
                initialState.ExecuteExpressions();
                CurrentFrame = initialState.CurrentFrame;
                BeginFrame = initialState.BeginFrame;
                TotalFrame = initialState.TotalFrame;
                Position = initialState.Position;
                Speed = initialState.Speed;
                SpeedAngle = initialState.SpeedAngle;
                Acspeed = initialState.Acspeed;
                AcspeedAngle = initialState.AcspeedAngle;
                Visibility = initialState.Visibility;
                for (int i = 0;i < Variables.Count;++i)
                    Variables[i].Value = initialState.Variables[i].Value;
            }
            MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
            MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
        }
    }
}
