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
                        var sound = pc.System.Sounds?.FirstOrDefault((item) => string.Equals(item.Label, label));
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
                    else if (typeId < pc.System.CustomTypes.Count)
                    {
                        if (pc is Emitter) (pc as Emitter).Template.Type = pc.System.CustomTypes[typeId];
                        else if (pc is ParticleBase) (pc as ParticleBase).Type = pc.System.CustomTypes[typeId];
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
                        else if (typeId < pc.System.CustomTypes.Count)
                        {
                            (pc as Emitter).Template.Type = pc.System.CustomTypes[typeId];
                        }
                    }
                    else if (pc is ParticleBase)
                    {
                        int typeId = (pc as ParticleBase).Type.ID + args0;
                        if (typeId >= ParticleType.DefaultTypeIndex)
                        {
                            (pc as ParticleBase).Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        }
                        else if (typeId < pc.System.CustomTypes.Count)
                        {
                            (pc as ParticleBase).Type = pc.System.CustomTypes[typeId];
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
                        else if (typeId >= 0 && typeId < pc.System.CustomTypes.Count)
                        {
                            (pc as Emitter).Template.Type = pc.System.CustomTypes[typeId];
                        }
                    }
                    else if (pc is ParticleBase)
                    {
                        int typeId = (pc as ParticleBase).Type.ID - args0;
                        if (typeId >= ParticleType.DefaultTypeIndex)
                        {
                            (pc as ParticleBase).Type = ParticleType.DefaultTypes[typeId - ParticleType.DefaultTypeIndex];
                        }
                        else if (typeId >= 0 && typeId < pc.System.CustomTypes.Count)
                        {
                            (pc as ParticleBase).Type = pc.System.CustomTypes[typeId];
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
        public static void Initialize()
        {
            OnSoundPlay = null;
            executorList = new List<EventExecutor>();
            cache = new Dictionary<long, Dictionary<int, TypeSet>>();
        }
        public static void AddEvent(PropertyContainer propertyContainer, PropertyContainer bindingContainer, VMEventInfo eventInfo, 
            float frameScale)
        {
            var executor = EventExecutor.Rent(NullData.Empty);
            executor.Reset();
            executor.UniqueID = GetUniqueKey(propertyContainer.System.InstancedID, propertyContainer, bindingContainer);
            executor.PropertyContainer = propertyContainer;
            executor.PropertyContainerID = propertyContainer.ID;
            executor.BindingContainer = bindingContainer;
            executor.BindingContainerID = bindingContainer != null ? bindingContainer.ID : 0;
            executor.PropertyID = eventInfo.resultPropertyID;
            executor.ChangeMode = eventInfo.changeMode;
            executor.ChangeType = eventInfo.changeType;
            executor.ChangeTime = eventInfo.changeTime;
            var initialValue = new TypeSet();
            var targetValue = eventInfo.resultValue;
            targetValue.type = eventInfo.resultType;
            if (eventInfo.isExpressionResult)
            {
                VM.Execute(propertyContainer, eventInfo.resultExpression, frameScale);
                switch (eventInfo.resultType)
                {
                    case PropertyType.Boolean:
                        targetValue.boolValue = VM.PopBool();
                        break;
                    case PropertyType.Int32:
                        targetValue.intValue = (int)VM.PopFloat();
                        break;
                    case PropertyType.Single:
                        targetValue.floatValue = VM.PopFloat();
                        break;
                    case PropertyType.Enum:
                        targetValue.enumValue = VM.PopInt();
                        break;
                    case PropertyType.Vector2:
                        targetValue.vector2Value = VM.PopVector2();
                        break;
                    case PropertyType.RGB:
                        targetValue.rgbValue = VM.PopRGB();
                        break;
                    case PropertyType.String:
                        targetValue.stringValue = VM.PopString();
                        break;
                }
            }
            if (executor.ChangeMode == EventChangeMode.Sin || executor.ChangeMode == EventChangeMode.Cos)
            {
                propertyContainer.PushProperty(executor.PropertyID);
                switch (eventInfo.resultType)
                {
                    case PropertyType.Boolean:
                        initialValue.boolValue = VM.PopBool();
                        break;
                    case PropertyType.Int32:
                        initialValue.intValue = VM.PopInt();
                        if (executor.ChangeType == EventChangeType.ChangeTo) targetValue.intValue -= initialValue.intValue;
                        break;
                    case PropertyType.Single:
                        initialValue.floatValue = VM.PopFloat();
                        if (executor.ChangeType == EventChangeType.ChangeTo) targetValue.floatValue -= initialValue.floatValue;
                        break;
                    case PropertyType.Enum:
                        initialValue.enumValue = VM.PopInt();
                        break;
                    case PropertyType.Vector2:
                        initialValue.vector2Value = VM.PopVector2();
                        if (executor.ChangeType == EventChangeType.ChangeTo) targetValue.vector2Value -= initialValue.vector2Value;
                        break;
                    case PropertyType.RGB:
                        initialValue.rgbValue = VM.PopRGB();
                        if (executor.ChangeType == EventChangeType.ChangeTo) targetValue.rgbValue -= initialValue.rgbValue;
                        break;
                    case PropertyType.String:
                        initialValue.stringValue = VM.PopString();
                        break;
                }
                executor.InitialValue = initialValue;
                executor.TargetValue = targetValue;
                executor.Update(frameScale);
            }
            else
            {
                executor.TargetValue = targetValue;
                if (executor.ChangeType != EventChangeType.ChangeTo) executor.Update(frameScale);
            }
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
                if (executorList[i].Finished || executorList[i].Invalid)
                {
                    if (executorList[i].Invalid) cache.Remove(executorList[i].UniqueID);
                    EventExecutor.Return(executorList[i]);
                    executorList.RemoveAt(i);
                    --i;
                }
                else if (executorList[i].BindingContainer == null)
                {
                    var frameScale = executorList[i].PropertyContainer.System.FrameFactor *
                        ParticleSystem.FRAME_RATE_BASE / frameRate;
                    executorList[i].Update(frameScale);
                }
            }
        }
        public static bool BindingUpdate(long id, Component component, ParticleBase particle, float frameScale)
        {
            bool updated = false;
            for (int i = 0; i < executorList.Count; ++i)
            {
                if (executorList[i].PropertyContainer != component || executorList[i].BindingContainer != particle) continue;
                if (executorList[i].Finished || executorList[i].Invalid) continue;
                if (!cache.ContainsKey(id)) cache.Add(id, new Dictionary<int, TypeSet>());
                executorList[i].Update(frameScale);
                cache[id][executorList[i].PropertyID] = executorList[i].CurrentValue;
                updated = true;
            }
            return updated;
        }
        public static bool BindingRecover(long id, Component component)
        {
            if (!cache.ContainsKey(id)) return false;
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
                component.SetProperty(item.Key);
                VM.Clear();
            }
            return true;
        }
        public static long GetUniqueKey(int systemHash, PropertyContainer propertyContainer, PropertyContainer bindingContainer)
        {
            if (bindingContainer == null) return -1;
            else return systemHash * propertyContainer.ID * ParticleManager.MaximumParticleCount * 10 + bindingContainer.ID;
        }
        public static void ClearParticleCache(long id)
        {
            long elementID = -1;
            foreach (var kv in cache)
            {
                var key = kv.Key;
                var idPart = key % (ParticleManager.MaximumParticleCount * 10);
                if (idPart == id)
                {
                    elementID = key;
                    break;
                }
            }
            cache.Remove(elementID);
        }
        public static void PlaySound(string path)
        {
            OnSoundPlay?.Invoke(path);
        }
    }
}
