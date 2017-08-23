/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CrazyStorm.Core
{
    public class EventExecutor
    {
        TypeSet currentValue;
        float currentTime;
        public PropertyContainer PropertyContainer { get; set; }
        public PropertyContainer BindingContainer { get; set; }
        public string PropertyName { get; set; }
        public EventKeyword ChangeMode { get; set; }
        public TypeSet InitialValue { get; set; }
        public TypeSet CurrentValue { get { return currentValue; } }
        public TypeSet TargetValue { get; set; }
        public int ChangeTime { get; set; }
        public bool Finished { get { return currentTime >= ChangeTime; } }
        public EventExecutor()
        {
            currentValue = new TypeSet();
        }
        public void Update()
        {
            float ratio = (currentTime + 1) / ChangeTime;
            if (ChangeMode == EventKeyword.Accelerated)
                ratio *= ratio;
            else if (ChangeMode == EventKeyword.Decelerated)
                ratio *= (2 - ratio);

            currentValue.type = InitialValue.type;
            switch (InitialValue.type)
            {
                case PropertyType.Boolean:
                    currentValue.boolValue = TargetValue.boolValue;
                    VM.PushBool(CurrentValue.boolValue);
                    break;
                case PropertyType.Int32:
                    currentValue.intValue = (int)((1 - ratio) * InitialValue.intValue + ratio * TargetValue.intValue);
                    VM.PushFloat(CurrentValue.intValue);
                    break;
                case PropertyType.Single:
                    currentValue.floatValue = (1 - ratio) * InitialValue.floatValue + ratio * TargetValue.floatValue;
                    VM.PushFloat(currentValue.floatValue);
                    break;
                case PropertyType.Enum:
                    currentValue.enumValue = TargetValue.enumValue;
                    VM.PushEnum(currentValue.enumValue);
                    break;
                case PropertyType.Vector2:
                    Vector2 newVector2;
                    newVector2.x = (1 - ratio) * InitialValue.vector2Value.x + ratio * TargetValue.vector2Value.x;
                    newVector2.y = (1 - ratio) * InitialValue.vector2Value.y + ratio * TargetValue.vector2Value.y;
                    currentValue.vector2Value = newVector2;
                    VM.PushVector2(currentValue.vector2Value);
                    break;
                case PropertyType.RGB:
                    RGB newRGB;
                    newRGB.r = (1 - ratio) * InitialValue.rgbValue.r + ratio * TargetValue.rgbValue.r;
                    newRGB.g = (1 - ratio) * InitialValue.rgbValue.g + ratio * TargetValue.rgbValue.g;
                    newRGB.b = (1 - ratio) * InitialValue.rgbValue.b + ratio * TargetValue.rgbValue.b;
                    currentValue.rgbValue = newRGB;
                    VM.PushRGB(currentValue.rgbValue);
                    break;
                case PropertyType.String:
                    currentValue.stringValue = TargetValue.stringValue;
                    VM.PushString(currentValue.stringValue);
                    break;
            }
            PropertyContainer.SetProperty(PropertyName);
            currentTime++;
        }
    }
}
