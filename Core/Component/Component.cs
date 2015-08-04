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
        int beginFrame;
        int totalFrame;
        int minFrame;
        int maxFrame;
        Vector2 position;
        float speed;
        float speedAngle;
        float acspeed;
        float acspeedAngle;
        bool enable;
        bool selected;
        ObservableCollection<VariableResource> variables;
        ObservableCollection<EventGroup> componentEventGroups;
        ObservableCollection<Component> children;
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
        [IntProperty(0, int.MaxValue)]
        public int BeginFrame
        {
            get { return beginFrame; }
            set { beginFrame = value >= minFrame ? value : minFrame; }
        }
        [IntProperty(0, int.MaxValue)]
        public int TotalFrame
        {
            get { return totalFrame; }
            set { totalFrame = value <= maxFrame ? value : maxFrame; }
        }
        [Vector2Property]
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
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
        [BoolProperty]
        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
        public ObservableCollection<VariableResource> Variables { get { return variables; } }
        public ObservableCollection<EventGroup> ComponentEventGroups { get { return componentEventGroups; } }
        public ObservableCollection<Component> Children { get { return children; } }
        #endregion

        #region Constructor
        public Component()
        {
            enable = true;
            variables = new ObservableCollection<VariableResource>();
            componentEventGroups = new ObservableCollection<EventGroup>();
            children = new ObservableCollection<Component>();
        }
        #endregion

        #region Public Methods
        public void InitializeFrameRange(int minFrame, int maxFrame)
        {
            this.minFrame = minFrame;
            this.maxFrame = maxFrame;
            beginFrame = minFrame;
            totalFrame = maxFrame;
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
        public void SetFrameRange(int minFrame, int maxFrame)
        {
            this.minFrame = minFrame;
            this.maxFrame = maxFrame;
            beginFrame = beginFrame <= minFrame ? minFrame : beginFrame;
            totalFrame = totalFrame >= maxFrame ? maxFrame : totalFrame;
        }
        #endregion
    }
}
