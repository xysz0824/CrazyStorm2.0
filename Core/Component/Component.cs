/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
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
    public struct ComponentData
    {
        public int layerFrame;
        public int currentFrame;
        public int beginFrame;
        public int totalFrame;
        public Vector2 position;
        public float speed;
        public float speedAngle;
        public float acspeed;
        public float acspeedAngle;
        public bool visibility;
    }
    public class Component : PropertyContainer, INotifyPropertyChanged, IXmlData, IRebuildReference<Component>, IGeneratePlayData
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        Vector2 parentAbsolutePosition;
        Vector2 speedVector;
        Vector2 acspeedVector;
        ComponentData componentData;
        [PlayData]
        [XmlAttribute]
        int id;
        [PlayData]
        [XmlAttribute]
        string name;
        bool selected;
        Component parent;
        Emitter bindingTarget;
        IList<EventGroup> componentEventGroups;
        IList<Component> children;
        IList<int> childrenIDs;
        #endregion

        #region Protected Members
        protected Component initialState;
        #endregion

        #region Public Members
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        [StringProperty(1, 15, true, true, false, false)]
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
        public string LayerName { get; set; }
        [RuntimeProperty]
        public int LayerFrame
        {
            get { return componentData.layerFrame; }
            set { componentData.layerFrame = value; }
        }
        [RuntimeProperty]
        public int CurrentFrame
        {
            get { return componentData.currentFrame; }
            set { componentData.currentFrame = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int BeginFrame
        {
            get { return componentData.beginFrame; }
            set { componentData.beginFrame = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int TotalFrame
        {
            get { return componentData.totalFrame; }
            set { componentData.totalFrame = value; }
        }
        [Vector2Property]
        public Vector2 Position
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
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Speed
        {
            get { return componentData.speed; }
            set { componentData.speed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float SpeedAngle
        {
            get { return componentData.speedAngle; }
            set { componentData.speedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Acspeed
        {
            get { return componentData.acspeed; }
            set { componentData.acspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float AcspeedAngle
        {
            get { return componentData.acspeedAngle; }
            set { componentData.acspeedAngle = value; }
        }
        [BoolProperty]
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
        public int ParentID { get; private set; }
        public Emitter BindingTarget
        {
            get { return bindingTarget; }
            set { bindingTarget = value; }
        }
        public int BindingTargetID { get; private set; }
        public IList<VariableResource> Locals { get; set; }
        public IList<VariableResource> Globals { get; set; }
        public IList<EventGroup> ComponentEventGroups { get { return componentEventGroups; } }
        public IList<Component> Children { get { return children; } }
        #endregion

        #region Constructor
        public Component()
        {
            ParentID = -1;
            BindingTargetID = -1;
            name = string.Empty;
            componentData.totalFrame = 200;
            componentData.visibility = true;
            Locals = new GenericContainer<VariableResource>();
            componentEventGroups = new GenericContainer<EventGroup>();
            children = new GenericContainer<Component>();
        }
        #endregion

        #region Private Methods
        void ExecuteEventGroups()
        {
            for (int i = 0; i < ComponentEventGroups.Count; ++i)
                ComponentEventGroups[i].Execute(this);
        }
        #endregion

        #region Protected Methods
        protected delegate void Action();
        protected void BindingUpdate(Action updateFunc)
        {
            int saveCurrentFrame = CurrentFrame;
            Vector2 savePosition = Position;
            float saveSpeed = Speed;
            float saveSpeedAngle = SpeedAngle;
            float saveAcspeed = Acspeed;
            float saveAcspeedAngle = AcspeedAngle;
            foreach (var particle in BindingTarget.Particles)
            {
                CurrentFrame = particle.PCurrentFrame - BeginFrame;
                if (CurrentFrame < 0 || CurrentFrame >= TotalFrame || !Visibility)
                    continue;

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
        public void TransPositiontoRelative()
        {
            if (parent != null)
                componentData.position -= parent.GetAbsolutePosition();
        }
        public void TransPositiontoAbsolute()
        {
            if (parent != null)
                componentData.position += parent.GetAbsolutePosition();
        }
        public Vector2 GetAbsolutePosition()
        {
            if (parent != null)
                return componentData.position + parent.GetAbsolutePosition();

            return componentData.position;
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
                clone.ParentID = parent.id;

            clone.bindingTarget = null;
            if (bindingTarget != null)
                clone.BindingTargetID = bindingTarget.id;

            clone.Locals = new GenericContainer<VariableResource>();
            foreach (var variable in Locals)
                clone.Locals.Add(variable.Clone() as VariableResource);

            clone.componentEventGroups = new GenericContainer<EventGroup>();
            foreach (var componentEventGroup in componentEventGroups)
                clone.componentEventGroups.Add(componentEventGroup.Clone() as EventGroup);

            clone.children = new GenericContainer<Component>();
            if (children.Count > 0)
            {
                clone.childrenIDs = new List<int>();
                foreach (var child in children)
                    clone.childrenIDs.Add(child.Id);
            }
            return clone;
        }
        public virtual XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "Component";
            var componentNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName)
                componentNode = node;

            XmlHelper.BuildFromFields(typeof(Component), this, componentNode);
            //properties
            base.BuildFromXmlElement(componentNode);
            //componentData
            XmlHelper.BuildFromStruct(ref componentData, componentNode, "ComponentData");
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
            XmlHelper.BuildFromObjectList(Locals, new VariableResource(""), componentNode, "Variables");
            //componentEventGroups
            XmlHelper.BuildFromObjectList(componentEventGroups, new EventGroup(), componentNode, "ComponentEventGroups");
            //children
            childrenIDs = new List<int>();
            var childrenNode = componentNode.SelectSingleNode("Children");
            foreach (XmlElement childNode in childrenNode.ChildNodes)
            {
                string idAttribute = childNode.GetAttribute("id");
                int parsedID;
                if (int.TryParse(idAttribute, out parsedID))
                    childrenIDs.Add(parsedID);
                else
                    throw new System.IO.FileLoadException("FileDataError");
            }
            return componentNode;
        }
        public virtual XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var componentNode = doc.CreateElement("Component");
            var specificTypeAttribute = doc.CreateAttribute("specificType");
            specificTypeAttribute.Value = GetType().Name;
            componentNode.Attributes.Append(specificTypeAttribute);
            XmlHelper.StoreFields(typeof(Component), this, doc, componentNode);
            //properties
            componentNode.AppendChild(base.GetXmlElement(doc));
            //componentData
            XmlHelper.StoreStruct(componentData, doc, componentNode, "ComponentData");
            //parent
            if (parent != null)
            {
                var parentAttribute = doc.CreateAttribute("parent");
                parentAttribute.Value = parent.Id.ToString();
                componentNode.Attributes.Append(parentAttribute);
            }
            //bindingTarget
            if (bindingTarget != null)
            {
                var bindingTargetAttribute = doc.CreateAttribute("bindingTarget");
                bindingTargetAttribute.Value = bindingTarget.Id.ToString();
                componentNode.Attributes.Append(bindingTargetAttribute);
            }
            //variables
            XmlHelper.StoreObjectList(Locals, doc, componentNode, "Variables");
            //componentEventGroups
            XmlHelper.StoreObjectList(componentEventGroups, doc, componentNode, "ComponentEventGroups");
            //children
            var childrenNode = doc.CreateElement("Children");
            foreach (var component in children)
            {
                var childNode = doc.CreateElement("Component");
                var idAttribute = doc.CreateAttribute("id");
                idAttribute.Value = component.Id.ToString();
                childNode.Attributes.Append(idAttribute);
                childrenNode.AppendChild(childNode);
            }
            componentNode.AppendChild(childrenNode);
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
                    if (ParentID == target.Id)
                    {
                        parent = target;
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
                    if (BindingTargetID == target.Id)
                    {
                        bindingTarget = target as Emitter;
                        break;
                    }
                }
                BindingTargetID = -1;
            }
            //children
            if (childrenIDs != null)
            {
                foreach (var childrenID in childrenIDs)
                {
                    foreach (var target in collection)
                    {
                        if (childrenID == target.Id)
                        {
                            children.Add(target);
                            break;
                        }
                    }
                }
                childrenIDs = null;
            }
        }
        public virtual List<byte> GeneratePlayData()
        {
            var componentBytes = new List<byte>();
            //type
            componentBytes.AddRange(PlayDataHelper.GetStringBytes(GetType().Name));
            PlayDataHelper.GenerateFields(typeof(Component), this, componentBytes);
            //properties
            base.GeneratePlayData(componentBytes);
            //componentData
            PlayDataHelper.GenerateStruct(componentData, componentBytes);
            //parent
            if (parent != null)
                componentBytes.AddRange(PlayDataHelper.GetBytes(parent.Id));
            else
                componentBytes.AddRange(PlayDataHelper.GetBytes(-1));
            //bindingTarget
            if (bindingTarget != null)
                componentBytes.AddRange(PlayDataHelper.GetBytes(bindingTarget.Id));
            else
                componentBytes.AddRange(PlayDataHelper.GetBytes(-1));
            //variables
            PlayDataHelper.GenerateObjectList(Locals, componentBytes);
            //componentEventGroups
            PlayDataHelper.GenerateObjectList(componentEventGroups, componentBytes);
            return PlayDataHelper.CreateBlock(componentBytes);
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
                    LayerFrame = dataReader.ReadInt32();
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
                PlayDataHelper.LoadObjectList(Locals, componentReader, version);
                //componentEventGroups
                PlayDataHelper.LoadObjectList(ComponentEventGroups, componentReader, version);
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
        public override bool PushProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Name":
                    VM.PushString(Name);
                    return true;
                case "LayerFrame":
                    VM.PushInt(LayerFrame);
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
            }
            for (int i = 0; i < Locals.Count; ++i)
            {
                if (Locals[i].Label == propertyName)
                {
                    VM.PushFloat(Locals[i].Value);
                    return true;
                }
            }
            for (int i = 0; i < Globals.Count; ++i)
            {
                if (Globals[i].Label == propertyName)
                {
                    VM.PushFloat(Globals[i].Value);
                    return true;
                }
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = VM.PopString();
                    return true;
                case "LayerFrame":
                    LayerFrame = VM.PopInt();
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
            }
            for (int i = 0; i < Locals.Count; ++i)
            {
                if (Locals[i].Label == propertyName)
                {
                    Locals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            for (int i = 0; i < Globals.Count; ++i)
            {
                if (Globals[i].Label == propertyName)
                {
                    Globals[i].Value = VM.PopFloat();
                    return true;
                }
            }
            return false;
        }
        public virtual bool Update(int currentFrame)
        {
            LayerFrame = currentFrame;
            if (BindingTarget == null || CheckCircularBinding())
            {
                CurrentFrame = currentFrame - BeginFrame;
                if (CurrentFrame < 0 || CurrentFrame >= TotalFrame || !Visibility)
                    return false;
            }
            Position = GetRelativePositionRuntime();
            if (BindingTarget == null || CheckCircularBinding())
            {
                speedVector += acspeedVector;
                Position += speedVector;
                for (int i = 0; i < ComponentEventGroups.Count; ++i)
                    ComponentEventGroups[i].Execute(this);
            }
            else
                BindingUpdate(ExecuteEventGroups);

            Position = GetAbsolutePositionRuntime();
            return true;
        }
        public virtual void Reset()
        {
            parentAbsolutePosition = Vector2.Zero;
            if (initialState == null)
            {
                initialState = this.MemberwiseClone() as Component;
                initialState.Locals = new List<VariableResource>();
                foreach (VariableResource item in Locals)
                {
                    var variable = new VariableResource { Value = item.Value };
                    initialState.Locals.Add(variable);
                }
                initialState.ExecuteExpressions();
            }
            else
            {
                initialState.ExecuteExpressions();
                BeginFrame = initialState.BeginFrame;
                TotalFrame = initialState.TotalFrame;
                Position = initialState.Position;
                Speed = initialState.Speed;
                SpeedAngle = initialState.SpeedAngle;
                Acspeed = initialState.Acspeed;
                AcspeedAngle = initialState.AcspeedAngle;
                Visibility = initialState.Visibility;
                for (int i = 0;i < Locals.Count;++i)
                    Locals[i].Value = initialState.Locals[i].Value;
            }
            MathHelper.SetVector2(ref speedVector, Speed, SpeedAngle);
            MathHelper.SetVector2(ref acspeedVector, Acspeed, AcspeedAngle);
        }
        #endregion
    }
}
