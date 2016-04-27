/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CrazyStorm.Core
{
    public struct ComponentData : IFieldData
    {
        public int currentFrame;
        public int beginFrame;
        public int totalFrame;
        public Vector2 position;
        public float speed;
        public float speedAngle;
        public float acspeed;
        public float acspeedAngle;
        public bool visibility;
        public void SetField(int fieldIndex, ValueType value)
        {
            throw new NotImplementedException();
        }
        public ValueType GetField(int fieldIndex)
        {
            throw new NotImplementedException();
        }
    }
    public class Component : PropertyContainer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        ComponentData componentData;
        string name;
        bool selected;
        Component parent;
        Component bindingTarget;
        IList<VariableResource> variables;
        IList<EventGroup> componentEventGroups;
        IList<Component> children;
        #endregion

        #region Public Members
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
        public Component BindingTarget
        {
            get { return bindingTarget; }
            set { bindingTarget = value; }
        }
        public IList<VariableResource> Variables { get { return variables; } }
        public IList<EventGroup> ComponentEventGroups { get { return componentEventGroups; } }
        public IList<Component> Children { get { return children; } }
        #endregion

        #region Constructor
        public Component()
        {
            componentData.visibility = true;
            variables = new ObservableCollection<VariableResource>();
            componentEventGroups = new ObservableCollection<EventGroup>();
            children = new ObservableCollection<Component>();
        }
        #endregion

        #region Public Methods
        public void TransPositiontoRelative()
        {
            if (parent != null)
            {
                componentData.position -= parent.GetAbsolutePosition();
            }
        }
        public void TransPositiontoAbsolute()
        {
            if (parent != null)
            {
                componentData.position += parent.GetAbsolutePosition();
            }
        }
        public Vector2 GetAbsolutePosition()
        {
            if (parent != null)
            {
                return componentData.position + parent.GetAbsolutePosition();
            }
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
            clone.bindingTarget = null;
            clone.variables = new ObservableCollection<VariableResource>();
            foreach (var item in variables)
                clone.variables.Add(item.Clone() as VariableResource);

            clone.componentEventGroups = new ObservableCollection<EventGroup>();
            foreach (var item in componentEventGroups)
                clone.componentEventGroups.Add(item.Clone() as EventGroup);

            clone.children = new ObservableCollection<Component>();
            return clone;
        }
        #endregion
    }
}
