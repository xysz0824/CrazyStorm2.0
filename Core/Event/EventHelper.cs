/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;

namespace CrazyStorm.Core
{
    public enum EventChangeType : byte
    {
        ChangeTo,
        Increase,
        Decrease,
    }
    public enum EventChangeMode : byte
    {
        Linear,
        Accelerated,
        Decelerated,
        Sin,
        Cos,
        Instant
    }
    public enum PropertyType : byte
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
    public struct TypeSet
    {
        public PropertyType type;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public int enumValue;
        public Vector2 vector2Value;
        public RGB rgbValue;
        public string stringValue;
    }
    public class EventInfo
    {
        public string condition;
        public bool isSpecialEvent;
        public string resultProperty;
        public string changeType;
        public bool isExpressionResult;
        public PropertyType resultType;
        public string resultValue;
        public string changeMode;
        public string changeTime;
        public string specialEvent;
        public string arguments;
    }
    public class VMEventInfo
    {
        public VMInstruction[] conditionExpression;
        public bool isSpecialEvent;
        public string resultProperty;
        public EventChangeType changeType;
        public bool isExpressionResult;
        public PropertyType resultType;
        public TypeSet resultValue;
        public VMInstruction[] resultExpression;
        public EventChangeMode changeMode;
        public int changeTime;
        public string specialEvent;
        public string[] arguments;
        public VMInstruction[][] argumentExpressions;
    }
    public class EventHelper
    {
        public static string BuildEvent(EventInfo eventInfo, bool addTypeFlag)
        {
            StringBuilder eventString = new StringBuilder();
            if (!string.IsNullOrEmpty(eventInfo.condition))
            {
                eventString.Append(eventInfo.condition);
                eventString.Append(": ");
            }
            if (!eventInfo.isSpecialEvent)
            {
                if (eventInfo.isExpressionResult) eventInfo.resultValue = $"({eventInfo.resultValue})";
                eventString.Append(string.Format("{0} {1} {2}, {3}, {4}", eventInfo.resultProperty, eventInfo.changeType,
                    eventInfo.resultValue, eventInfo.changeMode, eventInfo.changeTime));
            }
            else eventString.Append(string.Format("{0}({1})", eventInfo.specialEvent, eventInfo.arguments));
            if (addTypeFlag) eventString.Append((char)eventInfo.resultType);
            return eventString.ToString();
        }
        public static EventInfo SplitEvent(string text)
        {
            var info = new EventInfo();
            string[] parts = text.Split(':');
            string eventText = string.Empty;
            if (parts.Length == 2)
            {
                info.condition = parts[0];
                eventText = parts[1].Trim();
            }
            else
            {
                eventText = parts[0].Trim();
            }
            var changeTypeNames = Enum.GetNames(typeof(EventChangeType));
            var changeModeNames = Enum.GetNames(typeof(EventChangeMode));
            string[] split = eventText.Split(' ');
            var changeType = changeTypeNames.FirstOrDefault((str) => Array.IndexOf(split, str) != -1);
            if (changeType != null)
            {
                info.resultProperty = split[0];
                info.changeType = split[1];
                info.resultValue = string.Empty;
                split = Regex.Split(eventText, split[1])[1].Split(',');
                for (int i = 0; i < split.Length; ++i)
                {
                    split[i] = split[i].Trim();
                    var changeMode = changeModeNames.FirstOrDefault((str) => split[i] == str);
                    if (changeMode != null)
                    {
                        info.changeMode = split[i];
                        string temp = split[i + 1].Trim();
                        info.resultType = (PropertyType)temp[temp.Length - 1];
                        info.changeTime = temp.Remove(temp.Length - 1, 1);
                        break;
                    }
                    else
                        info.resultValue += split[i] + ",";
                }
                info.resultValue = info.resultValue.Remove(info.resultValue.Length - 1);
                if (info.resultValue.StartsWith("("))
                {
                    info.isExpressionResult = true;
                    info.resultValue = info.resultValue.Remove(0, 1);
                    info.resultValue = info.resultValue.Remove(info.resultValue.Length - 1);
                }
            }
            else
            {
                info.isSpecialEvent = true;
                split = eventText.Split('(');
                info.specialEvent = split[0];
                split = split[1].Split(')');
                info.arguments = split[0];
            }
            return info;
        }
        public static byte[] Compile(string str)
        {
            var lexer = new Expression.Lexer();
            lexer.Load(str);
            var compiledBytes = new List<byte>();
            if (lexer.Tokens.Count > 0)
            {
                var syntaxTree = new Expression.Parser(lexer).Expression();
                syntaxTree.Compile(compiledBytes);
            }
            return compiledBytes.ToArray();
        }
        public static byte[] GenerateEventData(string text)
        {
            EventInfo eventInfo = SplitEvent(text);
            List<byte> bytes = new List<byte>();
            if (eventInfo.condition != null)
            {
                byte[] compiledExpression = Compile(eventInfo.condition);
                bytes.AddRange(BitConverter.GetBytes(compiledExpression.Length));
                bytes.AddRange(compiledExpression);
            }
            else
            {
                bytes.AddRange(BitConverter.GetBytes(0));
            }
            bytes.AddRange(BitConverter.GetBytes(eventInfo.isSpecialEvent));
            if (!eventInfo.isSpecialEvent)
            {
                bytes.AddRange(PlayDataHelper.GetStringBytes(eventInfo.resultProperty));
                bytes.Add((byte)Enum.Parse(typeof(EventChangeType), eventInfo.changeType));
                bytes.AddRange(BitConverter.GetBytes(eventInfo.isExpressionResult));
                bytes.Add((byte)eventInfo.resultType);
                if (eventInfo.isExpressionResult)
                {
                    byte[] compiledExpression = Compile(eventInfo.resultValue);
                    bytes.AddRange(BitConverter.GetBytes(compiledExpression.Length));
                    bytes.AddRange(compiledExpression);
                }
                else
                {
                    bytes.AddRange(GetBytes(eventInfo.resultType,
                        PropertyTypeRule.Parse(eventInfo.resultType, eventInfo.resultProperty, eventInfo.resultValue)));
                }
                bytes.Add((byte)Enum.Parse(typeof(EventChangeMode), eventInfo.changeMode));
                bytes.AddRange(BitConverter.GetBytes(int.Parse(eventInfo.changeTime)));
            }
            else
            {
                bytes.AddRange(PlayDataHelper.GetStringBytes(eventInfo.specialEvent));
                string[] split = eventInfo.arguments.Split(',');
                bytes.AddRange(BitConverter.GetBytes(split.Length));
                for (int i = 0; i < split.Length; ++i)
                {
                    bytes.AddRange(PlayDataHelper.GetStringBytes(split[i]));
                    byte[] compiledExpression = Compile(split[i]);
                    bytes.AddRange(BitConverter.GetBytes(compiledExpression.Length));
                    bytes.AddRange(compiledExpression);
                }
            }
            return bytes.ToArray();
        }
        public static VMEventInfo BuildFromPlayData(byte[] bytes)
        {
            VMEventInfo eventInfo = new VMEventInfo();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var conditionLength = reader.ReadInt32();
                if (conditionLength > 0)
                {
                    eventInfo.conditionExpression = VM.Decode(reader.ReadBytes(conditionLength));
                }
                eventInfo.isSpecialEvent = reader.ReadBoolean();
                if (!eventInfo.isSpecialEvent)
                {
                    eventInfo.resultProperty = PlayDataHelper.ReadString(reader);
                    eventInfo.changeType = (EventChangeType)reader.ReadByte();
                    eventInfo.isExpressionResult = reader.ReadBoolean();
                    eventInfo.resultType = (PropertyType)reader.ReadByte();
                    if (eventInfo.isExpressionResult)
                    {
                        int length = reader.ReadInt32();
                        eventInfo.resultExpression = VM.Decode(reader.ReadBytes(length));
                    }
                    else
                        eventInfo.resultValue = ReadValue(reader, eventInfo.resultType);

                    eventInfo.changeMode = (EventChangeMode)reader.ReadByte();
                    eventInfo.changeTime = reader.ReadInt32();
                }
                else
                {
                    eventInfo.specialEvent = PlayDataHelper.ReadString(reader);
                    int argumentCount = reader.ReadInt32();
                    var arguments = new List<string>();
                    var argumentExpressions = new List<VMInstruction[]>();
                    for (int i = 0; i < argumentCount; ++i)
                    {
                        arguments.Add(PlayDataHelper.ReadString(reader));
                        int length = reader.ReadInt32();
                        argumentExpressions.Add(VM.Decode(reader.ReadBytes(length)));
                    }
                    eventInfo.arguments = arguments.ToArray();
                    eventInfo.argumentExpressions = argumentExpressions.ToArray();
                }
            }
            return eventInfo;
        }
        public static byte[] GetBytes(PropertyType type, object value)
        {
            switch (type)
            {
                case PropertyType.Boolean:
                    return BitConverter.GetBytes((bool)value);
                case PropertyType.Int32:
                    return BitConverter.GetBytes((int)value);
                case PropertyType.Single:
                    return BitConverter.GetBytes((float)value);
                case PropertyType.Enum:
                    return BitConverter.GetBytes((int)value);
                case PropertyType.Vector2:
                    return PlayDataHelper.GetStructBytes((Vector2)value);
                case PropertyType.RGB:
                    return PlayDataHelper.GetStructBytes((RGB)value);
                case PropertyType.String:
                    return PlayDataHelper.GetStringBytes((string)value);
            }
            return new byte[0];
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
                    set.vector2Value = PlayDataHelper.ReadStruct<Vector2>(reader);
                    break;
                case PropertyType.RGB:
                    set.rgbValue = PlayDataHelper.ReadStruct<RGB>(reader);
                    break;
                case PropertyType.String:
                    set.stringValue = PlayDataHelper.ReadString(reader);
                    break;
            }
            return set;
        }
        public static bool Execute(PropertyContainer propertyContainer, PropertyContainer bindingContainer, VMEventInfo eventInfo)
        {
            if (eventInfo.conditionExpression != null)
            {
                VM.Execute(propertyContainer, eventInfo.conditionExpression);
                bool result = VM.PopBool();
                if (!result) return false;
            }
            if (!eventInfo.isSpecialEvent)
            {
                EventManager.AddEvent(propertyContainer, bindingContainer, eventInfo);
                return false;
            }
            else
            {
                return EventManager.ExecuteSpecialEvent(propertyContainer, eventInfo.specialEvent, eventInfo.arguments,
                    eventInfo.argumentExpressions);
            }
        }
        public static bool IsSpecialEvent(string str)
        {
            var changeTypeNames = Enum.GetNames(typeof(EventChangeType));
            foreach (var name in changeTypeNames)
            {
                if (str.Contains($" {name} "))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
