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
    public class EventExecutorPool : PoolObject<EventExecutorPool, NullData>
    {
        public EventExecutor Instance { get; private set; }
        public EventExecutorPool()
        {
            Instance = new EventExecutor();
            Instance.PoolObject = this;
        }
    }
    public static class EventManager
    {
        public static readonly Dictionary<string, Func<PropertyContainer, VMInstruction[][], float, bool>> SpecialEvents =
            new Dictionary<string, Func<PropertyContainer, VMInstruction[][], float, bool>>()
            {
                { "EmitParticle", (pc, expr, frameScale) =>
                {
                    (pc as Emitter)?.EmitParticle(frameScale);
                    return false;
                } },
                { "PlaySound", (pc, expr, frameScale) =>
                {
                    if (OnSoundPlay != null)
                    {
                        VM.Execute(pc, expr[0], frameScale);
                        var label = VM.PopString();
                        var sound = Sounds.FirstOrDefault((item) => string.Equals(item.Label, label));
                        if (sound != null) OnSoundPlay(sound.AbsolutePath);
                    }
                    return false;
                } },
                { "Loop", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    if (!VM.PopBool()) return true;
                    return false;
                } },
                { "ChangeType", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    int typeId = VM.PopInt();
                    if (typeId >= ParticleType.DefaultTypeIndex)
                    {
                        if (pc is Emitter) (pc as Emitter).Template.Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        else if (pc is ParticleBase) (pc as ParticleBase).Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                    }
                    else if (typeId < CustomTypes.Count)
                    {
                        if (pc is Emitter) (pc as Emitter).Template.Type = CustomTypes[typeId];
                        else if (pc is ParticleBase) (pc as ParticleBase).Type = CustomTypes[typeId];
                    }
                    return false;
                } },
                { "IncreaseType", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    int args0 = VM.PopInt();
                    if (pc is Emitter)
                    {
                        int typeId = (pc as Emitter).Template.Type.ID + args0;
                        if (typeId >= ParticleType.DefaultTypeIndex)
                        {
                            (pc as Emitter).Template.Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        }
                        else if (typeId < CustomTypes.Count)
                        {
                            (pc as Emitter).Template.Type = CustomTypes[typeId];
                        }
                    }
                    else if (pc is ParticleBase)
                    {
                        int typeId = (pc as ParticleBase).Type.ID + args0;
                        if (typeId >= ParticleType.DefaultTypeIndex)
                        {
                            (pc as ParticleBase).Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        }
                        else if (typeId < CustomTypes.Count)
                        {
                            (pc as ParticleBase).Type = CustomTypes[typeId];
                        }
                    }
                    return false;
                } },
                { "DecreaseType", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    int args0 = VM.PopInt();
                    if (pc is Emitter)
                    {
                        int typeId = (pc as Emitter).Template.Type.ID - args0;
                        if (typeId >= 0 && typeId >= ParticleType.DefaultTypeIndex)
                        {
                            (pc as Emitter).Template.Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        }
                        else if (typeId >= 0 && typeId < CustomTypes.Count)
                        {
                            (pc as Emitter).Template.Type = CustomTypes[typeId];
                        }
                    }
                    else if (pc is ParticleBase)
                    {
                        int typeId = (pc as ParticleBase).Type.ID - args0;
                        if (typeId >= ParticleType.DefaultTypeIndex)
                        {
                            (pc as ParticleBase).Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        }
                        else if (typeId >= 0 && typeId < CustomTypes.Count)
                        {
                            (pc as ParticleBase).Type = CustomTypes[typeId];
                        }
                    }
                    return false;
                } },
                { "GotoFrame", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    int args0 = VM.PopInt();
                    VM.Execute(pc, expr[1], frameScale);
                    int args1 = VM.PopInt();
                    if (args0 > 0 && pc is Component)
                    {
                        var component = pc as Component;
                        if (args1 == 0 || args1 >= component.System.FrameSkipCount)
                        {
                            component.System.CurrentFrame = args0;
                            component.System.FrameSkipCount++;
                        }
                    }
                    return false;
                } },
                { "QuakeScreen", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    int args0 = VM.PopInt();
                    VM.Execute(pc, expr[1], frameScale);
                    int args1 = VM.PopInt();
                    pc.System.ShakeScreen(args1, args0);
                    return false;
                } },
                { "StopScreen", (pc, expr, frameScale) =>
                {
                    VM.Execute(pc, expr[0], frameScale);
                    int args0 = VM.PopInt();
                    VM.Execute(pc, expr[1], frameScale);
                    int args1 = VM.PopInt();
                    pc.System.ScaleFrame(args1, args0);
                    return false;
                } },
                { "Recover", (pc, expr, frameScale) =>
                {
                    (pc as Component)?.Reset();
                    return false;
                } },
            };
        public static Func<string, PropertyContainer, VMInstruction[][], float, bool> OnFunctionCall;
        public delegate void SoundPlayHandler(string path);
        public static event SoundPlayHandler OnSoundPlay;
        public static bool CanSoundPlay => OnSoundPlay != null;

        static List<EventExecutor> executorList;
        static Dictionary<long, Dictionary<int, TypeSet>> cache;
        public static GenericContainer<ParticleType> CustomTypes { get; set; }
        public static GenericContainer<FileResource> Sounds { get; set; }
        public static Dictionary<int, int> TypeSoundMap { get; set; }
        public static void Initialize()
        {
            OnSoundPlay = null;
            executorList = new List<EventExecutor>();
            cache = new Dictionary<long, Dictionary<int, TypeSet>>();
        }
        public static void AddEvent(PropertyContainer propertyContainer, PropertyContainer bindingContainer, VMEventInfo eventInfo, 
            float frameScale)
        {
            var executor = EventExecutorPool.Rent(NullData.Empty).Instance;
            executor.Reset();
            executor.PropertyContainer = propertyContainer;
            executor.BindingContainer = bindingContainer;
            executor.PropertyID = eventInfo.resultPropertyID;
            executor.ChangeMode = eventInfo.changeMode;
            executor.ChangeTime = eventInfo.changeTime;
            propertyContainer.PushProperty(executor.PropertyID);
            var initialValue = new TypeSet();
            initialValue.type = eventInfo.resultType;
            var targetValue = eventInfo.resultValue;
            if (eventInfo.isExpressionResult)
            {
                VM.Execute(propertyContainer, eventInfo.resultExpression, frameScale);
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
            executor.Update(frameScale);
            executorList.Add(executor);
        }
        public static bool ExecuteSpecialEvent(PropertyContainer propertyContainer, string eventName, 
            VMInstruction[][] argumentExpressions, float frameScale)
        {
            if (!SpecialEvents.ContainsKey(eventName))
            {
                if (OnFunctionCall!= null) return OnFunctionCall.Invoke(eventName, propertyContainer, argumentExpressions, frameScale);
                else return false;
            }
            return SpecialEvents[eventName](propertyContainer, argumentExpressions, frameScale);
        }
        public static void Update(float frameRate)
        {
            for (int i = 0; i < executorList.Count; ++i)
            {
                if (executorList[i].BindingContainer == null)
                {
                    if (executorList[i].Finished)
                    {
                        EventExecutorPool.Return(executorList[i].PoolObject);
                        executorList.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        var frameScale = executorList[i].PropertyContainer.System.FrameFactor *
                            ParticleSystem.FRAME_RATE_BASE / frameRate;
                        executorList[i].Update(frameScale);
                    }
                }
            }
        }
        public static bool BindingUpdate(PropertyContainer propertyContainer, PropertyContainer bindingContainer, float frameScale)
        {
            bool updated = false;
            long id = GetUniqueKey(propertyContainer, bindingContainer);
            for (int i = 0; i < executorList.Count; ++i)
            {
                if (executorList[i].PropertyContainer == propertyContainer && executorList[i].BindingContainer == bindingContainer)
                {
                    if (!cache.ContainsKey(id))
                    {
                        cache.Add(id, new Dictionary<int, TypeSet>());
                    }
                    if (!executorList[i].Finished)
                    {
                        executorList[i].Update(frameScale);
                    }
                    cache[id][executorList[i].PropertyID] = executorList[i].CurrentValue;
                    if (executorList[i].Finished)
                    {
                        EventExecutorPool.Return(executorList[i].PoolObject);
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
            long id = GetUniqueKey(propertyContainer, bindingContainer);
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
                VM.Clear();
            }
            return true;
        }
        private static long GetUniqueKey(PropertyContainer propertyContainer, PropertyContainer bindingContainer)
        {
            return (propertyContainer as Component).ID * ParticleManager.MaximumParticleCount + (bindingContainer as ParticleBase).ID;
        }
        public static void PlaySound(string path)
        {
            OnSoundPlay?.Invoke(path);
        }
    }
}
