/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public class EventExecutor
    {
        TypeSet currentValue;
        float currentTime;
        public PropertyContainer PropertyContainer { get; set; }
        public long PropertyContainerID { get; set; }
        public PropertyContainer BindingContainer { get; set; }
        public long BindingContainerID { get; set; }
        public int PropertyID { get; set; }
        public EventChangeMode ChangeMode { get; set; }
        public EventChangeType ChangeType { get; set; }
        public TypeSet CurrentValue => currentValue;
        public TypeSet TargetValue { get; set; }
        public int ChangeTime { get; set; }
        public bool Finished => currentTime >= ChangeTime;
        public bool Invalid => (PropertyContainer.ID != PropertyContainerID) || 
            (BindingContainer != null && BindingContainer.ID != BindingContainerID);
        public EventExecutorPool PoolObject { get; set; }
        public EventExecutor()
        {
            Reset();
        }
        public void Reset()
        {
            currentValue = new TypeSet();
            currentTime = 0;
        }
        public void Update(float frameScale)
        {
            float ratio = 0;
            var sign = ChangeType != EventChangeType.Decrease ? 1f : -1f;
            ratio = frameScale * sign;
            //Use derivatives of different lerps
            ratio *= 1f / ChangeTime;
            if (ChangeMode == EventChangeMode.Accelerated) ratio *= 2 * currentTime / ChangeTime;
            else if (ChangeMode == EventChangeMode.Decelerated) ratio *= (2 - 2 * currentTime / ChangeTime);
            else if (ChangeMode == EventChangeMode.Sin) ratio *= (float)(Math.Cos(currentTime / ChangeTime * Math.PI * 2) * Math.PI * 2);
            else if (ChangeMode == EventChangeMode.Cos) ratio *= -(float)(Math.Sin(currentTime / ChangeTime * Math.PI * 2) * Math.PI * 2);
            else if (ChangeMode == EventChangeMode.Instant) ratio *= ChangeTime;
            ratio = Math.Min(1, ratio);
            currentValue.type = TargetValue.type;
            switch (TargetValue.type)
            {
                case PropertyType.Boolean:
                    currentValue.boolValue = TargetValue.boolValue;
                    VM.PushBool(CurrentValue.boolValue);
                    break;
                case PropertyType.Int32:
                    PropertyContainer.PushProperty(PropertyID);
                    currentValue.intValue = VM.PopInt();
                    currentValue.intValue = (int)(currentValue.intValue + ratio * TargetValue.intValue);
                    VM.PushFloat(CurrentValue.intValue);
                    break;
                case PropertyType.Single:
                    PropertyContainer.PushProperty(PropertyID);
                    currentValue.floatValue = VM.PopFloat();
                    currentValue.floatValue = currentValue.floatValue + ratio * TargetValue.floatValue;
                    VM.PushFloat(currentValue.floatValue);
                    break;
                case PropertyType.Enum:
                    currentValue.enumValue = TargetValue.enumValue;
                    VM.PushInt(currentValue.enumValue);
                    break;
                case PropertyType.Vector2:
                    PropertyContainer.PushProperty(PropertyID);
                    currentValue.vector2Value = VM.PopVector2();
                    currentValue.vector2Value = currentValue.vector2Value + TargetValue.vector2Value * ratio;
                    VM.PushVector2(currentValue.vector2Value);
                    break;
                case PropertyType.RGB:
                    PropertyContainer.PushProperty(PropertyID);
                    currentValue.rgbValue = VM.PopRGB();
                    currentValue.rgbValue = currentValue.rgbValue + TargetValue.rgbValue * ratio;
                    VM.PushRGB(currentValue.rgbValue);
                    break;
                case PropertyType.String:
                    currentValue.stringValue = TargetValue.stringValue;
                    VM.PushString(currentValue.stringValue);
                    break;
            }
            PropertyContainer.SetProperty(PropertyID);
            VM.Clear();
            currentTime += frameScale;
        }
    }
}
