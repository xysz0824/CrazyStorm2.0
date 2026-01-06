/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CrazyStorm.Core
{
    public static class EventManager
    {
        public static readonly Dictionary<string, Func<PropertyContainer, string[], VMInstruction[], bool>> SpecialEvents =
            new Dictionary<string, Func<PropertyContainer, string[], VMInstruction[], bool>>()
            {
                { "EmitParticle", (pc, args, expr) => 
                {
                    (pc as Emitter).EmitParticle();
                    return false;
                } },
                { "PlaySound", (pc, args, expr) =>
                {
                    if (OnSoundPlay != null)
                    {
                        var label = args[0];
                        var volume = float.Parse(args[1]);
                        var sound = Sounds.FirstOrDefault((item) => string.Equals(item.Label, label));
                        if (sound != null) OnSoundPlay(sound.AbsolutePath, volume);
                    }
                    return false;
                } },
                { "Loop", (pc, args, expr) =>
                {
                    VM.Execute(pc, expr);
                    if (!VM.PopBool()) return true;
                    return false;
                } },
                { "ChangeType", (pc, args, expr) =>
                {
                    int typeId = int.Parse(args[0]) + int.Parse(args[1]);
                    if (typeId >= ParticleType.DefaultTypeIndex)
                    {
                        if (pc is Emitter) (pc as Emitter).Template.Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        else if (pc is ParticleBase) (pc as ParticleBase).Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                    }
                    else
                    {
                        if (pc is Emitter) (pc as Emitter).Template.Type = CustomTypes[typeId];
                        else if (pc is ParticleBase) (pc as ParticleBase).Type = CustomTypes[typeId];
                    }
                    return false;
                } },
                { "GotoFrame", (pc, args, expr) =>
                {
                    //TODO : GotoFrame
                    return false;
                } },
                { "QuakeScreen", (pc, args, expr) =>
                {
                    //TODO : QuakeScreen
                    return false;
                } },
                { "StopScreen", (pc, args, expr) =>
                {
                    //TODO : StopScreen
                    return false;
                } },
            };
        public delegate void SoundPlayHandler(string path, float volume);
        public static event SoundPlayHandler OnSoundPlay;

        static List<EventExecutor> executorList;
        static Dictionary<string, Dictionary<string, TypeSet>> cache;
        public static IList<ParticleType> CustomTypes { get; set; }
        public static IList<FileResource> Sounds { get; set; }
        public static void Initialize()
        {
            OnSoundPlay = null;
            executorList = new List<EventExecutor>();
            cache = new Dictionary<string, Dictionary<string, TypeSet>>();
        }
        public static void AddEvent(PropertyContainer propertyContainer, PropertyContainer bindingContainer, VMEventInfo eventInfo)
        {
            var executor = new EventExecutor();
            executor.PropertyContainer = propertyContainer;
            executor.BindingContainer = bindingContainer;
            executor.PropertyName = eventInfo.resultProperty;
            executor.ChangeMode = eventInfo.changeMode;
            executor.ChangeTime = eventInfo.changeTime;
            propertyContainer.PushProperty(executor.PropertyName);
            var initialValue = new TypeSet();
            initialValue.type = eventInfo.resultType;
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
                        if (eventInfo.changeType == EventChangeType.ChangeTo)
                            targetValue.intValue = resultInt;
                        else if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.intValue = initialValue.intValue + resultInt;
                        else
                            targetValue.intValue = initialValue.intValue - resultInt;

                        break;
                    case PropertyType.Single:
                        float resultFloat = VM.PopFloat();
                        initialValue.floatValue = VM.PopFloat();
                        if (eventInfo.changeType == EventChangeType.ChangeTo)
                            targetValue.floatValue = resultFloat;
                        else if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.floatValue = initialValue.floatValue + resultFloat;
                        else
                            targetValue.floatValue = initialValue.floatValue - resultFloat;

                        break;
                    case PropertyType.Enum:
                        targetValue.enumValue = VM.PopInt();
                        initialValue.enumValue = VM.PopInt();
                        break;
                    case PropertyType.Vector2:
                        Vector2 resultVector2 = VM.PopVector2();
                        initialValue.vector2Value = VM.PopVector2();
                        if (eventInfo.changeType == EventChangeType.ChangeTo)
                            targetValue.vector2Value = resultVector2;
                        else if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.vector2Value = initialValue.vector2Value + resultVector2;
                        else
                            targetValue.vector2Value = initialValue.vector2Value - resultVector2;

                        break;
                    case PropertyType.RGB:
                        RGB resultRGB = VM.PopRGB();
                        initialValue.rgbValue = VM.PopRGB();
                        if (eventInfo.changeType == EventChangeType.ChangeTo)
                            targetValue.rgbValue = resultRGB;
                        else if (eventInfo.changeType == EventChangeType.Increase)
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
                        if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.intValue = initialValue.intValue + targetValue.intValue;
                        else if (eventInfo.changeType == EventChangeType.Decrease)
                            targetValue.intValue = initialValue.intValue - targetValue.intValue;

                        break;
                    case PropertyType.Single:
                        initialValue.floatValue = VM.PopFloat();
                        if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.floatValue = initialValue.floatValue + targetValue.floatValue;
                        else if (eventInfo.changeType == EventChangeType.Decrease)
                            targetValue.floatValue = initialValue.floatValue - targetValue.floatValue;

                        break;
                    case PropertyType.Enum:
                        initialValue.enumValue = VM.PopInt();
                        break;
                    case PropertyType.Vector2:
                        initialValue.vector2Value = VM.PopVector2();
                        if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.vector2Value = initialValue.vector2Value + targetValue.vector2Value;
                        else if (eventInfo.changeType == EventChangeType.Decrease)
                            targetValue.vector2Value = initialValue.vector2Value - targetValue.vector2Value;

                        break;
                    case PropertyType.RGB:
                        initialValue.rgbValue = VM.PopRGB();
                        if (eventInfo.changeType == EventChangeType.Increase)
                            targetValue.rgbValue = initialValue.rgbValue + targetValue.rgbValue;
                        else if (eventInfo.changeType == EventChangeType.Decrease)
                            targetValue.rgbValue = initialValue.rgbValue - targetValue.rgbValue;

                        break;
                    case PropertyType.String:
                        initialValue.stringValue = VM.PopString();
                        break;
                }
            }
            executor.InitialValue = initialValue;
            executor.TargetValue = targetValue;
            executor.Update();
            executorList.Add(executor);
        }
        public static bool ExecuteSpecialEvent(PropertyContainer propertyContainer, string eventName, string[] arguments,
            VMInstruction[] argumentExpression)
        {
            return SpecialEvents[eventName](propertyContainer, arguments, argumentExpression);
        }
        public static void Update()
        {
            for (int i = 0; i < executorList.Count; ++i)
            {
                if (executorList[i].BindingContainer == null)
                {
                    if (executorList[i].Finished)
                    {
                        executorList.RemoveAt(i);
                        --i;
                    }
                    else
                        executorList[i].Update();
                }
            }
        }
        public static bool BindingUpdate(PropertyContainer propertyContainer, PropertyContainer bindingContainer)
        {
            bool updated = false;
            string id = GetUniqueKey(propertyContainer, bindingContainer);
            for (int i = 0; i < executorList.Count; ++i)
            {
                if (executorList[i].PropertyContainer == propertyContainer && executorList[i].BindingContainer == bindingContainer)
                {
                    if (!cache.ContainsKey(id))
                    {
                        cache.Add(id, new Dictionary<string, TypeSet>());
                    }
                    if (!executorList[i].Finished)
                    {
                        executorList[i].Update();
                    }
                    cache[id][executorList[i].PropertyName] = executorList[i].CurrentValue;
                    if (executorList[i].Finished)
                    {
                        executorList.RemoveAt(i);
                        --i;
                    }
                    updated = true;
                }
            }
            return updated;
        }
        public static bool BindingRecover(PropertyContainer propertyContainer, PropertyContainer bindingContainer)
        {
            string id = GetUniqueKey(propertyContainer, bindingContainer);
            if (!cache.ContainsKey(id))
                return false;

            foreach (var item in cache[id])
            {
                switch (item.Value.type)
                {
                    case PropertyType.Boolean:
                        VM.PushBool(item.Value.boolValue);
                        break;
                    case PropertyType.Int32:
                        VM.PushFloat(item.Value.intValue);
                        break;
                    case PropertyType.Single:
                        VM.PushFloat(item.Value.floatValue);
                        break;
                    case PropertyType.Enum:
                        VM.PushInt(item.Value.enumValue);
                        break;
                    case PropertyType.Vector2:
                        VM.PushVector2(item.Value.vector2Value);
                        break;
                    case PropertyType.RGB:
                        VM.PushRGB(item.Value.rgbValue);
                        break;
                    case PropertyType.String:
                        VM.PushString(item.Value.stringValue);
                        break;
                }
                propertyContainer.SetProperty(item.Key);
            }
            return true;
        }
        private static string GetUniqueKey(PropertyContainer propertyContainer, PropertyContainer bindingContainer)
        {
            return (propertyContainer as Component).ID + "_" + (bindingContainer as ParticleBase).ID;
        }
    }
}
