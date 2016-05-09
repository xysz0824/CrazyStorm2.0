using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class Component : IPlayData, IRebuildReference<Component>
    {
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
        public IList<VariableResource> Variables { get; private set; }
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
                //TODO
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
                //TODO
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
    }
}
