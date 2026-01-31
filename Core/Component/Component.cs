/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.IO;

namespace CrazyStorm.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ComponentData
    {
        public int id;
        public int beginFrame;
        public int totalFrame;
        public Vector2 position;
        public float speed;
        public float speedAngle;
        public float acspeed;
        public float acspeedAngle;
        public bool visibility;
    }
    public class Component : PropertyContainer, INotifyPropertyChanged, IXmlData, IGeneratePlayData, IPlayable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        [StringData]
        [XmlAttribute]
        string name;
        Vector2 parentAbsolutePosition;
        Vector2 speedVector;
        Vector2 acspeedVector;
        ComponentData componentData;
        bool selected;
        Component parent;
        Emitter bindingTarget;
        IList<EventGroup> componentEventGroups;
        IList<Component> children;
        #endregion

        #region Protected Members
        protected Component initialState;
        #endregion

        #region Public Members
        public int ID
        {
            get { return componentData.id; }
            set { componentData.id = value; }
        }
        [StringProperty(0, 1, 15, true, true, false, false)]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }
        [ReadOnlyProperty(1)]
        public Emitter BindingTarget
        {
            get { return bindingTarget; }
            set { bindingTarget = value; }
        }
        public string LayerName { get; set; }
        public int LayerID { get; set; }
        [RuntimeProperty(2)]
        public float LayerFrame { get; set; }
        [RuntimeProperty(3)]
        public float CurrentFrame { get; set; }
        [IntProperty(4, 1, int.MaxValue)]
        public int BeginFrame
        {
            get { return componentData.beginFrame; }
            set { componentData.beginFrame = value; }
        }
        [IntProperty(5, 1, int.MaxValue)]
        public int TotalFrame
        {
            get { return componentData.totalFrame; }
            set { componentData.totalFrame = value; }
        }
        [Vector2Property(6)]
        public Vector2 Position //When in Edit mode, this represents relative position, in Play mode, absolute position
        {
            get { return componentData.position; }
            set { componentData.position = value; }
        }
        public float X
        {
            get { return componentData.position.x; }
            set { componentData.position.x = value; }
        }
        public float Y
        {
            get { return componentData.position.y; }
            set { componentData.position.y = value; }
        }
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
        [FloatProperty(9, float.MinValue, float.MaxValue)]
        public float Speed
        {
            get { return componentData.speed; }
            set { componentData.speed = value; }
        }
        [FloatProperty(10, float.MinValue, float.MaxValue)]
        public float SpeedAngle
        {
            get { return componentData.speedAngle; }
            set { componentData.speedAngle = value; }
        }
        [FloatProperty(11, float.MinValue, float.MaxValue)]
        public float Acspeed
        {
            get { return componentData.acspeed; }
            set { componentData.acspeed = value; }
        }
        [FloatProperty(12, float.MinValue, float.MaxValue)]
        public float AcspeedAngle
        {
            get { return componentData.acspeedAngle; }
            set { componentData.acspeedAngle = value; }
        }
        [BoolProperty(13)]
        public bool Visibility
        {
            get { return componentData.visibility; }
            set { componentData.visibility = value; }
        }
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
        public Component Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        public int ParentID { get; set; }
        public int BindingTargetID { get; set; }
        public IList<VariableResource> Globals { get; set; }
        public IList<VariableResource> Locals { get; private set; }
        public Vector2 BodyPosition { get; set; }
        public Vector2 CenterPosition { get; set; }
        public int Status { get; set; }
        public float StatusFrame { get; set; }
        public IList<EventGroup> ComponentEventGroups { get { return componentEventGroups; } }
        public IList<Component> Children { get { return children; } }
        #endregion

        #region Constructor
        public Component()
        {
            ParentID = -1;
            BindingTargetID = -1;
            name = string.Empty;
            componentData.beginFrame = 1;
            componentData.totalFrame = 200;
            componentData.visibility = true;
            Locals = new GenericContainer<VariableResource>();
            componentEventGroups = new GenericContainer<EventGroup>();
            children = new GenericContainer<Component>();
        }
        #endregion

        #region Protected Methods
        public delegate void Action(float frameScale);
        public void BindingUpdate(Action updateFunc, bool executeEvents, float frameScale)
        {
            float saveCurrentFrame = CurrentFrame;
            Vector2 savePosition = Position;
            float saveSpeed = Speed;
            float saveSpeedAngle = SpeedAngle;
            float saveAcspeed = Acspeed;
            float saveAcspeedAngle = AcspeedAngle;
            bool eventImpacted = false;
            foreach (var particle in BindingTarget.Particles)
            {
                CurrentFrame = particle.PCurrentFrame - BeginFrame;
                if (CurrentFrame < 1 || CurrentFrame > TotalFrame || !particle.Alive || !Visibility) continue;
                if (executeEvents && !EventManager.BindingRecover(this, particle) && eventImpacted)
                {
                    Reset();
                }
                Position = particle.PPosition;
                Speed = particle.PSpeed;
                SpeedAngle = particle.PSpeedAngle;
                Acspeed = particle.PAcspeed;
                AcspeedAngle = particle.PAcspeedAngle;
                ExecuteDynamicExpressions(frameScale);
                if (executeEvents)
                {
                    for (int i = 0; i < ComponentEventGroups.Count; ++i)
                    {
                        ComponentEventGroups[i].Execute(this, particle, frameScale);
                    }
                }
                updateFunc?.Invoke(frameScale);
                if (executeEvents && EventManager.BindingUpdate(this, particle, frameScale))
                {
                    eventImpacted = true;
                }
            }
            CurrentFrame = saveCurrentFrame;
            Position = savePosition;
            Speed = saveSpeed;
            SpeedAngle = saveSpeedAngle;
            Acspeed = saveAcspeed;
            AcspeedAngle = saveAcspeedAngle;
        }
        protected bool CheckCircularBinding()
        {
            if (this is Emitter)
                return BindingTarget != null && BindingTarget.BindingTarget == this &&
                    BindingTarget.Particles.Count == 0 && (this as Emitter).Particles.Count == 0;
            else
                return false;
        }
        #endregion

        #region Public Methods
        public override string ToString() => Name;
        public void TransPositiontoRelative()
        {
            if (parent != null) Position -= parent.GetAbsolutePosition();
        }
        public void TransPositiontoAbsolute()
        {
            if (parent != null) Position += parent.GetAbsolutePosition();
        }
        public Vector2 GetAbsolutePosition()
        {
            if (parent != null) return Position + parent.GetAbsolutePosition();
            return Position;
        }
        public IList<Component> GetPosterity()
        {
            List<Component> posterity = new List<Component>();
            foreach (var item in children)
            {
                posterity.Add(item);
                posterity.AddRange(item.GetPosterity());
            }
            return posterity;
        }
        public Component FindParent(Component child)
        {
            if (children.Count == 0)
                return null;

            if (children.Contains(child))
                return this;

            foreach (var item in children)
            {
                var parent = item.FindParent(child);
                if (parent != null)
                    return parent;
            }
            return null;
        }
        public override object Clone()
        {
            var clone = base.Clone() as Component;
            clone.parent = null;
            if (parent != null)
                clone.ParentID = parent.ID;

            clone.bindingTarget = null;
            if (bindingTarget != null)
                clone.BindingTargetID = bindingTarget.ID;

            clone.Locals = new GenericContainer<VariableResource>();
            foreach (var variable in Locals)
            {
                clone.Locals.Add(variable.Clone() as VariableResource);
            }
            clone.componentEventGroups = new GenericContainer<EventGroup>();
            foreach (var componentEventGroup in componentEventGroups)
            {
                clone.componentEventGroups.Add(componentEventGroup.Clone() as EventGroup);
            }
            clone.children = new GenericContainer<Component>();
            if (children.Count > 0)
            {
                foreach (var child in children)
                    clone.children.Add(child);
            }
            return clone;
        }
        public virtual XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Component";
            var componentNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                componentNode = node;

            XmlHelper.BuildFromFields(this, componentNode);
            //properties
            base.BuildFromXmlElement(componentNode);
            //componentData
            XmlHelper.BuildFromStruct(ref componentData, componentNode);
            //parent
            if (componentNode.HasAttribute("parent"))
            {
                string parentAttribute = componentNode.GetAttribute("parent");
                int parsedID;
                if (int.TryParse(parentAttribute, out parsedID))
                    ParentID = parsedID;
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
            //bindingTarget
            if (componentNode.HasAttribute("bindingTarget"))
            {
                string bindingTargetAttribute = componentNode.GetAttribute("bindingTarget");
                int parsedID;
                if (int.TryParse(bindingTargetAttribute, out parsedID))
                    BindingTargetID = parsedID;
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
            //variables
            XmlHelper.BuildFromObjectList(Locals, new VariableResource(int.MinValue, ""), componentNode, "Variables");
            //componentEventGroups
            XmlHelper.BuildFromObjectList(componentEventGroups, new EventGroup(), componentNode, "ComponentEventGroups");
            return componentNode;
        }
        public virtual XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var componentNode = doc.CreateElement("Component");
            var specificTypeAttribute = doc.CreateAttribute("specificType");
            specificTypeAttribute.Value = GetType().Name;
            componentNode.Attributes.Append(specificTypeAttribute);
            XmlHelper.StoreFields(this, doc, componentNode);
            //properties
            componentNode.AppendChild(base.GetXmlElement(doc));
            //componentData
            XmlHelper.StoreStruct(componentData, doc, componentNode);
            //parent
            if (parent != null)
            {
                var parentAttribute = doc.CreateAttribute("parent");
                parentAttribute.Value = parent.ID.ToString();
                componentNode.Attributes.Append(parentAttribute);
            }
            //bindingTarget
            if (bindingTarget != null)
            {
                var bindingTargetAttribute = doc.CreateAttribute("bindingTarget");
                bindingTargetAttribute.Value = bindingTarget.ID.ToString();
                componentNode.Attributes.Append(bindingTargetAttribute);
            }
            //variables
            XmlHelper.StoreObjectList(Locals, doc, componentNode, "Variables");
            //componentEventGroups
            XmlHelper.StoreObjectList(componentEventGroups, doc, componentNode, "ComponentEventGroups");
            node.AppendChild(componentNode);
            return componentNode;
        }
        public void RebuildReferenceFromCollection(IList<Component> collection)
        {
            //parent
            if (ParentID != -1)
            {
                foreach (var target in collection)
                {
                    if (ParentID == target.ID)
                    {
                        parent = target;
                        parent.children.Add(this);
                        break;
                    }
                }
                ParentID = -1;
            }
            //bindingTarget
            if (BindingTargetID != -1)
            {
                foreach (var target in collection)
                {
                    if (BindingTargetID == target.ID)
                    {
                        bindingTarget = target as Emitter;
                        break;
                    }
                }
                BindingTargetID = -1;
            }
        }
        public virtual List<byte> GeneratePlayData(File file)
        {
            var componentBytes = new List<byte>();
            //type for factory
            componentBytes.AddRange(PlayDataHelper.GetStringBytes(GetType().Name));
            //stringDataFields
            PlayDataHelper.GenerateStringDataFields(this, componentBytes);
            //componentData
            PlayDataHelper.GenerateStruct(componentData, componentBytes);
            //parent
            componentBytes.AddRange(BitConverter.GetBytes(parent != null ? parent.ID : -1));
            //bindingTarget
            componentBytes.AddRange(BitConverter.GetBytes(bindingTarget != null ? bindingTarget.ID : -1));
            //variables
            PlayDataHelper.GenerateObjectList(file, Locals, componentBytes);
            //properties
            var variables = new List<VariableResource>();
            variables.AddRange(Locals);
            variables.AddRange(file.Globals);
            base.GeneratePropertyExpressions(variables, componentBytes);
            //componentEventGroups
            PlayDataHelper.GenerateObjectList(file, componentEventGroups, componentBytes);
            return PlayDataHelper.CreateBlock(componentBytes);
        }
        public virtual void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader componentReader = PlayDataHelper.GetBlockReader(reader))
            {
                //Must swallow type string here
                PlayDataHelper.ReadString(componentReader);
                //stringDataFields
                PlayDataHelper.ReadStringDataFields(this, componentReader);
                //componentData
                componentData = PlayDataHelper.ReadStruct<ComponentData>(componentReader);
                //parent
                ParentID = componentReader.ReadInt32();
                //bindingTarget
                BindingTargetID = componentReader.ReadInt32();
                //variables
                PlayDataHelper.ReadObjectList(Locals, componentReader, version);
                //properties
                base.LoadPropertyExpressions(componentReader);
                //componentEventGroups
                PlayDataHelper.ReadObjectList(ComponentEventGroups, componentReader, version);
            }
        }
        public Vector2 GetAbsolutePositionRuntime()
        {
            if (Parent != null && parentAbsolutePosition == Vector2.Zero)
            {
                parentAbsolutePosition = Parent.GetAbsolutePosition();
                return Position + parentAbsolutePosition;
            }
            return Position;
        }
        public Vector2 GetRelativePositionRuntime()
        {
            if (Parent != null)
            {
                Vector2 relative = Position - parentAbsolutePosition;
                parentAbsolutePosition = Vector2.Zero;
                return relative;
            }
            return Position;
        }
        protected virtual bool PushSystemProperty(int propertyID)
        {
            switch (propertyID)
            {
                case -1:
                    VM.PushInt(Status);
                    return true;
                case -2:
                    VM.PushFloat(StatusFrame, true, true);
                    return true;
                case -3:
                    VM.PushFloat(0);
                    return true;
                case -4:
                    VM.PushVector2(BodyPosition);
                    return true;
                case -5:
                    VM.PushFloat(BodyPosition.x);
                    return true;
                case -6:
                    VM.PushFloat(BodyPosition.y);
                    return true;
                case -7:
                    var degree = MathHelper.GetDegree(BodyPosition - Position);
                    VM.PushFloat(degree);
                    return true;
                case -8:
                    VM.PushVector2(CenterPosition);
                    return true;
                case -9:
                    VM.PushFloat(CenterPosition.x);
                    return true;
                case -10:
                    VM.PushFloat(CenterPosition.y);
                    return true;
                case -11:
                    degree = MathHelper.GetDegree(CenterPosition - Position);
                    VM.PushFloat(degree);
                    return true;
            }
            return false;
        }
        public override bool PushProperty(int propertyID)
        {
            if (PushSystemProperty(propertyID)) return true;
            switch (propertyID)
            {
                case 0:
                    VM.PushString(Name);
                    return true;
                case 2:
                    VM.PushFloat(LayerFrame, true, true);
                    return true;
                case 3:
                    VM.PushFloat(CurrentFrame, true, true);
                    return true;
                case 4:
                    VM.PushInt(BeginFrame);
                    return true;
                case 5:
                    VM.PushInt(TotalFrame);
                    return true;
                case 6:
                    VM.PushVector2(Position);
                    return true;
                case 7:
                    VM.PushFloat(Position.x);
                    return true;
                case 8:
                    VM.PushFloat(Position.y);
                    return true;
                case 9:
                    VM.PushFloat(Speed);
                    return true;
                case 10:
                    VM.PushFloat(SpeedAngle);
                    return true;
                case 11:
                    VM.PushFloat(Acspeed);
                    return true;
                case 12:
                    VM.PushFloat(AcspeedAngle);
                    return true;
                case 13:
                    VM.PushBool(Visibility);
                    return true;
            }
            for (int i = 0; i < Locals.Count; ++i)
            {
                if (Locals[i].ID == propertyID)
                {
                    VM.PushFloat(Locals[i].Value);
                    return true;
                }
            }
            for (int i = 0; i < Globals.Count; ++i)
            {
                if (Globals[i].ID == propertyID)
                {
                    VM.PushFloat(Globals[i].Value);
                    return true;
                }
            }
            return false;
        }
        public override bool SetProperty(int propertyID)
        {
            switch (propertyID)
            {
                case 0:
                    Name = VM.PopString();
                    return true;
                case 4:
                    BeginFrame = VM.PopInt();
                    return true;
                case 5:
                    TotalFrame = VM.PopInt();
                    return true;
                case 6:
                    Position = VM.PopVector2();
                    return true;
                case 7:
                    Position = new Vector2(VM.PopFloat(), Position.y);
                    return true;
                case 8:
                    Position = new Vector2(Position.x, VM.PopFloat());
                    return true;
                case 9:
                    Speed = VM.PopFloat();
                    MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
                    return true;
                case 10:
                    SpeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
                    return true;
                case 11:
                    Acspeed = VM.PopFloat();
                    MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
                    return true;
                case 12:
                    AcspeedAngle = VM.PopFloat();
                    MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
                    return true;
                case 13:
                    Visibility = VM.PopBool();
                    return true;
            }
            for (int i = 0; i < Locals.Count; ++i)
            {
                if (Locals[i].ID == propertyID)
                {
                    Locals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            for (int i = 0; i < Globals.Count; ++i)
            {
                if (Globals[i].ID == propertyID)
                {
                    Globals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            return false;
        }
        public virtual bool Update(float frameScale, float currentFrame)
        {
            ExecuteDynamicExpressions(frameScale);
            LayerFrame = currentFrame;
            if (BindingTarget == null || CheckCircularBinding())
            {
                CurrentFrame = currentFrame - BeginFrame + 1;
                if (CurrentFrame < 1 || CurrentFrame > TotalFrame || !Visibility)
                    return false;
            }
            Position = GetRelativePositionRuntime();
            if (BindingTarget == null || CheckCircularBinding())
            {
                speedVector += acspeedVector * frameScale;
                Position += speedVector * frameScale;
                for (int i = 0; i < ComponentEventGroups.Count; ++i)
                    ComponentEventGroups[i].Execute(this, null, frameScale);
            }
            Position = GetAbsolutePositionRuntime();
            return true;
        }
        public virtual void Reset()
        {
            if (initialState == null)
            {
                initialState = this.MemberwiseClone() as Component;
                initialState.Locals = new List<VariableResource>();
                foreach (VariableResource item in Locals)
                {
                    var variable = new VariableResource { ID = item.ID, Label = item.Label, Value = item.Value };
                    initialState.Locals.Add(variable);
                }
                initialState.ExecuteExpressionsAndSet(1);
            }
            else
            {
                initialState.ExecuteExpressionsAndSet(1);
                BeginFrame = initialState.BeginFrame;
                TotalFrame = initialState.TotalFrame;
                Position = initialState.Position;
                Speed = initialState.Speed;
                SpeedAngle = initialState.SpeedAngle;
                Acspeed = initialState.Acspeed;
                AcspeedAngle = initialState.AcspeedAngle;
                Visibility = initialState.Visibility;
                for (int i = 0; i < Locals.Count; ++i)
                {
                    Locals[i].Value = initialState.Locals[i].Value;
                }
            }
            MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
            MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
            parentAbsolutePosition = Vector2.Zero;
            Position = GetAbsolutePositionRuntime();
        }
        #endregion
    }
}
