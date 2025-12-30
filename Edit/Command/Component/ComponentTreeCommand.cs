/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrazyStorm.Core;

namespace CrazyStorm
{
    class ComponentTreeCommand : Command
    {
        public override void Redo(CommandStack stack)
        {
            base.Redo(stack);
            var selectedSystem = Parameter[0] as ParticleSystem;
            var sourceComponent = Parameter[1] as Component;
            var targetComponent = Parameter[2] as Component;
            var updateProperty = Parameter[3] as Action;
            ChangeRelationship(selectedSystem, sourceComponent, targetComponent);
            updateProperty();
        }
        public override void Undo(CommandStack stack)
        {
            base.Undo(stack);
            var selectedSystem = Parameter[0] as ParticleSystem;
            var sourceComponent = Parameter[1] as Component;
            var targetComponent = Parameter[2] as Component;
            var updateProperty = Parameter[3] as Action;
            ChangeRelationship(selectedSystem, sourceComponent, targetComponent);
            updateProperty();
        }
        void ChangeRelationship(ParticleSystem selectedSystem, Component source, Component target)
        {
            if (target.Children.Contains(source))
            {
                //A way to change the node from parenthood to brotherhood. 
                var tree = new Component();
                foreach (Component item in selectedSystem.ComponentTree) tree.Children.Add(item);
                var parent = tree.FindParent(target);
                if (parent != null)
                {
                    if (parent == tree)
                    {
                        selectedSystem.ComponentTree.Add(source);
                        source.TransPositiontoAbsolute();
                        source.Parent = null;
                    }
                    else
                    {
                        parent.Children.Add(source);
                        source.TransPositiontoAbsolute();
                        source.Parent = parent;
                        source.TransPositiontoRelative();
                    }
                    target.Children.Remove(source);
                }
            }
            else
            {
                //Add source component to target component as child
                var tree = new Component();
                foreach (Component item in selectedSystem.ComponentTree) tree.Children.Add(item);
                var parent = tree.FindParent(source);
                if (parent != null)
                {
                    if (parent == tree) selectedSystem.ComponentTree.Remove(source);
                    else parent.Children.Remove(source);
                    target.Children.Add(source);
                    source.TransPositiontoAbsolute();
                    source.Parent = target;
                    source.TransPositiontoRelative();
                }
            }
        }
    }
}
