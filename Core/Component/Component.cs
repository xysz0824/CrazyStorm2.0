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
    public class Component : PropertyContainer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Members
        string name;
        int currentFrame;
        int beginFrame;
        int totalFrame;
        Vector2 position;
        float speed;
        float speedAngle;
        float acspeed;
        float acspeedAngle;
        bool visibility;
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
            get { return currentFrame; }
            set { currentFrame = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int BeginFrame
        {
            get { return beginFrame; }
            set { beginFrame = value; }
        }
        [IntProperty(0, int.MaxValue)]
        public int TotalFrame
        {
            get { return totalFrame; }
            set { totalFrame = value; }
        }
        [Vector2Property]
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public float X
        {
            get { return position.x; }
            set { position.x = value; }
        }
        public float Y
        {
            get { return position.y; }
            set { position.y = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float SpeedAngle
        {
            get { return speedAngle; }
            set { speedAngle = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float Acspeed
        {
            get { return acspeed; }
            set { acspeed = value; }
        }
        [FloatProperty(float.MinValue, float.MaxValue)]
        public float AcspeedAngle
        {
            get { return acspeedAngle; }
            set { acspeedAngle = value; }
        }
        [BoolProperty]
        public bool Visibility
        {
            get { return visibility; }
            set { visibility = value; }
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
            visibility = true;
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
                position -= parent.GetAbsolutePosition();
            }
        }
        public void TransPositiontoAbsolute()
        {
            if (parent != null)
            {
                position += parent.GetAbsolutePosition();
            }
        }
        public Vector2 GetAbsolutePosition()
        {
            if (parent != null)
            {
                return position + parent.GetAbsolutePosition();
            }
            return position;
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
