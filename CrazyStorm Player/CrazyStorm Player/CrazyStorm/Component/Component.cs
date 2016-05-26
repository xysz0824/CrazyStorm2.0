using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class Component : PropertyContainer, IPlayData, IRebuildReference<Component>, IPlayable
    {
        private Vector2 speedVector;
        private Vector2 acspeedVector;
        protected Component initialState;
        public int Id { get; set; }
        public string Name { get; set; }
        public int CurrentFrame { get; set; }
        public int BeginFrame { get; set; }
        public int TotalFrame { get; set; }
        public Vector2 Position { get; set; }
        public float Speed { get; set; }
        public float SpeedAngle { get; set; }
        public float Acspeed { get; set; }
        public float AcspeedAngle { get; set; }
        public bool Visibility { get; set; }
        public Component Parent { get; private set; }
        public int ParentID { get; private set; }
        public Component BindingTarget { get; private set; }
        public int BindingTargetID { get; private set; }
        public IList<VariableResource> Variables { get; set; }
        public IList<EventGroup> ComponentEventGroups { get; private set; }
        public Component()
        {
            ParentID = -1;
            BindingTargetID = -1;
            Variables = new List<VariableResource>();
            ComponentEventGroups = new List<EventGroup>();
        }
        public virtual void LoadPlayData(BinaryReader reader)
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
                PlayDataHelper.LoadObjectList(Variables, componentReader);
                //componentEventGroups
                PlayDataHelper.LoadObjectList(ComponentEventGroups, componentReader);
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
                        BindingTarget = target;
                        break;
                    }
                }
            }
        }
        public virtual bool Update(int currentFrame)
        {
            if (currentFrame < BeginFrame || currentFrame >= BeginFrame + TotalFrame)
                return false;

            CurrentFrame = currentFrame - BeginFrame;
            base.ExecuteExpressions();
            speedVector += acspeedVector;
            Position += speedVector;
            double vf = 0;
            if (speedVector.y != 0)
            {
                vf = Math.PI / 2 - Math.Atan(speedVector.x / speedVector.y);
                if (speedVector.y < 0) 
                    vf += Math.PI;
            }
            else
            {
                vf = speedVector.x >= 0 ? 0 : Math.PI;
            }
            SpeedAngle = (float)MathHelper.RadToDeg(vf);
            for (int i = 0; i < ComponentEventGroups.Count; ++i)
                ComponentEventGroups[i].Execute();

            return true;
        }
        public virtual void Reset()
        {
            if (initialState == null)
            {
                initialState = this.MemberwiseClone() as Component;
                initialState.Variables = new List<VariableResource>();
                foreach (VariableResource item in Variables)
                {
                    var variable = new VariableResource { Value = item.Value };
                    initialState.Variables.Add(variable);
                }
            }
            else
            {
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
