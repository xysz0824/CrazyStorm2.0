/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class EventExecutor
    {
        float currentTime;
        public PropertyContainer PropertyContainer { get; set; }
        public string PropertyName { get; set; }
        public PropertyType PropertyType { get; set; }
        public EventKeyword ChangeMode { get; set; }
        public TypeSet InitialValue { get; set; }
        public TypeSet TargetValue { get; set; }
        public int ChangeTime { get; set; }
        public bool Finished { get { return currentTime + 1 == ChangeTime; } }
        public void Update()
        {
            float ratio = (currentTime + 1) / ChangeTime;
            if (ChangeMode == EventKeyword.Accelerated)
                ratio *= ratio;
            else if (ChangeMode == EventKeyword.Decelerated)
                ratio *= (2 - ratio);
            else if (ChangeMode == EventKeyword.Instant)
                ratio = 1;

            switch (PropertyType)
            {
                case PropertyType.Boolean:
                    VM.PushBool(TargetValue.boolValue);
                    break;
                case PropertyType.Int32:
                    VM.PushFloat((int)((1 - ratio) * InitialValue.intValue + ratio * TargetValue.intValue));
                    break;
                case PropertyType.Single:
                    VM.PushFloat((1 - ratio) * InitialValue.floatValue + ratio * TargetValue.floatValue);
                    break;
                case PropertyType.Enum:
                    VM.PushEnum(TargetValue.enumValue);
                    break;
                case PropertyType.Vector2:
                    Vector2 newVector2;
                    newVector2.x = (1 - ratio) * InitialValue.vector2Value.x + ratio * TargetValue.vector2Value.x;
                    newVector2.y = (1 - ratio) * InitialValue.vector2Value.y + ratio * TargetValue.vector2Value.y;
                    VM.PushVector2(newVector2);
                    break;
                case PropertyType.RGB:
                    RGB newRGB;
                    newRGB.r = (1 - ratio) * InitialValue.rgbValue.r + ratio * TargetValue.rgbValue.r;
                    newRGB.g = (1 - ratio) * InitialValue.rgbValue.g + ratio * TargetValue.rgbValue.g;
                    newRGB.b = (1 - ratio) * InitialValue.rgbValue.b + ratio * TargetValue.rgbValue.b;
                    VM.PushRGB(newRGB);
                    break;
                case PropertyType.String:
                    VM.PushString(TargetValue.stringValue);
                    break;
            }
            PropertyContainer.SetProperty(PropertyName);
            currentTime++;
        }
    }
}
