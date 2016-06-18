using System;
using System.Collections.Generic;
using System.Linq;
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
    class EventManager
    {
        static List<EventExecutor> executorList;
        public static List<ParticleType> DefaultTypes { get; set; }
        public static void AddEvent(PropertyContainer propertyContainer, EventInfo eventInfo)
        {
            if (executorList == null)
                executorList = new List<EventExecutor>();

            var executor = new EventExecutor();
            executor.PropertyContainer = propertyContainer;
            executor.PropertyName = eventInfo.resultProperty;
            executor.PropertyType = eventInfo.resultType;
            executor.ChangeMode = eventInfo.changeMode;
            executor.ChangeTime = eventInfo.changeTime;
            propertyContainer.PushProperty(executor.PropertyName);
            var initialValue = new TypeSet();
            var targetValue = eventInfo.resultValue;
            if (eventInfo.isExpressionResult)
            {
                VM.Execute(propertyContainer, eventInfo.resultExpression);
                switch (eventInfo.resultType)
                {
                    case PropertyType.Boolean:
                        targetValue.boolValue = VM.PopBool();
                        initialValue.boolValue = VM.PopBool();
                        break;
                    case PropertyType.Int32:
                        int resultInt = (int)VM.PopFloat();
                        initialValue.intValue = (int)VM.PopFloat();
                        if (eventInfo.changeType == EventKeyword.ChangeTo)
                            targetValue.intValue = resultInt;
                        else if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.intValue = initialValue.intValue + resultInt;
                        else
                            targetValue.intValue = initialValue.intValue - resultInt;

                        break;
                    case PropertyType.Single:
                        float resultFloat = VM.PopFloat();
                        initialValue.floatValue = VM.PopFloat();
                        if (eventInfo.changeType == EventKeyword.ChangeTo)
                            targetValue.floatValue = resultFloat;
                        else if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.floatValue = initialValue.floatValue + resultFloat;
                        else
                            targetValue.floatValue = initialValue.floatValue - resultFloat;

                        break;
                    case PropertyType.Enum:
                        targetValue.enumValue = VM.PopEnum();
                        initialValue.enumValue = VM.PopEnum();
                        break;
                    case PropertyType.Vector2:
                        Vector2 resultVector2 = VM.PopVector2();
                        initialValue.vector2Value = VM.PopVector2();
                        if (eventInfo.changeType == EventKeyword.ChangeTo)
                            targetValue.vector2Value = resultVector2;
                        else if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.vector2Value = initialValue.vector2Value + resultVector2;
                        else
                            targetValue.vector2Value = initialValue.vector2Value - resultVector2;

                        break;
                    case PropertyType.RGB:
                        RGB resultRGB = VM.PopRGB();
                        initialValue.rgbValue = VM.PopRGB();
                        if (eventInfo.changeType == EventKeyword.ChangeTo)
                            targetValue.rgbValue = resultRGB;
                        else if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.rgbValue = initialValue.rgbValue + resultRGB;
                        else
                            targetValue.rgbValue = initialValue.rgbValue - resultRGB;

                        break;
                    case PropertyType.String:
                        targetValue.stringValue = VM.PopString();
                        initialValue.stringValue = VM.PopString();
                        break;
                }
            }
            else
            {
                switch (eventInfo.resultType)
                {
                    case PropertyType.Boolean:
                        initialValue.boolValue = VM.PopBool();
                        break;
                    case PropertyType.Int32:
                        initialValue.intValue = VM.PopInt();
                        if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.intValue = initialValue.intValue + targetValue.intValue;
                        else if (eventInfo.changeType == EventKeyword.Decrease)
                            targetValue.intValue = initialValue.intValue - targetValue.intValue;

                        break;
                    case PropertyType.Single:
                        initialValue.floatValue = VM.PopFloat();
                        if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.floatValue = initialValue.floatValue + targetValue.floatValue;
                        else if (eventInfo.changeType == EventKeyword.Decrease)
                            targetValue.floatValue = initialValue.floatValue - targetValue.floatValue;

                        break;
                    case PropertyType.Enum:
                        initialValue.enumValue = VM.PopEnum();
                        break;
                    case PropertyType.Vector2:
                        initialValue.vector2Value = VM.PopVector2();
                        if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.vector2Value = initialValue.vector2Value + targetValue.vector2Value;
                        else if (eventInfo.changeType == EventKeyword.Decrease)
                            targetValue.vector2Value = initialValue.vector2Value - targetValue.vector2Value;

                        break;
                    case PropertyType.RGB:
                        initialValue.rgbValue = VM.PopRGB();
                        if (eventInfo.changeType == EventKeyword.Increase)
                            targetValue.rgbValue = initialValue.rgbValue + targetValue.rgbValue;
                        else if (eventInfo.changeType == EventKeyword.Decrease)
                            targetValue.rgbValue = initialValue.rgbValue - targetValue.rgbValue;

                        break;
                    case PropertyType.String:
                        initialValue.stringValue = VM.PopString();
                        break;
                }
            }
            executor.InitialValue = initialValue;
            executor.TargetValue = targetValue;
            if (executor.ChangeMode == EventKeyword.Instant)
                executor.Update();
            else
                executorList.Add(executor);
        }
        public static bool ExecuteSpecialEvent(PropertyContainer propertyContainer, string eventName, string[] arguments, 
            VMInstruction[] argumentExpression)
        {
            switch (eventName)
            {
                case "EmitParticle":
                    (propertyContainer as Emitter).EmitParticle();
                    break;
                case "PlaySound":
                    break;
                case "Loop":
                    VM.Execute(propertyContainer, argumentExpression);
                    if (!VM.PopBool())
                        return true;

                    break;
                case "ChangeType":
                    int typeId = int.Parse(arguments[0]);
                    if (typeId >= ParticleType.DefaultTypeIndex)
                    {
                        if (propertyContainer is Emitter)
                            (propertyContainer as Emitter).Template.Type = DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        else if (propertyContainer is ParticleBase)
                            (propertyContainer as ParticleBase).Type = DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                    }
                    else
                    {
                        //TODO
                    }
                    break;
            }
            return false;
        }
        public static void Update()
        {
            if (executorList == null)
                return;

            for (int i = 0; i < executorList.Count;++i)
            {
                executorList[i].Update();
                if (executorList[i].Finished)
                {
                    executorList.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
