/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    public enum EventOperator : byte
    {
        More,
        Less,
        Equal,
        And,
        Or
    }
    public enum EventKeyword : byte
    {
        ChangeTo,
        Increase,
        Decrease,
        Linear,
        Accelerated,
        Decelerated,
        Instant
    }
    enum PropertyType : byte
    {
        IllegalType,
        Boolean,
        Int32,
        Single,
        Enum,
        Vector2,
        RGB,
        String
    }
    struct TypeSet
    {
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public int enumValue;
        public Vector2 vector2Value;
        public RGB rgbValue;
        public string stringValue;
    }
    class EventInfo
    {
        public bool hasCondition;
        public string leftProperty;
        public EventOperator leftOperator;
        public PropertyType leftType;
        public TypeSet leftValue;
        public EventOperator midOperator;
        public string rightProperty;
        public EventOperator rightOperator;
        public PropertyType rightType;
        public TypeSet rightValue;
        public bool isSpecialEvent;
        public string resultProperty;
        public EventKeyword changeType;
        public bool isExpressionResult;
        public PropertyType resultType;
        public TypeSet resultValue;
        public VMInstruction[] resultExpression;
        public EventKeyword changeMode;
        public int changeTime;
        public string specialEvent;
        public string[] arguments;
        public VMInstruction[] argumentExpression;
    }
    class EventHelper
    {
        public static EventInfo BuildFromPlayData(byte[] bytes)
        {
            EventInfo eventInfo = new EventInfo();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                eventInfo.hasCondition = reader.ReadBoolean();
                if (eventInfo.hasCondition)
                {
                    eventInfo.leftProperty = PlayDataHelper.ReadString(reader);
                    eventInfo.leftOperator = (EventOperator)reader.ReadByte();
                    eventInfo.leftType = (PropertyType)reader.ReadByte();
                    eventInfo.leftValue = ReadValue(reader, eventInfo.leftType);
                    eventInfo.midOperator = (EventOperator)reader.ReadByte();
                    if (eventInfo.midOperator != 0)
                    {
                        eventInfo.rightProperty = PlayDataHelper.ReadString(reader);
                        eventInfo.rightOperator = (EventOperator)reader.ReadByte();
                        eventInfo.rightType = (PropertyType)reader.ReadByte();
                        eventInfo.rightValue = ReadValue(reader, eventInfo.rightType);
                    }
                }
                eventInfo.isSpecialEvent = reader.ReadBoolean();
                if (!eventInfo.isSpecialEvent)
                {
                    eventInfo.resultProperty = PlayDataHelper.ReadString(reader);
                    eventInfo.changeType = (EventKeyword)reader.ReadByte();
                    eventInfo.isExpressionResult = reader.ReadBoolean();
                    eventInfo.resultType = (PropertyType)reader.ReadByte();
                    if (eventInfo.isExpressionResult)
                    {
                        int length = reader.ReadInt32();
                        eventInfo.resultExpression = VM.Decode(reader.ReadBytes(length));
                    }
                    else
                        eventInfo.resultValue = ReadValue(reader, eventInfo.resultType);

                    eventInfo.changeMode = (EventKeyword)reader.ReadByte();
                    eventInfo.changeTime = reader.ReadInt32();
                }
                else
                {
                    eventInfo.specialEvent = PlayDataHelper.ReadString(reader);
                    int argumentCount = reader.ReadInt32();
                    if (eventInfo.specialEvent == "Loop")
                    {
                        int length = reader.ReadInt32();
                        eventInfo.argumentExpression = VM.Decode(reader.ReadBytes(length));
                    }
                    else
                    {
                        var arguments = new List<string>();
                        for (int i = 0; i < argumentCount; ++i)
                            arguments.Add(PlayDataHelper.ReadString(reader));

                        eventInfo.arguments = arguments.ToArray();
                    }
                }
            }
            return eventInfo;
        }
        public static TypeSet ReadValue(BinaryReader reader, PropertyType type)
        {
            var set = new TypeSet();
            switch (type)
            {
                case PropertyType.Boolean:
                    set.boolValue = reader.ReadBoolean();
                    break;
                case PropertyType.Int32:
                    set.intValue = reader.ReadInt32();
                    break;
                case PropertyType.Single:
                    set.floatValue = reader.ReadSingle();
                    break;
                case PropertyType.Enum:
                    set.enumValue = reader.ReadInt32();
                    break;
                case PropertyType.Vector2:
                    set.vector2Value = PlayDataHelper.ReadVector2(reader);
                    break;
                case PropertyType.RGB:
                    set.rgbValue = PlayDataHelper.ReadRGB(reader);
                    break;
                case PropertyType.String:
                    set.stringValue = PlayDataHelper.ReadString(reader);
                    break;
            }
            return set;
        }
        public static bool Execute(PropertyContainer propertyContainer, EventInfo eventInfo)
        {
            if (eventInfo.hasCondition)
            {
                bool result = TestCondition(propertyContainer, eventInfo.leftProperty, eventInfo.leftOperator, 
                    eventInfo.leftType, eventInfo.leftValue);
                if (eventInfo.rightProperty != null)
                {
                    if (eventInfo.midOperator == EventOperator.And)
                        result &= TestCondition(propertyContainer, eventInfo.rightProperty, eventInfo.rightOperator, 
                            eventInfo.rightType, eventInfo.rightValue);
                    else if (eventInfo.midOperator == EventOperator.Or)
                        result |= TestCondition(propertyContainer, eventInfo.rightProperty, eventInfo.rightOperator, 
                            eventInfo.rightType, eventInfo.rightValue);
                }
                if (!result)
                    return false;
            }
            if (!eventInfo.isSpecialEvent)
            {
                EventManager.AddEvent(propertyContainer, eventInfo);
                return false;
            }
            else
            {
                return EventManager.ExecuteSpecialEvent(propertyContainer, eventInfo.specialEvent, eventInfo.arguments,
                    eventInfo.argumentExpression);
            }
        }
        static bool TestCondition(PropertyContainer propertyContainer, string property, EventOperator operators, 
            PropertyType type, TypeSet value)
        {
            propertyContainer.PushProperty(property);
            switch (type)
            {
                case PropertyType.Boolean:
                    return VM.PopBool() == value.boolValue;
                case PropertyType.Int32:
                    if (operators == EventOperator.Less)
                        return VM.PopInt() < value.intValue;
                    else if (operators == EventOperator.Equal)
                        return VM.PopInt() == value.intValue;
                    else
                        return VM.PopInt() > value.intValue;
                case PropertyType.Single:
                    if (operators == EventOperator.Less)
                        return VM.PopFloat() < value.floatValue;
                    else if (operators == EventOperator.Equal)
                        return VM.PopFloat() == value.floatValue;
                    else
                        return VM.PopFloat() > value.floatValue;
                case PropertyType.Enum:
                    return VM.PopEnum() == value.enumValue;
                case PropertyType.Vector2:
                    return VM.PopVector2() == value.vector2Value;
                case PropertyType.RGB:
                    return VM.PopRGB() == value.rgbValue;
                case PropertyType.String:
                    return VM.PopString() == value.stringValue;
                default:
                    return false;
            }
        }
    }
}
